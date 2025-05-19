using UnityEngine;
using System.Diagnostics;
using System.IO;

public class ServerRunner : MonoBehaviour
{
    [Header("Server Config")]
    public bool autoStartServer = true;
    public float serverStartDelay = 2f;

    private Process serverProcess;

    void Start()
    {
        if (autoStartServer)
        {
            Invoke("StartServer", serverStartDelay);
        }
    }

    void OnApplicationQuit()
    {
        StopServer();
    }

    // void StartServer()
    // {
    //     try
    //     {
    //         string serverFolder = Path.Combine(Application.dataPath, "StreamingAssets");
    //         string serverScript = Path.Combine(serverFolder, 
    //             Application.platform == RuntimePlatform.WindowsEditor ? 
    //             "launch_server.bat" : "launch_server.command");

    //         if (!File.Exists(serverScript))
    //         {
    //             UnityEngine.Debug.LogError($"Server script not found at: {serverScript}");
    //             return;
    //         }

    //         #if !UNITY_EDITOR_WIN
    //         SetFileExecutable(serverScript);
    //         #endif

    //         var startInfo = new ProcessStartInfo
    //         {
    //             FileName = serverScript,
    //             WorkingDirectory = serverFolder,
    //             UseShellExecute = true,
    //             CreateNoWindow = false
    //         };

    //         serverProcess = Process.Start(startInfo);
    //         UnityEngine.Debug.Log($"Server started at: {serverScript}");
    //     }
    //     catch (System.Exception e)
    //     {
    //         UnityEngine.Debug.LogError($"Server start failed: {e.Message}\n{e.StackTrace}");
    //     }
    // }
    void StartServer()
{
    try
    {
        string serverFolder = Path.Combine(Application.dataPath, "StreamingAssets");
        string pythonPath = Application.platform == RuntimePlatform.WindowsEditor ? 
            "python" : "python3";

        var startInfo = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"\"{Path.Combine(serverFolder, "server.py")}\"",
            WorkingDirectory = serverFolder,
            UseShellExecute = true,
            CreateNoWindow = false,
            RedirectStandardOutput = false
        };

        serverProcess = new Process { StartInfo = startInfo };
        serverProcess.Start();
        
        UnityEngine.Debug.Log($"Server started on PID: {serverProcess.Id}");
    }
    catch (System.Exception e)
    {
        UnityEngine.Debug.LogError($"Server start failed: {e.Message}\n{e.StackTrace}");
    }
}

    void StopServer()
    {
        try
        {
            if (serverProcess != null && !serverProcess.HasExited)
            {
                serverProcess.Kill();
                UnityEngine.Debug.Log("Server stopped");
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Error stopping server: {e.Message}");
        }
    }

    #if !UNITY_EDITOR_WIN
    void SetFileExecutable(string path)
    {
        try
        {
            var chmod = new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"+x \"{path}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            using (var p = Process.Start(chmod))
            {
                p.WaitForExit();
                UnityEngine.Debug.Log($"Made executable: {path} (exit code: {p.ExitCode})");
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"chmod failed: {e.Message}");
        }
    }
    #endif
}