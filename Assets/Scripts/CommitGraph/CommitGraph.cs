using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CommitGraph : ICommitMapComponent
{
    private float PixelSize = 0.01f;
    private float Radius = 0.54f;
    private int MinZoom = 4;
    private int MaxZoom = 16;
    private CommitNetwork Network;

    private GameObject Parent;
    public GameObject CanvasParent { get; private set;}
    private UILineRender[] Lines; // 4 Box, 4 Leads
    private Vector4 Bounds; // x, y, width, height

    private Camera CommitGraphRenderCamera;
    private RenderTexture RenderedTexture;

    private RawImage RawImage;

    private Rect Rect;

    private LineRendererArrow Arrow;

    public class RenderNode
    {
        public SpriteRenderer SpriteRenderer;
        public LineRendererArrow LineRendererArrow;
        public TextMesh TextMesh;
        public RenderNode(SpriteRenderer spriteRenderer, LineRendererArrow lineRendererArrow, TextMesh textMesh)
        {
            SpriteRenderer = spriteRenderer;
            LineRendererArrow = lineRendererArrow;
            TextMesh = textMesh;
        }
    }
    private List<RenderNode> RenderNodes;
    private List<HistoryNode> HistoryNodes;
    private List<long> BoardStateHashes;
    private Rect CanvasComponent;
    private Font Font;
    public void Instantiate(GameObject canvas, CommitNetwork network)
    {
        Font = Resources.Load<Font>("Misc/Menlo-Regular-World-Space");

        Parent = new GameObject("CommitGraph");
        Parent.transform.position = Vector3.zero;

        CanvasParent = new GameObject("CommitGraph");
        CanvasParent.transform.SetParent(canvas.transform, false);

        RenderedTexture = new RenderTexture(2048, 2048, 32, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear){
            enableRandomWrite = true
        };
        RenderedTexture.Create();

        CommitGraphRenderCamera = new GameObject("CommitGraphRenderCamera").AddComponent<Camera>();
        CommitGraphRenderCamera.transform.SetParent(Parent.transform, false);
        CommitGraphRenderCamera.transform.localPosition = new Vector3(0, 0, -10);
        CommitGraphRenderCamera.orthographicSize = MaxZoom;
        CommitGraphRenderCamera.backgroundColor = new Color(0, 0, 0, 0.9f);
        CommitGraphRenderCamera.clearFlags = CameraClearFlags.SolidColor;
        CommitGraphRenderCamera.cullingMask = 1 << LayerMask.NameToLayer("CommitGraph");
        CommitGraphRenderCamera.targetTexture = RenderedTexture;

        CommitGraphRenderCamera.orthographic = true;

        RawImage = new GameObject("RawImage").AddComponent<RawImage>();
        RawImage.transform.SetParent(CanvasParent.transform, false);
        RawImage.texture = RenderedTexture;
        RectTransform RectTransform = RawImage.GetComponent<RectTransform>();

        CanvasComponent = canvas.GetComponent<RectTransform>().rect;

        float rectTransformSize = CanvasComponent.width / 2;
        if (rectTransformSize > CanvasComponent.height)
            rectTransformSize = CanvasComponent.height;
        RectTransform.sizeDelta = new Vector2(rectTransformSize, rectTransformSize);
        RectTransform.anchoredPosition = new Vector2(0, 0);
        RectTransform.localPosition = new Vector3(CanvasComponent.width / 4, 0, 0);

        Network = network;
        UpdateBounds();


        Lines = new UILineRender[12];
        for (int i = 0; i < Lines.Length; i++)
        {
            GameObject newLine = new GameObject("Line" + i);
            newLine.transform.SetParent(CanvasParent.transform, false);
            RectTransform lineTransform = newLine.AddComponent<RectTransform>();
            lineTransform.sizeDelta = new Vector2(1, 1);
            lineTransform.localPosition = Vector3.zero;

            Lines[i] = new UILineRender(lineTransform);
        }
        for (int i = 0; i < 4; i++){
            float multiplier = (i == 0 || i == 3 ) ? 1f : 0.7f;
            Lines[i].SetColor(new Color(multiplier, multiplier, multiplier, 1));
        }
        for (int i = 4; i < 8; i++){
            float multiplier = (i == 4 || i == 7 ) ? 1f : 0.7f;
            Lines[i].SetColor(new Color(0.6f * multiplier, 0.6f * multiplier, 0.6f * multiplier, 1));
        }

        RawImage.transform.SetSiblingIndex(CanvasParent.transform.childCount - 1 - 4);

        Rect = new(RectTransform.localPosition.x, RectTransform.localPosition.y, RectTransform.sizeDelta.x, RectTransform.sizeDelta.y);
        DrawBox(Rect);

        RenderNodes = new List<RenderNode>();
        BoardStateHashes = new List<long>();
    }
    
    public Vector2 CommitGraphMouseUV()
    {
        Vector2 mousePositionInCanvas = new Vector2(Input.mousePosition.x / Screen.width * CanvasComponent.width, Input.mousePosition.y / Screen.height * CanvasComponent.height);
        Vector2 uvInRect = mousePositionInCanvas - (GetBounds(Rect)[3] + new Vector2(CanvasComponent.width / 2, CanvasComponent.height / 2));
        uvInRect.x /= Rect.width;
        uvInRect.y /= Rect.height;
        return uvInRect;
    }
    public void Update()
    {
        Vector2 uvInRect = CommitGraphMouseUV();
        Vector2 fakeScreenPosition = new Vector2(uvInRect.x * CommitGraphRenderCamera.pixelWidth, uvInRect.y * CommitGraphRenderCamera.pixelHeight);
        Vector2 worldPosition = CommitGraphRenderCamera.ScreenToWorldPoint(fakeScreenPosition);
        RenderNode mouseOverNode = GetNodeAt(worldPosition);

        // Node click behavior
        if (Input.GetMouseButtonDown(0) && mouseOverNode != null){
            HistoryNode historyNode = null;
            for (int i = 0; i < HistoryNodes.Count; i++)
                if (RenderNodes[i] == mouseOverNode)
                    historyNode = HistoryNodes[i];
            if (historyNode != null){

                // Update selected node or load node
                Network.UpdateSelectedNode(historyNode);
                if ((DoubleClick.RecentlyClicked || Input.GetKey(KeyCode.LeftShift)) && historyNode == Network.SelectedHistoryNode){


                    BoardHistory.TraverseTo(historyNode.Hash);
                    Board.Instance.SetNumUserBoardStates(CommitMapArrows.Instance.NumStepsForwards);

                    Board.Instance.RegenerateDeltas(2);

                    Network.UpdateLoadedNode();
                    CoreLoop.Instance.CleanUp();
                }
            }else{
                // Clicked on a future board, load it or commit it
                int numBoardInFuture = -1;
                for (int i = HistoryNodes.Count; i < RenderNodes.Count; i++){
                    if (RenderNodes[i] == mouseOverNode){
                        numBoardInFuture = i - HistoryNodes.Count;
                    }
                }

                if (DoubleClick.RecentlyClicked || Input.GetKey(KeyCode.LeftShift)){ // commit all nodes til(and including) current
                    numBoardInFuture++;
                    //TODO Make it start at the first new hash.
                    for (int i = 0; i < numBoardInFuture; i++){
                        long boardHash = Board.Instance.GenerateCurrentBoardDelta().Hash;
                        if(BoardHistory.CheckHash(boardHash)){ // New Hash
                            Board.Instance.CommitTurn(false);

                            // if board history only contains the root board, then it needs to be added to the network as a branch.
                            if (BoardHistory.Root.Children[0].Children.Count == 0){
                                Network.ForceAddBranch(BoardHistory.Root);
                                Network.ForceUpdateLoadedNode(BoardHistory.Current);
                            }else{
                                Network.ExtendLoadedBranch(BoardHistory.Current);
                            }


                        }
                    }
                    Network.UpdateSelectedNode(BoardHistory.Current);
                    Network.UpdateLoadedNode();

                    Board.Instance.SetNumUserBoardStates(CommitMapArrows.Instance.NumStepsForwards);
                    CoreLoop.Instance.CleanUp();
                }else{ // load working board
                    Board.Instance.SetWorkingBoard(numBoardInFuture * 2 + 1);
                    CoreLoop.Instance.CleanUp();
                    Network.UpdateSelectedNode(Network.LoadedHistoryNode);
                }
            }
        }

        if (uvInRect.x >= 0 && uvInRect.x <= 1 && uvInRect.y >= 0 && uvInRect.y <= 1){
            if (Input.mouseScrollDelta.y != 0)
            {
                float zoom = CommitGraphRenderCamera.orthographicSize;
                zoom -= Input.mouseScrollDelta.y;
                if (zoom < MinZoom)
                    zoom = MinZoom;
                if (zoom > MaxZoom)
                    zoom = MaxZoom;
                CommitGraphRenderCamera.orthographicSize = zoom;
            }
        }


        if (HistoryNodes != Network.SelectedHistoryNodesToRender || BoardStateHashes != Board.Instance.BoardStateHashes){
            HistoryNodes = Network.SelectedHistoryNodesToRender;
            BoardStateHashes = Board.Instance.BoardStateHashes;

            for (int i = 0; i < RenderNodes.Count; i++)
                GameObject.Destroy(RenderNodes[i].SpriteRenderer.gameObject);
            RenderNodes.Clear();

            int numberRenderedBoardStates = BoardStateHashes.Count;
            if (numberRenderedBoardStates > CommitMapArrows.MaxStepsForwards)
                numberRenderedBoardStates = CommitMapArrows.MaxStepsForwards;

            int totalNodes = HistoryNodes.Count + numberRenderedBoardStates;
            Dictionary<long, int> nodeToIndex = new();
            for (int i = 0; i < totalNodes; i++)
            {
                long hash = i < HistoryNodes.Count ? HistoryNodes[i].Hash : BoardStateHashes[i - HistoryNodes.Count];

                GameObject go = new("CommitNode");
                go.layer = LayerMask.NameToLayer("CommitGraph");
                go.transform.SetParent(Parent.transform);
                go.transform.localScale = new Vector3(0.1f, 0.1f, 1);
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = Resources.Load<Sprite>("Sprites/Misc/NP/CommitNode");
                sr.sortingLayerName = "CommitGraph";
                sr.sortingOrder = 1;

                GameObject textChild = new("Text");
                textChild.transform.SetParent(go.transform, true);
                textChild.transform.localScale = new Vector3(1, 1, 1);
                textChild.transform.localPosition = new Vector3(0, -7, 0);
                textChild.layer = LayerMask.NameToLayer("CommitGraph");

                MeshRenderer textMeshRenderer = textChild.AddComponent<MeshRenderer>();
                textMeshRenderer.material = Font.material;

                TextMesh textMesh = textChild.AddComponent<TextMesh>();
                textMesh.text = SaveUtility.ToHexSubstring(hash);
                textMesh.font = Font;
                textMesh.characterSize = 0.75f;
                textMesh.fontStyle = FontStyle.Bold;
                textMesh.anchor = TextAnchor.UpperCenter;

                GameObject parentGameObject = null;
                if(i < HistoryNodes.Count){
                    if (HistoryNodes[i].Parent != null){
                        long parentHash = HistoryNodes[i].Parent.Hash;
                        if (nodeToIndex.TryGetValue(parentHash, out int parentIndex))
                            parentGameObject = RenderNodes[parentIndex].SpriteRenderer.gameObject;
                    }
                }else{
                    if (i == HistoryNodes.Count){
                        long loadedNodeHash = Network.LoadedHistoryNode.Hash;
                        if (nodeToIndex.TryGetValue(loadedNodeHash, out int parentIndex))
                            parentGameObject = RenderNodes[parentIndex].SpriteRenderer.gameObject;
                    }else{
                        parentGameObject = RenderNodes[i - 1].SpriteRenderer.gameObject;
                    }
                }
                
                LineRendererArrow arrow = parentGameObject == null ? null : new LineRendererArrow(parentGameObject.transform, 0.5f, 0.7f);

                RenderNodes.Add(new RenderNode(sr, arrow, textMesh));
                nodeToIndex[hash] = i;
            }
        }

        Vector2 parentPos = Vector2.zero;
        float parentAngle = 0;
        if(HistoryNodes.Count > 0){
            parentPos = Network.GetPosition(Network.LoadedHistoryNode) ?? Vector2.zero;
            parentAngle = Network.GetAngle(Network.LoadedHistoryNode);
        }

        int workingBoardNum = Board.Instance.WorkingBoard / 2;
        for (int i = 0; i < RenderNodes.Count; i++)
        {
            Vector2? position;
            long hash;

            if (i < HistoryNodes.Count)
            {
                HistoryNode historyNode = HistoryNodes[i];
                position = Network.GetPosition(historyNode);
                hash = historyNode.Hash;
            }else{
                hash = BoardStateHashes[i - HistoryNodes.Count];
                bool branched = i == HistoryNodes.Count && Network.LoadedHistoryNode.Children.Count > 0;
                (parentPos, parentAngle) = Network.CalculatePosition(hash, parentPos, parentAngle, branched);
                position = parentPos;
            }

            if (position == null) // ? maybe instead of leaving set it so a guessed value
                continue;

            RenderNode renderNode = RenderNodes[i];

            float variation = hash % 1000 / 1000f / 5 + 0.8f;

            if (i < HistoryNodes.Count){
                HistoryNode historyNode = HistoryNodes[i];
                if (historyNode == Network.LoadedHistoryNode)
                    renderNode.SpriteRenderer.color = new Color(variation/4, variation, variation/4, 1);
                else if (historyNode == Network.SelectedHistoryNode)
                    renderNode.SpriteRenderer.color = new Color(variation/2, variation/2, variation, 1);
                else
                    renderNode.SpriteRenderer.color = new Color(variation, variation, variation, 1);
            }else{
                if (i - HistoryNodes.Count == workingBoardNum && Network.SelectedHistoryNode == Network.LoadedHistoryNode)
                    renderNode.SpriteRenderer.color = new Color(variation/2, variation/2, variation, 0.4f);
                else
                    renderNode.SpriteRenderer.color = new Color(variation, variation, variation, 0.1f);
            }


            renderNode.SpriteRenderer.transform.position = (Vector2)position;
            renderNode.LineRendererArrow?.UpdateArrow((Vector2)position);
        }

        if(mouseOverNode != null)
            mouseOverNode.SpriteRenderer.color *= 0.8f;

        int idxToFollow = 0;
        if (Network.SelectedHistoryNode != Network.LoadedHistoryNode){
            for (int i = 0; i < HistoryNodes.Count; i++){
                if (HistoryNodes[i] == Network.SelectedHistoryNode){
                    idxToFollow = i;
                    break;
                }
            }
        }
        else{
            idxToFollow = HistoryNodes.Count + workingBoardNum;
        }

        Vector2 followNode = RenderNodes[idxToFollow].SpriteRenderer.transform.position;
        
        CommitGraphRenderCamera.transform.position = new Vector3(followNode.x, followNode.y, -10);


        DrawLeads(Rect, Network.SelectedHistoryNode, 1);
        DrawLeads(Rect, Network.LoadedHistoryNode, 0);
    }
    private RenderNode GetNodeAt(Vector2 position)
    {
        for (int i = 0; i < RenderNodes.Count; i++)
        {
            RenderNode renderNode = RenderNodes[i];
            Vector2 nodePosition = renderNode.SpriteRenderer.transform.position;
            if (Vector2.Distance(nodePosition, position) < Radius)
                return renderNode;
        }
        return null;
    }
    private void UpdateBounds() // half of screenWidth and screenHeight in world space
    {
        float VerticalExtent = Camera.main.orthographicSize * 2;
        float HorizontalExtent = VerticalExtent * Camera.main.aspect;
        Bounds = new Vector4(Camera.main.transform.position.x, Camera.main.transform.position.y, HorizontalExtent, VerticalExtent);
    }
    private Vector2[] GetBounds(Rect Bounds)
    {
        Vector2 topLeft = new(Bounds.x - Bounds.width / 2, Bounds.y + Bounds.height / 2);
        Vector2 topRight = new(Bounds.x + Bounds.width / 2, Bounds.y + Bounds.height / 2);
        Vector2 bottomRight = new(Bounds.x + Bounds.width / 2, Bounds.y - Bounds.height / 2);
        Vector2 bottomLeft = new(Bounds.x - Bounds.width / 2, Bounds.y - Bounds.height / 2);

        return new Vector2[] { topLeft, topRight, bottomRight, bottomLeft };
    }
    private void DrawBox(Rect rect){
        Vector2[] bounds = GetBounds(rect);
        for (int i = 0; i < 4; i++)
            Lines[i + 8].DrawLine(bounds[i], bounds[(i + 1) % 4]);
    }

    private void DrawLeads(Rect rect, HistoryNode node, int number){
        if (node == null)
            return;

        Vector2[] bounds = GetBounds(rect);
        Vector2 selectedNode = Network.GetAbsolutePosition(node) ?? Vector2.zero;
        selectedNode = Network.ToScreenSpace(selectedNode);

        for (int i = 0; i < 4; i++)
            Lines[i + number * 4].DrawLine(selectedNode, bounds[i]);
    }

    public void SetOpacity(float opacity){
        CommitGraphRenderCamera.backgroundColor = new Color(0, 0, 0, opacity);
    }

    public void Show(){
        Parent.SetActive(true);
        CanvasParent.SetActive(true);
    }

    public void Hide()
    {
        Parent.SetActive(false);
        CanvasParent.SetActive(false);
    }

    public void Dispose() {}
}
