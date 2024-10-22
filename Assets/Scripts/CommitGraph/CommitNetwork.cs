using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public class CommitNetwork : ICommitMapComponent
{
    // private const int MaxNodesSqrt = 1024;
    private const int MaxNodesSqrt = 2048;

    private const int Resolution = 1024 * 4;
    // private const int Resolution = 1024 * 1;
    private const int GroupSize = 8;
    private const float DefaultLength = 3f;

    private ComputeShader CommitNetworkShader;
    public RawImage Image;
    public RectTransform RectTransform;
    public RenderTexture Texture { get; private set; }
    private CommitNode[] Nodes;
    private ComputeBuffer NodeBuffer;
    private int NumNodes;
    public struct CommitNode
    {
        public float x;
        public float y;

        public float TargetX;
        public float TargetY;
        
        public int ParentIndex;

        public float OffsetAngle;
        public float AbsoluteAngle;
        public float Length;

        public int Depth;
        public int BranchIndex;

        public float RandSeed;
    }
    public struct Vector3Default
    {
        public float x;
        public float y;
        public float z;
    }

    private int CurrentEmitIdx;
    private static int StructSize = Marshal.SizeOf(typeof(CommitNode));

    //TODO Add History Merging
    private int IdxWithinTraversal;


    private const int MaxBranches = 1024;
    public const int MaxRequestNodes = 3; // LoadedNode, SelectedNode, RootOfSet
    private float Opacity;
    private enum NodeRequestType
    {
        LoadedNode,
        SelectedNode,
        RootOfSet
    }

    private Dictionary<int, int> IdxToRequestMapping;
    private Vector3Default[] ReturnedNodesArray;
    private ComputeBuffer NodeToAdd;
    private ComputeBuffer NodesToAdd;
    private ComputeBuffer RequestedNodes;
    private ComputeBuffer ReturnedNodes;

    private int UpdateKernel;
    private int DetectClickKernel;
    private int BatchNodeAddKernel;
    private Queue<int> AvailableBranches;
    private int CurrentBranchIdx;
    private ComputeBuffer BranchClickBuffer;

    public HistoryNode SelectedHistoryNode { get; private set; }
    public HistoryNode LoadedHistoryNode { get; private set; }

    public List<HistoryNode> SelectedHistoryNodesToRender { get; private set; }
    public List<HistoryNode> BoardGeneratedNodes { get; private set; }
    private List<long> RenderedHashes;

    // private bool[] BranchClickMatrix; //TODO Eventually do a matrix if overlaps cause too many issues.

    private Dictionary<int, List<HistoryNode>> Histories;

    private float DataFetch;

    private int[] NodeRequest;

    public GameObject CommitNetworkImage { get; private set; }
    public GameObject CommitNetworkImageBackground { get; private set; }

    private Vector2 CommitNetworkSizeBounds;

    private CommitGraph CommitGraph;
    public void Instantiate(GameObject canvas, CommitGraph commitGraph)
    {
        CommitGraph = commitGraph;

        CommitNetworkImageBackground = new GameObject("CommitNetworkBackground");
        CommitNetworkImageBackground.transform.SetParent(canvas.transform, false);
        CommitNetworkImageBackground.AddComponent<RawImage>();
        RectTransform BackgroundRectTransform = CommitNetworkImageBackground.GetComponent<RectTransform>();

        CommitNetworkImage = new GameObject("CommitNetwork");
        CommitNetworkImage.transform.SetParent(canvas.transform, false);
        CommitNetworkImage.AddComponent<RawImage>();
        Image = CommitNetworkImage.GetComponent<RawImage>();
        RectTransform = Image.GetComponent<RectTransform>();
        
        Rect canvasComponent = canvas.GetComponent<RectTransform>().rect;
        // seems that scale is screen scaled while position is canvas scaled.

        float rectTransformSize = canvasComponent.width / 2;
        if (rectTransformSize > canvasComponent.height)
            rectTransformSize = canvasComponent.height;

        CommitNetworkSizeBounds = new Vector2(rectTransformSize, rectTransformSize * 6f);
        RectTransform.sizeDelta = new Vector2(rectTransformSize, rectTransformSize);
        RectTransform.anchoredPosition = new Vector2(0, 0);
        RectTransform.localPosition = new Vector3(-canvasComponent.width / 4, 0, 0);

        BackgroundRectTransform.sizeDelta = RectTransform.sizeDelta;
        BackgroundRectTransform.anchoredPosition = RectTransform.anchoredPosition;
        BackgroundRectTransform.localPosition = RectTransform.localPosition;

        RectTransform.sizeDelta *= 2;

        CommitNetworkShader = Resources.Load<ComputeShader>("Compute/CommitNetwork"); 
        UpdateKernel = CommitNetworkShader.FindKernel("Update");
        DetectClickKernel = CommitNetworkShader.FindKernel("DetectClick");
        BatchNodeAddKernel = CommitNetworkShader.FindKernel("BatchNodeAdd");

        Histories = new Dictionary<int, List<HistoryNode>>();

        Nodes = new CommitNode[MaxNodesSqrt * MaxNodesSqrt];
        CurrentEmitIdx = 0;

        Nodes[0].ParentIndex = -1;

        NodeRequest = new int[MaxRequestNodes];


        NodeBuffer = new ComputeBuffer(Nodes.Length, StructSize);
        NodeBuffer.SetData(Nodes);
        CommitNetworkShader.SetBuffer(UpdateKernel, "nodes", NodeBuffer);
        CommitNetworkShader.SetBuffer(DetectClickKernel, "nodes", NodeBuffer);
        CommitNetworkShader.SetBuffer(BatchNodeAddKernel, "nodes", NodeBuffer);

        NodeToAdd = new ComputeBuffer(1, StructSize);
        CommitNetworkShader.SetBuffer(UpdateKernel, "NodeToAdd", NodeToAdd);

        NodesToAdd = new ComputeBuffer(MaxNodesSqrt * 8, StructSize); // ! Arbitrary Max Nodes
        CommitNetworkShader.SetBuffer(BatchNodeAddKernel, "NodesToAdd", NodesToAdd);


        BranchClickBuffer = new ComputeBuffer(1, sizeof(int));
        CommitNetworkShader.SetBuffer(DetectClickKernel, "BranchClickBuffer", BranchClickBuffer);

        ReturnedNodesArray = new Vector3Default[MaxRequestNodes];
        RequestedNodes = new ComputeBuffer(MaxRequestNodes, sizeof(int));
        ReturnedNodes = new ComputeBuffer(MaxRequestNodes, sizeof(float) * 3);
        CommitNetworkShader.SetBuffer(UpdateKernel, "RequestedNodes", RequestedNodes);
        CommitNetworkShader.SetBuffer(UpdateKernel, "ReturnedNodes", ReturnedNodes);

        // int resolution = 1024;
        Texture = new RenderTexture(Resolution, Resolution, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear){
            enableRandomWrite = true
        };
        Texture.Create();
        CommitNetworkShader.SetTexture(UpdateKernel, "Result", Texture);
        CommitNetworkShader.SetTexture(DetectClickKernel, "Result", Texture);

        CommitNetworkShader.SetInt("Resolution", Resolution);
        UpdateScrollScale();


        AvailableBranches = new Queue<int>();
        for (int i = 0; i < MaxBranches; i++)
            AvailableBranches.Enqueue(i);

        CurrentBranchIdx = -1;

        Image.texture = Texture;

        SelectedHistoryNodesToRender = new();
    }

    // TODO only fetch root node, then recalculate the positions from root node position. Then it should be less jittery.
    private void UpdateScrollScale() => CommitNetworkShader.SetFloat("ScrollScale", RectTransform.sizeDelta.x / CommitNetworkSizeBounds.x);
    public void Update()
    {
        DataFetch += Time.deltaTime;
        if (DataFetch > 0.0f){
            DataFetch = 0;
            ReturnedNodes.GetData(ReturnedNodesArray);
            ReCalculateRenderNodesPositions();
        }
        
        if (CanAddNextBranch()){
            HistoryNode historyNode = SaveFile.LoadNetworkGraph(Histories.Count + 1);
            AddBranch(historyNode);
        }

        List<HistoryNode> CurrentTreeTraversal = Histories[CurrentBranchIdx];
        if (IdxWithinTraversal >= CurrentTreeTraversal.Count){
            CommitNetworkShader.SetInt("CurrentEmitIdx", -1);
        }else{
            AddHistoryNode(CurrentTreeTraversal[IdxWithinTraversal], CurrentBranchIdx);
            NodeToAdd.SetData(new CommitNode[]{Nodes[CurrentEmitIdx]});
            CommitNetworkShader.SetInt("CurrentEmitIdx", CurrentEmitIdx);

            IdxWithinTraversal++;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, Input.mousePosition, null, out Vector2 localPoint);
        Vector2 uv = CommitGraph.CommitGraphMouseUV();
        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1){
            if(Input.mouseScrollDelta.y != 0){
                RectTransform.sizeDelta += new Vector2(Input.mouseScrollDelta.y, Input.mouseScrollDelta.y) * 3;
                RectTransform.sizeDelta = new Vector2(Mathf.Clamp(RectTransform.sizeDelta.x, CommitNetworkSizeBounds.x, CommitNetworkSizeBounds.y), Mathf.Clamp(RectTransform.sizeDelta.y, CommitNetworkSizeBounds.x, CommitNetworkSizeBounds.y));
                UpdateScrollScale();
            }
            if(Input.GetMouseButtonDown(0)){
                float scale = Resolution / RectTransform.rect.width;
                localPoint *= scale;
                CommitNetworkShader.SetFloat("MouseX", localPoint.x);
                CommitNetworkShader.SetFloat("MouseY", localPoint.y);
                BranchClickBuffer.SetData(new int[]{-1});
                CommitNetworkShader.Dispatch(DetectClickKernel, MaxNodesSqrt/GroupSize, MaxNodesSqrt/GroupSize, 1);

                int[] clickIdx = new int[1];
                BranchClickBuffer.GetData(clickIdx);
                int idx = clickIdx[0];

                if (idx != -1){
                    List<HistoryNode> selectedTraversal = Histories[Nodes[idx].BranchIndex];
                    for (int i = 0; i < selectedTraversal.Count; i++){
                        if (selectedTraversal[i].IdxWithinNetwork == idx){
                            SelectedHistoryNode = selectedTraversal[i];
                            break;
                        }
                    }
                    if (SelectedHistoryNode != null)
                        UpdateSelectedNode(SelectedHistoryNode);
                }
            }
        }

        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = Texture;
        GL.Clear(false, true, new Color(0, 0, 0, 0));
        RenderTexture.active = rt;

        float deltaTime = Time.deltaTime * 20;
        if (deltaTime > 2.0f) deltaTime = 1.0f; // Kinda weird
        CommitNetworkShader.SetFloat("DeltaTime", deltaTime);
        CommitNetworkShader.Dispatch(UpdateKernel, MaxNodesSqrt/GroupSize, MaxNodesSqrt/GroupSize, 1);
    }
    private HistoryNode Search(long hash){
        //! Optimize this
        foreach (KeyValuePair<int, List<HistoryNode>> pair in Histories){
            foreach (HistoryNode historyNode in pair.Value){
                if (historyNode.Hash == hash)
                    return historyNode;
            }
        }
        return null;
    }

    public bool ForceUpdateLoadedNode(HistoryNode historyNode){
        UpdateSelectedNode(historyNode);
        UpdateLoadedNode();
        return true;
    }
    
    // Updating Loaded Node should just be moving the Loaded Node to the selected node. Kinda-
    public void UpdateLoadedNode(){
        if (SelectedHistoryNode == null)
            return;
        LoadedHistoryNode = SelectedHistoryNode;
        NodeRequest[(int)NodeRequestType.LoadedNode] = LoadedHistoryNode.IdxWithinNetwork;
        if (!RenderedHashes.Contains(LoadedHistoryNode.Hash))
            RenderedHashes.Add(LoadedHistoryNode.Hash);
    }
    public void UpdateSelectedNode(HistoryNode historyNode){
        SelectedHistoryNode = historyNode;
        CommitNetworkShader.SetInt("SelectedBranch", Nodes[SelectedHistoryNode.IdxWithinNetwork].BranchIndex);

        IdxToRequestMapping = new Dictionary<int, int>();
        SelectedHistoryNodesToRender = new();

        NodeRequest[(int)NodeRequestType.SelectedNode] = historyNode.IdxWithinNetwork;


        // (Node, Depth)
        // ! Optimize this
        RenderedHashes = new();
        Stack<(HistoryNode, int)> stack = new();
        stack.Push((historyNode, 9));
        SelectedHistoryNodesToRender.Add(historyNode); // ! this is a bug but if I remove it it breaks
        while(stack.Count > 0){
            (HistoryNode, int) current = stack.Pop();
            HistoryNode currentNode = current.Item1;

            int depth = current.Item2;

            if (depth < 0)
                continue;

            if (currentNode.Parent != null){
                stack.Push((currentNode.Parent, depth - 1));

                if (!RenderedHashes.Contains(currentNode.Parent.Hash)){
                    RenderedHashes.Add(currentNode.Parent.Hash);
                    SelectedHistoryNodesToRender.Insert(0, currentNode.Parent);
                }
            }
            if (currentNode.Children != null){
                // ! There exists a bug where if you traverse backwards to a child branch, it will traverse down it without enough depth.
                // ? to fix both these bugs of order and child branches, maybe simplify the problem by first backtracking depth to root
                // ? then traverse down depth * 2 to get the children.
                foreach (HistoryNode child in currentNode.Children){
                    stack.Push((child, depth - 1));

                    if (!RenderedHashes.Contains(child.Hash)){
                        RenderedHashes.Add(child.Hash);
                        SelectedHistoryNodesToRender.Add(child);
                    }
                }
            }
        }

        // ? if first is root node, then remove it.
        // TODO?
        //! the exists a bug where it's not always in order
        // string debugTest = "";
        // for (int i = 0; i < SelectedHistoryNodesToRender.Count; i++)
        //     debugTest += SelectedHistoryNodesToRender[i].TurnNumber + " ";
        // Debug.Log(debugTest);


        // SelectedHistoryNodesToRender Should be in order now.
        NodeRequest[(int)NodeRequestType.RootOfSet] = SelectedHistoryNodesToRender[0].IdxWithinNetwork;


        // Add the loaded node to render nodes
        if (LoadedHistoryNode != null && !RenderedHashes.Contains(LoadedHistoryNode.Hash))
            RenderedHashes.Add(LoadedHistoryNode.Hash);

        RequestedNodes.SetData(NodeRequest);
        DataFetch = 100;
    }
    public void ReCalculateRenderNodesPositions(){
        // Debug.Log("");
        for (int i = 0; i < RequestedNodes.count; i++){
            // Debug.Log(NodeRequest[i]);
            Nodes[NodeRequest[i]].x = ReturnedNodesArray[i].x;
            Nodes[NodeRequest[i]].y = ReturnedNodesArray[i].y;
            Nodes[NodeRequest[i]].AbsoluteAngle = ReturnedNodesArray[i].z;
        }

        for (int i = 1; i < SelectedHistoryNodesToRender.Count; i++)
            ReCalculateNodePosition(SelectedHistoryNodesToRender[i]);
    }
    private void ReCalculateNodePosition(HistoryNode historyNode){
        CommitNode commitNode = Nodes[historyNode.IdxWithinNetwork];

        CommitNode parentCommitNode = (historyNode.Parent != null) ? Nodes[historyNode.Parent.IdxWithinNetwork] : Nodes[0];
        float parentAngle = parentCommitNode.AbsoluteAngle;

        if (parentCommitNode.ParentIndex != -1)
            commitNode.AbsoluteAngle = parentAngle + commitNode.OffsetAngle;
        // else
        //     Debug.Log("Parent Index is -1");


        Vector2 position = CalculatePosition(commitNode.AbsoluteAngle, commitNode.Length, new Vector2(parentCommitNode.x, parentCommitNode.y));

        commitNode.x = position.x;
        commitNode.y = position.y;
        commitNode.TargetX = commitNode.x;
        commitNode.TargetY = commitNode.y;

        Nodes[historyNode.IdxWithinNetwork] = commitNode;
    }
    private Vector2 CalculatePosition(float angle, float length, Vector2 parentPos) => new Vector2(parentPos.x + Mathf.Cos(angle) * length, parentPos.y + Mathf.Sin(angle) * length);
    public Vector2? GetAbsolutePosition(HistoryNode historyNode){
        if (historyNode == null || RenderedHashes == null || !RenderedHashes.Contains(historyNode.Hash))
            return null;
        int IdxWithinNetwork = historyNode.IdxWithinNetwork;
        for (int i = 0; i < RequestedNodes.count; i++){
            if (NodeRequest[i] == IdxWithinNetwork)
                return new Vector2(ReturnedNodesArray[i].x, ReturnedNodesArray[i].y);
        }
        CommitNode commitNode = Nodes[historyNode.IdxWithinNetwork];
        return new Vector2(commitNode.x, commitNode.y);
    }
    
    public (Vector2, float) CalculatePosition(long hash, Vector2 parentPos, float parentAngle, bool branched){
        float angle = OffsetAngleFromHash(hash, branched) * Mathf.Deg2Rad + parentAngle;
        return (CalculatePosition(angle, DefaultLength, parentPos), angle);
    }
    public Vector2? GetPosition(HistoryNode historyNode){
        if (historyNode == null || RenderedHashes == null || !RenderedHashes.Contains(historyNode.Hash))
            return null;
        CommitNode commitNode = Nodes[historyNode.IdxWithinNetwork];
        return new Vector2(commitNode.x, commitNode.y);
    }
    public float GetAngle(HistoryNode historyNode){
        if (historyNode == null || RenderedHashes == null || !RenderedHashes.Contains(historyNode.Hash))
            return 0;
        return Nodes[historyNode.IdxWithinNetwork].AbsoluteAngle;
    }

    public Vector2 ToScreenSpace(Vector2 TextureSpace){
        Vector2 origin = RectTransform.localPosition;
        Vector2 size = RectTransform.sizeDelta;
        TextureSpace /= Resolution;
        TextureSpace *= size;
        TextureSpace += origin;
        return TextureSpace;
    }

    public bool CanAddNextBranch() => CurrentBranchIdx == -1 || IdxWithinTraversal >= Histories[CurrentBranchIdx].Count && AvailableBranches.Count > 0;
    private float OffsetAngleFromHash(long hash, bool branched){
        if (hash == 0)
            return 0; 
        float randValue = new CoreRandom((int)hash).Range(-1f, 1f);
        if (!branched)
            return randValue * 8;
        return Mathf.Sign(randValue) * (Mathf.Abs(randValue) + 1) * 12;

    }

    private void AddHistoryNode(HistoryNode historyNode, int BranchIdx){
        if (historyNode.Hash == BoardHistory.Root.Hash)
            return;


        CurrentEmitIdx++;
        if (CurrentEmitIdx >= Nodes.Length)
            CurrentEmitIdx = 1;
        historyNode.SetIdxWithinNetwork(CurrentEmitIdx); // While you traverse, you keep updating the turn number

        int ParentIndex = historyNode.Parent?.IdxWithinNetwork ?? 0;
        bool branched = false;
        float OffsetAngle = OffsetAngleFromHash(historyNode.Hash, branched);

        CoreRandom random = new((int)historyNode.Hash);
        if (ParentIndex == 0)
            OffsetAngle = random.Range(0, 359);

        CommitNode commitNode = Nodes[CurrentEmitIdx];
        commitNode.ParentIndex = ParentIndex;
        commitNode.BranchIndex = BranchIdx;
        commitNode.Length = DefaultLength;
        commitNode.OffsetAngle = OffsetAngle * Mathf.Deg2Rad;
        commitNode.RandSeed = random.Range(-1.0f, 1.0f);
        commitNode.Depth = Nodes[ParentIndex].Depth + 1;
        Nodes[CurrentEmitIdx] = commitNode;
    }
    private void AddNodeList(List<HistoryNode> traversal, int BranchIdx){ // turn the traversal into commit nodes and then add them to the buffer
        CommitNode[] commitNodes = new CommitNode[traversal.Count];
        for (int i = 0; i < traversal.Count; i++){
            AddHistoryNode(traversal[i], BranchIdx);
            ReCalculateNodePosition(traversal[i]);
            commitNodes[i] = Nodes[traversal[i].IdxWithinNetwork];
        }

        // ! TODO make the commit nodes increase by factors of 8
        if (NodesToAdd.count < commitNodes.Length){
            NodesToAdd.Release();
            NodesToAdd = new ComputeBuffer(commitNodes.Length, StructSize);
            CommitNetworkShader.SetBuffer(BatchNodeAddKernel, "NodesToAdd", NodesToAdd);
        }
        NodesToAdd.SetData(commitNodes);
        CommitNetworkShader.SetInt("NumNodesToAdd", commitNodes.Length);
        CommitNetworkShader.SetInt("CurrentEmitIdx", CurrentEmitIdx);
        CommitNetworkShader.Dispatch(BatchNodeAddKernel, commitNodes.Length/GroupSize + 1, 1, 1);
    }
    public void ExtendLoadedBranch(HistoryNode historyNode){
        List<HistoryNode> traversal = historyNode.GetTraversal();
        int branchIdx = Nodes[NodeRequest[(int)NodeRequestType.LoadedNode]].BranchIndex;
        Histories[branchIdx].AddRange(traversal);
        AddNodeList(traversal, branchIdx);
        UpdateSelectedNode(SelectedHistoryNode);
    }

    public void ForceAddBranch(HistoryNode historyNode){
        List<HistoryNode> traversal = historyNode.GetTraversal();
        int branchIdx = AvailableBranches.Dequeue();
        Histories.Add(branchIdx, traversal);
        AddNodeList(traversal, branchIdx);
    }
    public void AddBranch(HistoryNode historyNode){
        IdxWithinTraversal = 0;
        CurrentBranchIdx = AvailableBranches.Dequeue();
        Histories.Add(CurrentBranchIdx, historyNode.GetTraversal());
    }


    public void Dispose()
    {
        Texture.Release();
        NodeBuffer.Release();
        BranchClickBuffer.Release();
        RequestedNodes.Release();
        ReturnedNodes.Release();
        NodeToAdd.Release();
        NodesToAdd.Release();
    }

    public void SetOpacity(float opacity){
        Opacity = opacity;   
        CommitNetworkImageBackground.GetComponent<RawImage>().color = new Color(0, 0, 0, opacity);
    }

    public void Show(){
        CommitNetworkImage.SetActive(true);   
        CommitNetworkImageBackground.SetActive(true);
    }

    public void Hide(){
        CommitNetworkImage.SetActive(false);   
        CommitNetworkImageBackground.SetActive(false);
    }
}
