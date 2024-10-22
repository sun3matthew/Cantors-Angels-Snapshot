 using UnityEngine;
 using System.IO;
 
 public class DebugToFile : MonoBehaviour
 {
    public static string filename = "";
    public static void Initialize(){
        Application.logMessageReceived += Log;
        Log("Log Started", "", LogType.Log);
    }
    public static void Log(string logString, string stackTrace, LogType type)
    {
        if (filename == "")
            filename = SaveUtility.LogPath + "CantorsAngelLogFile-" + Directory.GetFiles(SaveUtility.LogPath).Length + ".log";
        File.AppendAllText(filename, type.ToString() + ":\n" + logString + " " + Time.realtimeSinceStartup + "\n" + stackTrace + "\n\n");
    }
 }