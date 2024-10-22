using System;
using UnityEngine;
using UnityEngine.UI;

public class BasicButton : MonoBehaviour
{
    private Image Img;
    private RectTransform Rect;

    private Action<int> OnClick;
    private int ClickedIdx;
    private Color BaseColor;
    private bool OnFirstFrame;
    public void Instantiate(Image img, RectTransform rect, Action<int> onClick, int clickedIdx){
        Img = img;
        Rect = rect;
        OnClick = onClick;
        ClickedIdx = clickedIdx;
        OnFirstFrame = true;
    }

    private bool MouseInBox(){
        Vector2 pos = Input.mousePosition;
        Vector3[] corners = new Vector3[4];
        Rect.GetWorldCorners(corners);

        return pos.x > corners[0].x && pos.x < corners[2].x && pos.y > corners[0].y && pos.y < corners[2].y;
    }

    public void Update(){
        if(OnFirstFrame){
            OnFirstFrame = false;
            return;
        }

        if(OnClick == null){
            Img.color = new(0.25f, 0.25f, 0.25f, 1);
            return;
        }

        if(MouseInBox()){
            Img.color = new(0.9f, 0.9f, 0.9f, 1);
            if(Input.GetMouseButtonUp(0))
                OnClick(ClickedIdx);
            else if(Input.GetMouseButton(0))
                Img.color = new(0.5f, 0.5f, 0.5f, 1);
        }else{
            Img.color = Color.white;
        }
    }
}
