using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonDeltaMaker : DeltaMakerUI
{
    private List<ButtonDelta> ButtonDeltas;
    private bool Clicked;
    public class ButtonDelta{
        public Sprite sprite;
        public bool disabled;
        public BasicButton button;
        public ButtonDelta(Sprite sprite, bool disabled){
            this.sprite = sprite;
            this.disabled = disabled;
            this.button = null;
        }
    }
    public ButtonDeltaMaker(MonoDelta monoDelta, List<Sprite> sprites, List<bool> disabled) : base(monoDelta)
    {
        ButtonDeltas = new List<ButtonDelta>();

        for(int i = 0; i < sprites.Count; i++){
            ButtonDelta buttonDelta = new(sprites[i], disabled[i]);
            ButtonDeltas.Add(buttonDelta);
        }
    }

    public override void CreateDeltaUI()
    {
        for(int i = 0; i < ButtonDeltas.Count; i++){
            ButtonDelta buttonDelta = ButtonDeltas[i];
            (RectTransform, Image) image = UIPrefab.CreateImage(
                "Action " + i + " Item Frame",
                buttonDelta.sprite,
                UICollection.Instance.ActionButtonHolder.transform,
                new Vector2(i * 85 + 10, 10),
                new Vector2(0, 0),
                new Vector2(75, 75)
            );

            BasicButton basicButton = image.Item1.gameObject.AddComponent<BasicButton>();
            basicButton.Instantiate(
                image.Item2,
                image.Item1,
                buttonDelta.disabled ? null : 
                (int idx) => {
                    MonoDelta.Write(i);
                    Clicked = true;
                },
                i
            );
            buttonDelta.button = basicButton;
        }
    }

    public override void Destroy()
    {
        for(int i = 0; i < ButtonDeltas.Count; i++)
            GameObject.Destroy(ButtonDeltas[i].button.gameObject);
    }

    public override bool Resolve(){
        for (int i = 0; i < ButtonDeltas.Count; i++)
            ButtonDeltas[i].button.Update(); // * Force Update since update is on monoBehaviour

        if (Clicked)
            return true;

        int alphaNumPressed = GetAlphaNumPressed() - 1;
        if (alphaNumPressed >= 0 && alphaNumPressed < ButtonDeltas.Count && !ButtonDeltas[alphaNumPressed].disabled){
            MonoDelta.Write(alphaNumPressed);
            return true;
        }
        return false;
    }
    private int GetAlphaNumPressed()
    {
        string input = Input.inputString;
        if (input.Length == 0)
            return -1;
        if (input.Length > 1)
            return -1;
        if (input[0] >= '0' && input[0] <= '9')
            return input[0] - '0';
        return -1;
    }
}