using System.Collections;
using System.Runtime.Serialization;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MonoDelta
{
    public byte[] DeltaData;
    private List<byte> DeltaDataBuffer;
    public const int ReadStart = 8;
    public MonoDelta(Entity entity)
    {
        DeltaDataBuffer = new List<byte>();
        Write(entity.Position);
    }
    public MonoDelta(byte[] deltaData){
        DeltaData = deltaData;
        DeltaDataBuffer = new List<byte>();
        for(int i = 0; i < DeltaData.Length; i++)
            DeltaDataBuffer.Add(DeltaData[i]);
    }
    
    public void Write(byte deltaData){
        DeltaDataBuffer.Add(deltaData);
        WriteDeltaData();
    }

    public void Write(int deltaData){
        DeltaDataBuffer.AddRange(BitConverter.GetBytes(deltaData));
        WriteDeltaData();
    }

    public void Write(float deltaData){
        DeltaDataBuffer.AddRange(BitConverter.GetBytes(deltaData));
        WriteDeltaData();
    }

    public void Write(bool deltaData){
        DeltaDataBuffer.AddRange(BitConverter.GetBytes(deltaData));
        WriteDeltaData();
    }
    public void Write(string deltaData){
        Write(deltaData.Length);
        DeltaDataBuffer.AddRange(System.Text.Encoding.ASCII.GetBytes(deltaData));
        WriteDeltaData();
    }

    public void Write(Vector2 deltaData){
        DeltaDataBuffer.AddRange(BitConverter.GetBytes(deltaData.x));
        DeltaDataBuffer.AddRange(BitConverter.GetBytes(deltaData.y));
        WriteDeltaData();
    }
    public void Write(HexVector deltaData){
        DeltaDataBuffer.AddRange(BitConverter.GetBytes(deltaData.x));
        DeltaDataBuffer.AddRange(BitConverter.GetBytes(deltaData.y));
        WriteDeltaData();
    }

    public int ReadInt(ref int index){
        int deltaData = BitConverter.ToInt32(DeltaData, index);
        index += 4;
        return deltaData;
    }

    public float ReadFloat(ref int index){
        float deltaData = BitConverter.ToSingle(DeltaData, index);
        index += 4;
        return deltaData;
    }

    public bool ReadBool(ref int index){
        bool deltaData = BitConverter.ToBoolean(DeltaData, index);
        index += 1;
        return deltaData;
    }

    public string ReadString(ref int index){
        int length = ReadInt(ref index);
        string deltaData = System.Text.Encoding.ASCII.GetString(DeltaData, index, length);
        index += length;
        return deltaData;
    }

    public Vector2 ReadVector2(ref int index){
        float x = BitConverter.ToSingle(DeltaData, index);
        index += 4;
        float y = BitConverter.ToSingle(DeltaData, index);
        index += 4;
        return new Vector2(x, y);
    }

    public HexVector ReadHexVector(ref int index){
        int x = BitConverter.ToInt32(DeltaData, index);
        index += 4;
        int y = BitConverter.ToInt32(DeltaData, index);
        index += 4;
        return new HexVector(x, y);
    }
    public byte ReadByte(ref int index){
        byte deltaData = DeltaData[index];
        index += 1;
        return deltaData;
    }
    public T Read<T>(ref int index){
        if (typeof(T) == typeof(byte))
            return (T)(object)ReadByte(ref index);
        if (typeof(T) == typeof(int))
            return (T)(object)ReadInt(ref index);
        if (typeof(T) == typeof(float))
            return (T)(object)ReadFloat(ref index);
        if (typeof(T) == typeof(bool))
            return (T)(object)ReadBool(ref index);
        if (typeof(T) == typeof(string))
            return (T)(object)ReadString(ref index);
        if (typeof(T) == typeof(Vector2))
            return (T)(object)ReadVector2(ref index);
        if (typeof(T) == typeof(HexVector))
            return (T)(object)ReadHexVector(ref index);
        Debug.LogError("MonoDelta: GetDeltaData: unknown type");
        return default(T);
    }
    private void WriteDeltaData() => DeltaData = DeltaDataBuffer.ToArray();

    public override string ToString()
    {
        string deltaString = "";
        int index = 0;
        HexVector position = Read<HexVector>(ref index);
        deltaString += "Position: " + position + ", ";
        while(index < DeltaData.Length){
            int delta = ReadInt(ref index);
            deltaString += delta + " ";
        }
        return deltaString;
    }
    public void Serialize(List<byte> bytes){
        SaveFile.WriteInt(bytes, DeltaData.Length);
        bytes.AddRange(DeltaData);
    }
    public static MonoDelta Deserialize(byte[] bytes, ref int index){
        int length = SaveFile.ReadInt(bytes, ref index);
        byte[] deltaData = new byte[length];
        for(int i = 0; i < length; i++)
            deltaData[i] = bytes[index + i];
        index += length;
        return new MonoDelta(deltaData);
    }
}
