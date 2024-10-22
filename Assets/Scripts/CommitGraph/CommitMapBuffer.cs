using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommitMapBuffer : ICommitMapComponent
{
    private GameObject ScreenBufferParent;
    private RawImage TopBuffer;
    private RawImage BottomBuffer;

    private GameObject ArrowParent;
    private RawImage LeftArrow;
    private RawImage RightArrow;
    private Text LevelsText;
    private int LevelsForwards;

    public void Instantiate(GameObject canvas){
        ScreenBufferParent = new GameObject("ScreenBufferParent");
        ScreenBufferParent.transform.SetParent(canvas.transform, false);

        GameObject topBuffer = new GameObject("TopBuffer");
        topBuffer.transform.SetParent(ScreenBufferParent.transform, false);
        topBuffer.AddComponent<RawImage>().color = new Color(0, 0, 0, 0.9f);
        RectTransform topBufferRect = topBuffer.GetComponent<RectTransform>();
        TopBuffer = topBuffer.GetComponent<RawImage>();

        Rect canvasComponent = canvas.GetComponent<RectTransform>().rect;

        // size of the middle.
        float rectTransformSize = canvasComponent.width / 2;
        if (rectTransformSize > canvasComponent.height)
            rectTransformSize = canvasComponent.height;

        float bufferHeight = canvasComponent.height - rectTransformSize;

        topBufferRect.sizeDelta = new Vector2(canvasComponent.width, bufferHeight / 2);
        topBufferRect.anchoredPosition = new Vector2(0, 0);
        topBufferRect.localPosition = new Vector3(0, canvasComponent.height / 2 - bufferHeight / 4, 0);

        GameObject bottomBuffer = new GameObject("BottomBuffer");
        bottomBuffer.transform.SetParent(ScreenBufferParent.transform, false);
        bottomBuffer.AddComponent<RawImage>().color = new Color(0, 0, 0, 0.9f);
        RectTransform bottomBufferRect = bottomBuffer.GetComponent<RectTransform>();
        BottomBuffer = bottomBuffer.GetComponent<RawImage>();
        
        bottomBufferRect.sizeDelta = new Vector2(canvasComponent.width, bufferHeight / 2);
        bottomBufferRect.anchoredPosition = new Vector2(0, 0);
        bottomBufferRect.localPosition = new Vector3(0, -canvasComponent.height / 2 + bufferHeight / 4, 0);
    }

    public void Dispose() {}

    public void Hide()
    {
        ScreenBufferParent.SetActive(false);
    }

    public void SetOpacity(float opacity)
    {
        TopBuffer.color = new Color(0, 0, 0, opacity);
        BottomBuffer.color = new Color(0, 0, 0, opacity);        
    }

    public void Show()
    {
        ScreenBufferParent.SetActive(true);
    }

    public void Update(){}
}