using System.Collections.Generic;
using UnityEngine;
public static class DebugPerformance
{
    private static Stack<float> TimeStack = new();
    private static Stack<string> LayerStack = new(new string[] { "" });

    public static void Reset()
    {
        TimeStack.Clear();
        LayerStack.Clear();
        LayerStack.Push("");
    }
    public static void AddLayer(string message){
        string layerHeader = GetBufferHelper() + message + ": ";

        LayerStack.Push(layerHeader);
        LayerStack.Push("");
        TimeStack.Push(Time.realtimeSinceStartup);
        TimeStack.Push(Time.realtimeSinceStartup);
    }
    public static void EndLayer(){
        float time = Time.realtimeSinceStartup;
        string layerInfo = LayerStack.Pop();
        string layerHeader = LayerStack.Pop();
        TimeStack.Pop();
        layerHeader += Mathf.RoundToInt((time - TimeStack.Pop()) * 1000) + "ms\n";
        layerHeader += layerInfo;
        string buffer = LayerStack.Pop();
        buffer += layerHeader;
        LayerStack.Push(buffer);

        if (TimeStack.Count != 0){
            TimeStack.Pop();
            TimeStack.Push(time);
        }
    }
    public static void CreateSegment(string message)
    {
        float time = Time.realtimeSinceStartup;
        string buffer = LayerStack.Pop();
        buffer += GetBufferHelper() + message + ": " + Mathf.RoundToInt((time - TimeStack.Pop()) * 1000) + "ms\n";
        LayerStack.Push(buffer);
        TimeStack.Push(time);
    }
    private static string GetBufferHelper()
    {
        string buffer = "";
        for (int i = 0; i < TimeStack.Count / 2; i++)
            buffer += "|\t";
        return buffer;
    }
    public static void ClearBuffer()
    {
        LayerStack = new Stack<string>(new string[] { "" });
    }
    public static void PrintAndClearBuffer()
    {
        string buffer = LayerStack.Pop();
        Debug.Log(buffer);
        LayerStack.Push("");
    }
}
