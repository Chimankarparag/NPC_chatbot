using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GithubAI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] InputField inputField;
    [SerializeField] Button sendButton;
    [SerializeField] ScrollRect chatScroll;
    [SerializeField] GameObject userMessagePrefab;
    [SerializeField] GameObject botMessagePrefab;

    [Header("GitHub API Config")]
    [SerializeField] string apiEndpoint = "https://models.github.ai/inference/chat/completions";
    private string githubToken;

    private Dictionary<string, string> envVariables = new Dictionary<string, string>();
    private List<ChatMessage> chatHistory = new List<ChatMessage>();

    [System.Serializable]
    public class ChatMessage
    {
        public string role;
        public string content;
    }

    void Start()
    {
        sendButton.onClick.AddListener(OnSendMessage);
        inputField.onSubmit.AddListener(_ => OnSendMessage());

        LoadEnvFile();
    }

    // Load .env file, log all, and assign GitHub token if found
    private void LoadEnvFile()
    {
        string filePath = Path.Combine(Application.dataPath, ".env");

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("#"))
                {
                    string[] parts = line.Split(new char[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim().Trim('"');
                        envVariables[key] = value;
                        Debug.Log($"[ENV] {key} = {value}");

                        if (value.StartsWith("ghp_") && string.IsNullOrEmpty(githubToken))
                        {
                            githubToken = value;
                            Debug.Log("GitHub token detected and set from .env");
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(githubToken))
            {
                Debug.LogWarning("No GitHub token found in .env (value starting with 'ghp-')");
                sendButton.interactable = false;
                AddMessageToUI("Error: GitHub token not found in .env", false);
            }
        }
        else
        {
            Debug.LogWarning(".env file not found at: " + filePath);
            sendButton.interactable = false;
            AddMessageToUI("Error: .env file not found. Please add it to Assets/", false);
        }
    }

    void OnSendMessage()
    {
        if (string.IsNullOrWhiteSpace(inputField.text)) return;

        var userMessage = inputField.text;
        AddMessageToUI(userMessage, true);
        inputField.text = "";
        sendButton.interactable = false;

        StartCoroutine(SendChatRequest(userMessage));
    }

    IEnumerator SendChatRequest(string userInput)
    {
        if (string.IsNullOrEmpty(githubToken))
        {
            AddMessageToUI("Error: GitHub API token not configured.", false);
            sendButton.interactable = true;
            yield break;
        }

        chatHistory.Add(new ChatMessage { role = "user", content = userInput });

        var messageList = new List<object>
        {
            new { role = "system", content = "Act natural as a stranger" }
        };

        foreach (var msg in chatHistory)
        {
            messageList.Add(new { role = msg.role, content = msg.content });
        }

        var payloadObj = new
        {
            model = "openai/gpt-4.1",
            messages = messageList.ToArray(),
            temperature = 0.7,
            max_tokens = 150
        };

        string json = JsonUtility.ToJson(payloadObj);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(apiEndpoint, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", $"Bearer {githubToken}");
            req.SetRequestHeader("User-Agent", "Unity-App");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<GitHubApiResponse>(req.downloadHandler.text);
                string reply = response.choices[0].message.content;
                AddMessageToUI(reply, false);
                chatHistory.Add(new ChatMessage { role = "assistant", content = reply });
            }
            else
            {
                AddMessageToUI($"Error: {req.error}", false);
                Debug.LogError($"GitHub API Error: {req.downloadHandler.text}");
            }
        }

        sendButton.interactable = true;
    }

    void AddMessageToUI(string text, bool isUser)
    {
        GameObject message = Instantiate(
            isUser ? userMessagePrefab : botMessagePrefab,
            chatScroll.content
        );

        message.GetComponentInChildren<Text>().text = text;
        StartCoroutine(ScrollToBottom());
    }

    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        chatScroll.verticalNormalizedPosition = 0;
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatScroll.content);
    }

    [System.Serializable]
    private class GitHubApiResponse
    {
        public Choice[] choices;

        [System.Serializable]
        public class Choice
        {
            public Message message;

            [System.Serializable]
            public class Message
            {
                public string content;
            }
        }
    }
}
