using UnityEngine;
using UnityEngine.UI;

public class UILineRender
{
    public RectTransform Transform { get; private set; }
    private Image Image;

    public UILineRender(RectTransform transform)
    {
        Transform = transform;
        Image = transform.gameObject.AddComponent<Image>();
        Image.color = Color.white;
        Image.sprite = Resources.Load<Sprite>("Sprites/Misc/Pixel");
    }
    
    public void DrawLine(Vector2 point1, Vector2 point2)
    {
        Vector2 midpoint = (point1 + point2) / 2f;
        Transform.localPosition = midpoint;
        Vector2 dir = point1 - point2;
        Transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        Transform.localScale = new Vector3(dir.magnitude, 0.5f, 1f);
    }
    public void SetColor(Color color) => Image.color = color;
}