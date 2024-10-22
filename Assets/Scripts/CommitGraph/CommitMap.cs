using UnityEngine;

public class CommitMap
{
    private ICommitMapComponent[] Components;
    public CommitMapBuffer CommitMapBuffer { get { return Components[0] as CommitMapBuffer; } }
    public CommitNetwork CommitNetwork { get { return Components[1] as CommitNetwork; } }
    public CommitGraph CommitGraph { get { return Components[2] as CommitGraph; } }
    public CommitMapArrows CommitMapArrows { get { return Components[3] as CommitMapArrows; } }
    public bool IsActive { get; private set; }


    public CommitMap(GameObject canvas)
    {
        // CommitMapBuffer = new CommitMapBuffer(canvas);
        // CommitNetwork = new CommitNetwork(canvas);
        // CommitGraph = new CommitGraph(CommitNetwork, canvas);

        Components = new ICommitMapComponent[4];
        Components[0] = new CommitMapBuffer();
        Components[1] = new CommitNetwork();
        Components[2] = new CommitGraph();
        Components[3] = new CommitMapArrows();

        CommitMapBuffer.Instantiate(canvas);
        CommitNetwork.Instantiate(canvas, CommitGraph);
        CommitGraph.Instantiate(canvas, CommitNetwork);
        CommitMapArrows.Instantiate(canvas);

        for (int i = 0; i < Components.Length; i++)
            Components[i].SetOpacity(0.9f);
    }

    // public void ForceSelectNode(HistoryNode node)
    // {
    //     // CommitNetwork.
    // }

    public void Update(){
        for (int i = 0; i < Components.Length; i++)
            Components[i].Update();
    }

    public void Destroy(){
        for (int i = 0; i < Components.Length; i++)
            Components[i].Dispose();
    }
    public void Hide(){
        for (int i = 0; i < Components.Length; i++)
            Components[i].Hide();
        IsActive = false;
    }
    public void Show(){
        for (int i = 0; i < Components.Length; i++)
            Components[i].Show();
        IsActive = true;
    }
}
