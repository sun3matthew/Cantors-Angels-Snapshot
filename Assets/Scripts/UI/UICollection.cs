using UnityEngine;
using UnityEngine.UI;


//! I'm choosing to make this system more hard-coded and centralized. It'll just be a lot more simple and easier to manage.
public class UICollection
{
    public static UICollection Instance { get; private set; } // ! Bad design ig.
    public Canvas Canvas { get; private set; }

    public Text EconomyText { get; private set; }
    public DebugUI DebugUI { get; private set; }
    public GameObject ActionButtonHolder { get; private set; }
    public CommitMap CommitGraphManager { get; private set;}

    public UICollection(Canvas canvas){
        Instance = this;

        Canvas = canvas;

        ParticleSystemComputeCollection.SoftInitialize(canvas.transform);

        DebugUI = new DebugUI(canvas.gameObject);
        DebugUI.Toggle();

        ActionButtonHolder = UIPrefab.CreatePanel("Action Button Holder", canvas.transform).gameObject;

        CommitGraphManager = new CommitMap(Canvas.gameObject);
        CommitGraphManager.Hide();

        EconomyText = UIPrefab.CreateText(
            "Economy",
            Resources.Load<Font>("Misc/Pixeled"),
            canvas.transform,
            new Vector2(0.5f, 1)
        ).Item2;
        EconomyText.text = "Economy";
        EconomyText.alignment = TextAnchor.UpperCenter;
    }

    public void Update(){
        if(CommitGraphManager.IsActive)
            CommitGraphManager.Update(); // !Only update on active
        DebugUI.Update();

        if(Input.GetKeyDown(KeyCode.F1))
            DebugUI.Toggle();
        
        if(Input.GetKeyDown(KeyCode.Space))
            CommitGraphManager.Show();
        if(Input.GetKeyUp(KeyCode.Space))
            CommitGraphManager.Hide();

        Board board = Board.Instance;
        Economy economy = board.Current.GetEntity<Economy>(UniversalDeltaEntity.Economy);
        EconomyText.text = "Turn " + (board.TurnNumberOf(board.Current) / 2) + "\nFaith " + economy.GetResource(ResourceType.Faith) + " | Spice " + economy.GetResource(ResourceType.Spice);
        // EconomyText.color = CommitGraphManager.IsActive ? Color.white : Color.black;
    }

    public void Destroy(){
        CommitGraphManager.Destroy();
    }
}
