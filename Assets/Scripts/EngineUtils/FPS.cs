using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPS : MonoBehaviour
{
    public Text TxtFps;
    public float UpdateRateSeconds = 4.0F;

    int frameCount = 0;
    float dt = 0.0F;
    float fps = 0.0F;
    void Update()
    {
        frameCount++;
        dt += Time.unscaledDeltaTime;
        if (dt > 1.0 / UpdateRateSeconds)
        {
            fps = frameCount / dt;
            frameCount = 0;
            dt -= 1.0F / UpdateRateSeconds;
        }
        TxtFps.text = System.Math.Round(fps, 1).ToString("0.0") + " FPS";
    }
}