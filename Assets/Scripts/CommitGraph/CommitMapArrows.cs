using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class CommitMapArrows : ICommitMapComponent
{
    public static CommitMapArrows Instance;
    private GameObject ArrowParent;
    private Text StepsForwardsText;
    public int NumStepsForwards { get; private set; }
    public const int MaxStepsForwards = 100;
    private Text LevelsText;
    public void Instantiate(GameObject canvas){
        // ? idk if this should be here
        Instance = this;
        ArrowParent = UIPrefab.CreatePanel("ArrowParent", canvas.transform).gameObject;

        (RectTransform leftArrowRect, Image leftArrow) = UIPrefab.CreateImage(
            "LeftArrow",
            Resources.Load<Sprite>("UI/NP/ArrowLeft"),
            ArrowParent.transform,
            new Vector2(-45, 25),
            new Vector2(0.75f, 0),
            new Vector2(30, 30)
        );

        (RectTransform rightArrowRect, Image rightArrow) = UIPrefab.CreateImage(
            "RightArrow",
            Resources.Load<Sprite>("UI/NP/ArrowRight"),
            ArrowParent.transform,
            new Vector2(45, 25),
            new Vector2(0.75f, 0),
            new Vector2(30, 30)
        );

        (RectTransform levelsTextRect, Text levelsText) = UIPrefab.CreateText(
            "LevelsText",
            Resources.Load<Font>("Misc/Pixeled"),
            ArrowParent.transform,
            new Vector2(0.75f, 0)
        );
        levelsText.alignment = TextAnchor.LowerCenter;
        levelsTextRect.localScale = new Vector3(0.3f, 0.3f, 0.1f);
        levelsTextRect.offsetMin = new Vector2(0, 5);
        levelsTextRect.offsetMax = new Vector2(0, 5);
        levelsText.color = Color.white;

        BasicButton leftArrowButton = leftArrowRect.gameObject.AddComponent<BasicButton>();
        leftArrowButton.Instantiate(
            leftArrow,
            leftArrowRect,
            (int dummy) => {
                if (NumStepsForwards > 1){
                    NumStepsForwards--;
                    Board.Instance.DecreaseBoardStateCount();
                    CoreLoop.Instance.CleanUp();
                }
                levelsText.text = NumStepsForwards.ToString();
            },
            0
        );

        BasicButton rightArrowButton = rightArrowRect.gameObject.AddComponent<BasicButton>();
        rightArrowButton.Instantiate(
            rightArrow,
            rightArrowRect,
            (int dummy) => {
                if (NumStepsForwards < MaxStepsForwards){
                    NumStepsForwards++;
                    Board.Instance.GenerateNextUserBoard();
                    CoreLoop.Instance.CleanUp();
                }
                
                levelsText.text = NumStepsForwards.ToString();
            },
            0
        );

        StepsForwardsText = levelsText;
        NumStepsForwards = 1;
        levelsText.text = NumStepsForwards.ToString();

        LevelsText = levelsText;
    }


    public void Show() => ArrowParent.SetActive(true);
    public void Hide() => ArrowParent.SetActive(false);
    public void SyncToBoard(){
        NumStepsForwards = Board.Instance.NumberOfBoardStates() / 2;   
        LevelsText.text = NumStepsForwards.ToString();
    }
    


    public void SetOpacity(float opacity){}
    public void Dispose(){}
    public void Update(){}

}
