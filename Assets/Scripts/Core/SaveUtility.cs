using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;

public class SaveUtility
{
    static SaveUtility(){
        SavePath = Application.persistentDataPath;
#if UNITY_EDITOR
        // if mac and editor
        if (Application.platform == RuntimePlatform.OSXEditor){
            SavePath = Application.persistentDataPath;
            SavePath = SavePath.Substring(0, SavePath.LastIndexOf("Application Support")) + "Application Support/" + PlayerSettings.applicationIdentifier + "/";
        }
#endif

        CreateGameDirectories();
    }
    private static byte[] salt = new byte[16] { 3, 2, 1, 4, 2, 7, 10, 132, 3, 4, 8, 4, 2, 1, 10, 132 };
    private static string[] supportedVersions = new string[]{
        "1.0.0",
    };
    public const string FileExtension = ".scc";
    public static string SavePath;
    public static string Temp => SavePath + "/Temp/";
    public static string SaveDir => SavePath + "/Save/";
    public static string SteamSave => SavePath + "/SteamSave/";
    public static string LogPath => SavePath + "/Logs/";
    public static string LogKeysPath => LogPath + "Keys/";
    private static string NetworkGraphCacheDir => SavePath + "/NetworkGraphCache/";
    public static string Cache => SavePath + "/Cache/";
    public static string NetworkGraphCache(int seed) => NetworkGraphCacheDir + seed + "/";

    private static void CreateGameDirectories(){
        Directory.CreateDirectory(SavePath);
        Directory.CreateDirectory(Temp);
        Directory.CreateDirectory(SaveDir);
        Directory.CreateDirectory(SteamSave);
        Directory.CreateDirectory(LogPath);
        Directory.CreateDirectory(LogKeysPath);
        Directory.CreateDirectory(NetworkGraphCacheDir);
        Directory.CreateDirectory(Cache);
    }
    public static void CreateDirectory(string path){
        if(!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    public static bool CanPortSaveFile(string currVersion){
        if(currVersion == Application.version) 
            return true;
        for(int i = 0; i < supportedVersions.Length; i++)
            if(supportedVersions[i] == currVersion)
                return true;
        return false;
    }
    public static int CurrentSafeFileIdx(string path){
        string[] files = Directory.GetFiles(path, "*" + FileExtension);
        int max = 0;
        foreach(string file in files){
            string name = Path.GetFileNameWithoutExtension(file);
            if(int.TryParse(name, out int num))
                max = Math.Max(max, num);
        }
        return max;
    }

    public static void WriteFile(string path, byte[] data){
        FileStream fileStream = new(path, FileMode.Create);
        fileStream.Write(data, 0, data.Length);
        fileStream.Close();
    }
    public static bool FileExists(string path) => File.Exists(path);
    public static byte[] ReadFile(string path){
        FileStream fileStream = new(path, FileMode.Open);
        byte[] data = new byte[fileStream.Length];
        fileStream.Read(data, 0, data.Length);
        fileStream.Close();
        return data;
    }
    public static void DeleteDirectory(string path)
    {
        foreach (string directory in Directory.GetDirectories(path))
            DeleteDirectory(directory);
        try
        {
            Directory.Delete(path, true);
        }
        catch (IOException) 
        {
            Directory.Delete(path, true);
        }
        catch (UnauthorizedAccessException)
        {
            Directory.Delete(path, true);
        }
    }
    private static long DirSize(DirectoryInfo d) 
    {    
        long size = 0;    
        FileInfo[] fis = d.GetFiles();
        foreach (FileInfo fi in fis) 
            size += fi.Length;    
        DirectoryInfo[] dis = d.GetDirectories();
        foreach (DirectoryInfo di in dis) 
            size += DirSize(di);   
        return size;  
    }
    public static float DirSizeMB(string path) => DirSize(new DirectoryInfo(path))/1000000f;
    public static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));

        foreach (string newPath in Directory.GetFiles(sourcePath, "*.sce", SearchOption.AllDirectories))
            if(!File.Exists(newPath.Replace(sourcePath, targetPath)))
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath));
    }

    public static void CopyDirectoryRecursively(string sourcePath, string targetPath)
    {
        Directory.CreateDirectory(targetPath);

        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));

        foreach (string newPath in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            if(!File.Exists(newPath.Replace(sourcePath, targetPath)))
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath));
    }
    public static byte[] Compress(byte[] data){
        MemoryStream output = new();
        using (DeflateStream dStream = new(output, System.IO.Compression.CompressionLevel.Optimal))
            dStream.Write(data, 0, data.Length);
        return output.ToArray();
    }
    public static byte[] Decompress(byte[] data){
        MemoryStream input = new(data, 0, data.Length);
        MemoryStream output = new();
        using (DeflateStream dStream = new(input, System.IO.Compression.CompressionMode.Decompress))
            dStream.CopyTo(output);
        return output.ToArray();
    }

    public static void DumpToBoardHash(Board board){
        long hash = board.GenerateCurrentBoardDelta().Hash;
        string path = LogKeysPath + ToHexString(hash) + FileExtension;
        string data = board.FirstThreeBoards();
        WriteFile(path, ToBytes(data));
    }
    
    public static void CleanDirectory(string directory){
        if(Directory.Exists(directory))
            SaveUtility.DeleteDirectory(directory);
        Directory.CreateDirectory(directory);
    }

    public static byte[] ToBytes(string str) => Encoding.UTF8.GetBytes(str);
    public static string ToString(byte[] bytes) => Encoding.UTF8.GetString(bytes);
    public static long ReHash(long hash){
        hash = (~hash) + (hash << 21);
        hash ^= hash >> 24;
        hash = hash + (hash << 3) + (hash << 8);
        hash ^= hash >> 14;
        hash = hash + (hash << 2) + (hash << 4);
        hash ^= hash >> 28;
        hash += hash << 31;
        return hash;
    }
    public static string ToHexString(long hash) => hash.ToString("X16");
    public static string ToHexSubstring(long hash) => ToHexString(hash).Substring(0, 7);
    // public static string ToHexSubstring(long hash) => ToHexString(hash);
    public static int ShiftAndWrap(int value, int positions){
        positions %= 32;
        if (positions < 0)
            positions += 32;

        return (value << positions) | (value >> (32 - positions));
    }
    public static long ShiftAndWrap(long value, int positions)
    {
        positions %= 64;
        if (positions < 0)
            positions += 64;
        return (value << positions) | (value >> (64 - positions));
    }

    public static byte[] Encrypt(byte[] data, string password){
        new RNGCryptoServiceProvider().GetBytes(salt);
        Rfc2898DeriveBytes key = new(password, salt, 1000);
        Aes aes = Aes.Create();
        aes.Key = key.GetBytes(32);
        aes.IV = key.GetBytes(16);
        MemoryStream ms = new();
        ms.Write(salt, 0, salt.Length);
        CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(data, 0, data.Length);
        cs.Close();
        return ms.ToArray();
    }
    public static byte[] Decrypt(byte[] data, string password){
        Array.Copy(data, 0, salt, 0, 16);
        Rfc2898DeriveBytes key = new(password, salt, 1000);
        Aes aes = Aes.Create();
        aes.Key = key.GetBytes(32);
        aes.IV = key.GetBytes(16);
        MemoryStream ms = new();
        CryptoStream cs = new(new MemoryStream(data, 16, data.Length - 16), aes.CreateDecryptor(), CryptoStreamMode.Read);
        cs.CopyTo(ms);
        cs.Close();
        return ms.ToArray();
    }
}
