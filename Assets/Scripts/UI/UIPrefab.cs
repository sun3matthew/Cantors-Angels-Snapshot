using UnityEngine;
using UnityEngine.UI;

public class UIPrefab
{
    public static (RectTransform, Text) CreateText(string name, Font font, Transform parent, Vector2 anchor){
        GameObject go = new(name);
        go.transform.SetParent(parent);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMax = anchor;
        rect.anchorMin = anchor;
        rect.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Text textComponent = go.AddComponent<Text>();
        textComponent.font = font;
        textComponent.horizontalOverflow = HorizontalWrapMode.Overflow;
        textComponent.verticalOverflow = VerticalWrapMode.Overflow;
        textComponent.fontSize = 300;
        textComponent.text = "";
        textComponent.color = Color.black;
        return (rect, textComponent);
    }

    public static (RectTransform, Image) CreateImage(string name, Sprite sprite, Transform parent, Vector2 position, Vector2 anchor, Vector2 sizeDelta){
        GameObject go = new(name);
        go.transform.SetParent(parent);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.localScale = new Vector3(1, 1, 1);
        rect.offsetMin = position;
        rect.offsetMax = position;
        rect.sizeDelta = sizeDelta;
        // rect.position = position;

        Image imageComponent = go.AddComponent<Image>();
        imageComponent.sprite = sprite;
        imageComponent.color = Color.white;

        return (rect, imageComponent);
    }

    public static RectTransform CreatePanel(string name, Transform parent){
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = new Vector3(1, 1, 1);
        return rect;
    }
}
