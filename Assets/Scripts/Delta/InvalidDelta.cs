using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvalidDelta
{
    public int InvalidCode { get; private set; }
    public string InvalidMessage { get; private set; }

    public InvalidDelta(int invalidCode, string invalidMessage)
    {
        InvalidCode = invalidCode;
        InvalidMessage = invalidMessage;
    }

    public override string ToString()
    {
        return InvalidCode + " " + InvalidMessage;
    }
}
