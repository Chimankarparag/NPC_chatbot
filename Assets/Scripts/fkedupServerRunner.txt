using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Collections;

public class ServerRunner : MonoBehaviour
{
    public int serverPort = 5001;

    private Process serverProcess;
    private static ServerRunner instance;
    private bool serverReady = false;

    void Awake()
    {
        instance = this;
    }

    public static bool IsRunning =>
        instance != null && instance.serverProcess != null && !instance.serverProcess.HasExited;

    public bool IsReady => serverReady;

    public void LaunchServerManually()
    {
        if (!IsRunning)
        {
            UnityEngine.Debug.Log("Launching server...");
            instance.StartCoroutine(instance.StartServerAsync());
        }
        else
        {
            UnityEngine.Debug.Log("Server already running.");
        }
    }

    public void ShutdownServerManually()
    {
        StopServer();
    }

    IEnumerator StartServerAsync()
    {
        string serverPath = Path.Combine(Application.dataPath, "Scripts", "server.py");
        string pythonExec = GetPythonPath();

        if (!File.Exists(serverPath))
        {
            UnityEngine.Debug.LogError($"Server script missing at: {serverPath}");
            yield break;
        }

        var psi = new ProcessStartInfo
        {
            FileName = pythonExec,
            Arguments = $"\"{serverPath}\"",
            WorkingDirectory = Path.Combine(Application.dataPath, "Scripts"),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        try
        {
            serverProcess = new Process { StartInfo = psi };
            serverProcess.OutputDataReceived += OnOutputReceived;
            serverProcess.ErrorDataReceived += OnErrorReceived;

            serverProcess.Start();
            serverProcess.BeginOutputReadLine();
            serverProcess.BeginErrorReadLine();
            UnityEngine.Debug.Log($"Server started (PID: {serverProcess.Id})");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Process start failed: {e.Message}");
        }
    }

    string GetPythonPath()
    {
#if UNITY_EDITOR_OSX
        string[] paths = { "/usr/bin/python3", "/opt/homebrew/bin/python3", "/usr/local/bin/python3", "python3" };
        foreach (var path in paths)
            if (File.Exists(path)) return path;
        return "python3";
#else
        return "python";
#endif
    }

    void StopServer()
    {
        try
        {
            if (serverProcess != null && !serverProcess.HasExited)
            {
                serverProcess.OutputDataReceived -= OnOutputReceived;
                serverProcess.ErrorDataReceived -= OnErrorReceived;

                serverProcess.Kill();
                serverProcess.WaitForExit(1000);
                UnityEngine.Debug.Log("Server stopped");
            }

            serverProcess?.Dispose();
            serverProcess = null;
            serverReady = false;
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Error stopping server: {e.Message}");
        }
    }

    void OnApplicationQuit()
    {
        StopServer();
    }

    void OnOutputReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            UnityEngine.Debug.Log($"[Server STDOUT] {e.Data}");
            if (e.Data.Contains("Running on")) serverReady = true;
        }
    }

    void OnErrorReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            UnityEngine.Debug.LogError($"[Server Error] {e.Data}");
        }
    }
}
