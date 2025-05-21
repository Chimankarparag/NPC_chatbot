using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.IO;
using System.Collections;

public class ServerRunner : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject chatCanvas;
    public Button talkButton;
    public Button leaveButton;

    [Header("Player Reference")]
    public GameObject player;

    [Header("Server Config")]
    public int serverPort = 5001;

    private Process serverProcess;
    private static ServerRunner instance;
    private bool isPlayerNear = false;
    private bool serverReady = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (talkButton != null) talkButton.onClick.AddListener(StartConversation);
        if (leaveButton != null) leaveButton.onClick.AddListener(StopConversation);

        chatCanvas.SetActive(false);
        talkButton.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);
    }

    public void NotifyPlayerEntered()
    {
        isPlayerNear = true;
        if (!IsRunning)
            talkButton.gameObject.SetActive(true);
    }

    public void NotifyPlayerExited()
    {
        isPlayerNear = false;
        talkButton.gameObject.SetActive(false);
    }

    public void StartConversation()
    {
        LaunchServerManually();
        chatCanvas.SetActive(true);
        talkButton.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(true);
        FreezePlayer(true);
    }

    public void StopConversation()
    {
        ShutdownServerManually();
        chatCanvas.SetActive(false);
        leaveButton.gameObject.SetActive(false);
        FreezePlayer(false);

        if (isPlayerNear)
            talkButton.gameObject.SetActive(true);
    }

    public void LaunchServerManually()
    {
        if (!IsRunning)
            StartCoroutine(StartServerAsync());
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

            serverProcess?.Dispose(); // <--- Important!
            serverProcess = null;
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

    public static bool IsRunning => instance != null && instance.serverProcess != null && !instance.serverProcess.HasExited;

    void FreezePlayer(bool freeze)
    {
        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
            controller.FreezePlayer(freeze);
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
