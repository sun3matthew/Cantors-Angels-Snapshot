using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class SaveFile
{
    private static List<long> NetworkGraphBranches;
    public static void Save()
    {
        // byte[] saveFile = SaveUtility.Compress(GenerateSaveFile());
        int saveNumber = SaveUtility.CurrentSafeFileIdx(SaveUtility.SaveDir) + 1;
        byte[] saveFile = GenerateSaveFile();
        string saveFileName = saveNumber + SaveUtility.FileExtension;
        Debug.Log("Saving to: " + saveFileName);
        SaveUtility.WriteFile(SaveUtility.SaveDir + saveFileName, saveFile);

        //TODO add some system to prevent collisions between save files and commitGraph
        // Use Hash of the first node as the save file name?, dupes just have a counter.
        // for now just override
        byte[] commitGraph = BoardHistory.SerializeGraph(BoardHistory.Root);
        string commitGraphFileName = BoardHistory.Root.Children[0].Hash + SaveUtility.FileExtension;
        SaveUtility.WriteFile(SaveUtility.NetworkGraphCache(Board.Instance.Seed) + commitGraphFileName, commitGraph);
    }
    public static void GenerateRandomNetwork(long rootHash)
    {
        HistoryNode root = new(rootHash);
        Stack<(HistoryNode, float)> stack = new();
        stack.Push((root, 1f * CoreRandom.GlobalRange(0.3f, 1.7f)));
        while (stack.Count > 0){
            (HistoryNode, float) current = stack.Pop();
            if (current.Item2 < 0.05f) continue;
            current.Item2 *= CoreRandom.GlobalRange(0.990f, 0.999f);
            for(int failSafe = 0; failSafe < 1000; failSafe++){
                float randValue = CoreRandom.Value();
                HistoryNode child = new(CoreRandom.ValueLong());
                current.Item1.AddChild(child);
                if (current.Item1.Children.Count == 1)
                    stack.Push((child, current.Item2));
                else
                    stack.Push((child, current.Item2 * CoreRandom.GlobalRange(0.1f, 0.7f))); //Branch

                if (randValue < 0.995f || current.Item1 == root) // also prevent branch on root
                    break;
            }
        }

        byte[] commitGraph = BoardHistory.SerializeGraph(root);
        string commitGraphFileName = root.Children[0].Hash + SaveUtility.FileExtension;
        SaveUtility.WriteFile(SaveUtility.NetworkGraphCache(Board.Instance.Seed) + commitGraphFileName, commitGraph);
    }
    public static HistoryNode LoadNetworkGraph(int saveNumber)
    {
        if (NetworkGraphBranches == null){
            NetworkGraphBranches = new();
            string[] files = System.IO.Directory.GetFiles(SaveUtility.NetworkGraphCache(Board.Instance.Seed), "*" + SaveUtility.FileExtension);
            foreach (string file in files){
                string name = System.IO.Path.GetFileNameWithoutExtension(file);
                NetworkGraphBranches.Add(long.Parse(name));
            }
        }

        byte[] commitGraph = SaveUtility.ReadFile(SaveUtility.NetworkGraphCache(Board.Instance.Seed) + NetworkGraphBranches[saveNumber] + SaveUtility.FileExtension);
        return BoardHistory.DeserializeGraph(commitGraph);
    }
    public static byte[] GenerateSaveFile()
    {
        List<byte> saveFile = new();
        WriteString(saveFile, "Cantor's Angels Save File 0.0.1");
        Board.Instance.Serialize(saveFile);
        CoreCamera.Serialize(saveFile);
        return saveFile.ToArray();
    }

    public static void CacheEncryptedMap(int seed)
    {
        int[,] map = IslandGenerator.GenerateIslandTiles(BoardCreator.boardSize, seed);
        byte[] data = new byte[BoardCreator.boardSize * BoardCreator.boardSize + 4];
        for (int i = 0; i < BoardCreator.boardSize; i++)
            for (int j = 0; j < BoardCreator.boardSize; j++)
                data[i * BoardCreator.boardSize + j] = (byte)map[i, j];
        int end = BoardCreator.boardSize * BoardCreator.boardSize;
        data[end] = (byte)(seed >> 24);
        data[end + 1] = (byte)(seed >> 16);
        data[end + 2] = (byte)(seed >> 8);
        data[end + 3] = (byte)seed;

        byte[] encryptedData = SaveUtility.Encrypt(data, "January 6, 1918 " + seed); //DOB of Cantor
        SaveUtility.WriteFile(SaveUtility.Cache + seed + SaveUtility.FileExtension, encryptedData);
    }

    public static int[,] LoadEncryptedMap(int seed)
    {
        if (!SaveUtility.FileExists(SaveUtility.Cache + seed + SaveUtility.FileExtension))
            return null;
        byte[] encryptedData = SaveUtility.ReadFile(SaveUtility.Cache + seed + SaveUtility.FileExtension);
        byte[] data = null;
        try{
            data = SaveUtility.Decrypt(encryptedData, "January 6, 1918 " + seed);
        }catch{
            Debug.LogError("Tampering detected");
            return null;
        }
        int[,] map = new int[BoardCreator.boardSize, BoardCreator.boardSize];
        for (int i = 0; i < BoardCreator.boardSize; i++)
            for (int j = 0; j < BoardCreator.boardSize; j++)
                map[i, j] = data[i * BoardCreator.boardSize + j];
        int end = BoardCreator.boardSize * BoardCreator.boardSize;
        int loadedSeed = data[end] << 24 | data[end + 1] << 16 | data[end + 2] << 8 | data[end + 3];
        if (loadedSeed != seed){
            Debug.LogError("Tampering detected");
            return null;
        }
        return map;
    }
    public static void Load(int saveNumber)
    {
        // byte[] saveFile = SaveUtility.Decompress(SaveUtility.ReadFile(SaveUtility.SaveDir + saveNumber + SaveUtility.FileExtension));
        byte[] saveFile = SaveUtility.ReadFile(SaveUtility.SaveDir + saveNumber + SaveUtility.FileExtension);
        LoadSaveFile(saveFile);
    }

    public static void LoadSaveFile(byte[] saveFile)
    {
        int index = 0;
        string header = ReadString(saveFile, ref index);
        Board.Deserialize(saveFile, ref index);
        CoreCamera.Deserialize(saveFile, ref index);
    }

    // Little endian
    // public static void WriteInt(List<byte> saveFile, int value) => saveFile.AddRange(System.BitConverter.GetBytes(value));
    // public static void WriteFloat(List<byte> saveFile, float value) => saveFile.AddRange(System.BitConverter.GetBytes(value));
    // public static void WriteBool(List<byte> saveFile, bool value) => WriteInt(saveFile, value ? 1 : 0);
    // public static void WriteLong(List<byte> saveFile, long value) => saveFile.AddRange(System.BitConverter.GetBytes(value));

    // Big endian
    public static void WriteInt(List<byte> saveFile, int value)
    {
        byte[] bytes = System.BitConverter.GetBytes(value);
        System.Array.Reverse(bytes);
        saveFile.AddRange(bytes);
    }
    public static void WriteFloat(List<byte> saveFile, float value)
    {
        byte[] bytes = System.BitConverter.GetBytes(value);
        System.Array.Reverse(bytes);
        saveFile.AddRange(bytes);
    }
    public static void WriteBool(List<byte> saveFile, bool value) => WriteInt(saveFile, value ? 1 : 0);
    public static void WriteLong(List<byte> saveFile, long value)
    {
        byte[] bytes = System.BitConverter.GetBytes(value);
        System.Array.Reverse(bytes);
        saveFile.AddRange(bytes);
    }

    
    public static void WriteByte(List<byte> saveFile, byte value) => saveFile.Add(value);
    public static void WriteString(List<byte> saveFile, string value)
    {
        int pad = 4 - (value.Length % 4);
        saveFile.AddRange(System.Text.Encoding.ASCII.GetBytes(value));
        for (int i = 0; i < pad; i++)
            saveFile.Add(0);
    }
    public static void WriteVector2(List<byte> saveFile, Vector2 value)
    {
        WriteFloat(saveFile, value.x);
        WriteFloat(saveFile, value.y);
    }
    public static void WriteHexVector(List<byte> saveFile, HexVector value)
    {
        WriteInt(saveFile, value.x);
        WriteInt(saveFile, value.y);
    }

    // Little endian
    // public static int ReadInt(byte[] saveFile, ref int index)
    // {
    //     int value = System.BitConverter.ToInt32(saveFile, index);
    //     index += 4;
    //     return value;
    // }
    // public static float ReadFloat(byte[] saveFile, ref int index)
    // {
    //     float value = System.BitConverter.ToSingle(saveFile, index);
    //     index += 4;
    //     return value;
    // }
    // public static long ReadLong(byte[] saveFile, ref int index)
    // {
    //     long value = System.BitConverter.ToInt64(saveFile, index);
    //     index += 8;
    //     return value;
    // }
    // public static bool ReadBool(byte[] saveFile, ref int index) => ReadInt(saveFile, ref index) == 1;

    // Big endian
    public static int ReadInt(byte[] saveFile, ref int index)
    {
        byte[] bytes = new byte[4];
        for (int i = 0; i < 4; i++)
            bytes[i] = saveFile[index + 3 - i];
        int value = System.BitConverter.ToInt32(bytes, 0);
        index += 4;
        return value;
    }
    public static float ReadFloat(byte[] saveFile, ref int index)
    {
        byte[] bytes = new byte[4];
        for (int i = 0; i < 4; i++)
            bytes[i] = saveFile[index + 3 - i];
        float value = System.BitConverter.ToSingle(bytes, 0);
        index += 4;
        return value;
    }
    public static long ReadLong(byte[] saveFile, ref int index)
    {
        byte[] bytes = new byte[8];
        for (int i = 0; i < 8; i++)
            bytes[i] = saveFile[index + 7 - i];
        long value = System.BitConverter.ToInt64(bytes, 0);
        index += 8;
        return value;
    }
    public static byte ReadByte(byte[] saveFile, ref int index) => saveFile[index++];
    public static bool ReadBool(byte[] saveFile, ref int index) => ReadInt(saveFile, ref index) == 1;
    public static string ReadString(byte[] saveFile, ref int index){
        string value = "";
        while(saveFile[index] != 0){
            value += (char)saveFile[index];
            index++;
        }
        index += 4 - (value.Length % 4);
        return value;
    }

}
