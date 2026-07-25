using UnityEngine;

public class DebugHandler
{
    public static void Log(string name = "", string msg = "Log message")
    {
        Debug.Log($"[{name}] {msg}");
    }
    public static void LogError(string name = "", string msg = "Log message")
    {
        Debug.LogError($"[{name}] {msg}");
    }
    public static void LogWarning(string name = "", string msg = "Log message")
    {
        Debug.LogWarning($"[{name}] {msg}");
    }
}