using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceQuit : MonoBehaviour
{
    private List<char> keyBuffer = new();

    void Update()
    {
        char currChar = relevantKeys();
        if(currChar != ' '){
            keyBuffer.Add(currChar);
            if(keyBuffer.Count > 9)
                keyBuffer.RemoveAt(0);
            string keyString = "";
            foreach(char c in keyBuffer)
                keyString += c;
            if(keyString == "forcequit"){
                Application.Quit();
                Debug.Log("Force Quit");
            }
        }        
    }
    private char relevantKeys(){
        if(Input.GetKeyDown(KeyCode.F))
            return 'f';
        if(Input.GetKeyDown(KeyCode.O))
            return 'o';
        if(Input.GetKeyDown(KeyCode.R))
            return 'r';
        if(Input.GetKeyDown(KeyCode.C))
            return 'c';
        if(Input.GetKeyDown(KeyCode.E))
            return 'e';
        if(Input.GetKeyDown(KeyCode.Q))
            return 'q';
        if(Input.GetKeyDown(KeyCode.U))
            return 'u';
        if(Input.GetKeyDown(KeyCode.I))
            return 'i';
        if(Input.GetKeyDown(KeyCode.T))
            return 't'; 
        return ' ';
    }
}
