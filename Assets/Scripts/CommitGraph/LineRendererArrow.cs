 using UnityEngine;

public class LineRendererArrow
{
    private Transform ArrowOrigin;
    private LineRenderer LineRenderer;
    private float Width;
    private float LengthOffset;
    public LineRendererArrow(Transform origin, float width = 5, float lengthOffset = 0){
        GameObject arrow = new GameObject("Arrow");
        arrow.layer = origin.gameObject.layer;
        arrow.transform.parent = origin.transform;
        Width = width;
        LengthOffset = lengthOffset;
        LineRenderer = arrow.AddComponent<LineRenderer>();
        LineRenderer.sortingOrder = 1;
        LineRenderer.useWorldSpace = true;
        LineRenderer.positionCount = 4;
        LineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        ArrowOrigin = origin.transform;
        LineRenderer.enabled = false;
    }
    public void DisableArrow() => LineRenderer.enabled = false;
    public void UpdateArrow(Vector2 vector)
    {
        LineRenderer.enabled = true;
        float Length = Vector3.Distance(ArrowOrigin.position, vector) - LengthOffset;
        if (Length < Width)
            Length = Width;
        vector = (Vector2)ArrowOrigin.position + (vector - (Vector2)ArrowOrigin.position).normalized * Length;

        Vector2 origin = (Vector2)ArrowOrigin.position;
        origin += (vector - origin).normalized * LengthOffset;


        
        float percentHead = 0.9f * Width;
        float adaptiveSize = (float)(percentHead / Vector3.Distance(origin, vector));
        LineRenderer.widthCurve = new AnimationCurve(
            new Keyframe(0, 0.4f * Width)
            , new Keyframe(0.999f - adaptiveSize, 0.4f * Width)
            , new Keyframe(1 - adaptiveSize, 1f * Width)
            , new Keyframe(1, 0f));
        LineRenderer.SetPositions(new Vector3[] {
                origin
                , Vector3.Lerp(origin, vector, 0.999f - adaptiveSize)
                , Vector3.Lerp(origin, vector, 1 - adaptiveSize)
                , vector });
    }
}