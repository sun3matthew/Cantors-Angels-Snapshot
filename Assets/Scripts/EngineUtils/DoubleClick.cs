using UnityEngine;

public class DoubleClick : MonoBehaviour
{
    private static float lastClickTime = 0;
    private const float doubleClickTime = 0.2f;
    public static bool RecentlyClicked => Time.time - lastClickTime < doubleClickTime;
    private void LateUpdate() {
        if (Input.GetMouseButtonDown(0))
            lastClickTime = Time.time;
    }
}