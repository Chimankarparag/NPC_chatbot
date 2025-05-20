using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.Net.Sockets;

public class ServerRunner : MonoBehaviour
{
    [Header("Server Config")]
    public bool autoStartServer = true;
    public float serverStartDelay = 2f;
    public int serverPort = 5001;
    public float timeoutSeconds = 10f;

    private Process serverProcess;
    private bool serverReady;
    private bool serverStartFailed;

    void Start()
    {
        if (autoStartServer)
        {
            StartCoroutine(ServerStartRoutine());
        }
    }

    IEnumerator ServerStartRoutine()
    {
        yield return new WaitForSeconds(serverStartDelay);
        yield return StartCoroutine(StartServerAsync());
    }

    IEnumerator StartServerAsync()
    {
        string serverPath = "";
        string pythonExec = "";
        bool initializationError = false;

        try
        {
            serverPath = Path.Combine(Application.streamingAssetsPath, "server.py");
            pythonExec = GetPythonPath();

            if (!File.Exists(serverPath))
            {
                UnityEngine.Debug.LogError($"Server script missing at: {serverPath}");
                initializationError = true;
                yield break;
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Initialization error: {e.Message}");
            initializationError = true;
            yield break;
        }

        if (initializationError) yield break;

        // Server process setup
        var psi = new ProcessStartInfo
        {
            FileName = pythonExec,
            Arguments = $"\"{serverPath}\"",
            WorkingDirectory = Application.streamingAssetsPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        try
        {
            serverProcess = new Process { StartInfo = psi };
            
            serverProcess.OutputDataReceived += (sender, e) => 
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    UnityEngine.Debug.Log($"[Server] {e.Data}");
                    if (e.Data.Contains("Starting server")) serverReady = true;
                }
            };

            serverProcess.ErrorDataReceived += (sender, e) => 
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    UnityEngine.Debug.LogError($"[Server Error] {e.Data}");
                    serverStartFailed = true;
                }
            };

            serverProcess.Start();
            serverProcess.BeginOutputReadLine();
            serverProcess.BeginErrorReadLine();
            
            UnityEngine.Debug.Log($"Server process started (PID: {serverProcess.Id})");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Process start failed: {e.Message}");
            yield break;
        }

        // Wait for server readiness with timeout
        float startTime = Time.time;
        bool portOpen = false;
        
        while (Time.time - startTime < timeoutSeconds && !serverStartFailed)
        {
            portOpen = IsPortOpen();
            
            if (serverReady && portOpen)
            {
                UnityEngine.Debug.Log("Server ready - accepting connections");
                yield break;
            }
            
            yield return null; // Wait one frame
        }

        if (!serverReady || !portOpen)
        {
            UnityEngine.Debug.LogError($"Server failed to start within {timeoutSeconds} seconds");
            StopServer();
        }
    }

    bool IsPortOpen()
    {
        try
        {
            using (var client = new TcpClient())
            {
                var result = client.BeginConnect("127.0.0.1", serverPort, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(500);
                client.EndConnect(result);
                return success;
            }
        }
        catch
        {
            return false;
        }
    }

    string GetPythonPath()
    {
        #if UNITY_EDITOR_OSX
                string[] paths = {
                    "/usr/bin/python3",
                    "/opt/homebrew/bin/python3",
                    "/usr/local/bin/python3",
                    "python3"
                };

                foreach (var path in paths)
                {
                    if (File.Exists(path))
                    {
                        UnityEngine.Debug.Log($"Using Python at: {path}");
                        return path;
                    }
                }
                return "python3";
        #else
                return "python";
        #endif
    }

    void OnApplicationQuit()
    {
        StopServer();
    }

    void StopServer()
    {
        try
        {
            if (serverProcess != null && !serverProcess.HasExited)
            {
                serverProcess.Kill();
                serverProcess.WaitForExit(1000);
                UnityEngine.Debug.Log("Server stopped");
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Error stopping server: {e.Message}");
        }
    }
}