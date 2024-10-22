using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI
{
    public GameObject DebugUIParent { get; private set; }

    private List<GameObject> debugObjects;
    private Text HoverText;

    public DebugUI(GameObject canvasParent)
    {
        debugObjects = new List<GameObject>();

        GameObject debugUI = UIPrefab.CreatePanel("DebugUI", canvasParent.transform).gameObject;
        DebugUIParent = debugUI;

        // FPS
        (RectTransform, Text) text = UIPrefab.CreateText(
            "FPS",
            Resources.Load<Font>("Misc/Pixeled"),
            debugUI.transform,
            new Vector2(0, 1)
        );

        FPS fps = text.Item1.gameObject.AddComponent<FPS>();
        fps.TxtFps = text.Item2;

        debugObjects.Add(text.Item1.gameObject);


        // Hover Text
        // Bottom right
        (RectTransform, Text) hoverText = UIPrefab.CreateText(
            "HoverText",
            Resources.Load<Font>("Misc/Menlo-Regular"),
            debugUI.transform,
            new Vector2(1, 0)
        );
        hoverText.Item2.alignment = TextAnchor.LowerRight;
        hoverText.Item2.fontStyle = FontStyle.Bold;
        hoverText.Item2.fontSize = 90;
        HoverText = hoverText.Item2;
    }
    public void Update(){
        DeltaEntity deltaEntity = Board.Instance.Current.GetEntity<DeltaEntity>(CoreLoop.MouseHexPos());
        Tile tile = Board.Instance.Current.GetEntity<Tile>(CoreLoop.MouseHexPos());

        HoverText.text = "";
        if (deltaEntity != null)
            HoverText.text = deltaEntity.ToString();
        if (tile != null)
            HoverText.text += "\n" + tile.ToString();
    }

    public void Toggle() => DebugUIParent.SetActive(!DebugUIParent.activeSelf);
}
