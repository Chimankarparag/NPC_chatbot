using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class GithubAI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private InputField inputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private ScrollRect chatScroll;
    [SerializeField] private RectTransform sent;
    [SerializeField] private RectTransform received;

    [Header("GitHub API")]
    [SerializeField] private string apiEndpoint = "https://models.github.ai/inference/chat/completions";
    private string githubToken;
    private bool initialized = false;

    private string prompt = "You are Jessica, 19 years old, a student at the University of California, Berkeley. You are a friendly and helpful assistant. You are an NPC currently in a chat with a user.Do not mention you are AI. Convince the user to be your boyfriend";


    private float height;
    private List<ChatMessage> chatHistory = new List<ChatMessage>();
    private Dictionary<string, string> envVariables = new Dictionary<string, string>();

    [System.Serializable]
    public class ChatMessage
    {
        [JsonProperty("role")]
        public string role;

        [JsonProperty("content")]
        public string content;
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

    private void Start()
    {
        sendButton.onClick.AddListener(SendReply);
        LoadEnvFile();
    }

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

                        if (value.StartsWith("ghp_") && string.IsNullOrEmpty(githubToken))
                        {
                            githubToken = value;
                            Debug.Log($"[ENV] GitHub token found: {githubToken}");
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(githubToken))
            {
                Debug.LogWarning("GitHub token not found in .env");
                sendButton.interactable = false;
                AppendMessage(new ChatMessage { role = "assistant", content = "❌ GitHub token missing from .env." });
            }
        }
        else
        {
            Debug.LogWarning(".env file not found at: " + filePath);
            sendButton.interactable = false;
            AppendMessage(new ChatMessage { role = "assistant", content = "❌ .env file not found in Assets/" });
        }
    }

    private void SendReply()
    {
        if (string.IsNullOrWhiteSpace(inputField.text)) return;

        // Inject system prompt only once
        if (!initialized)
        {
            chatHistory.Insert(0, new ChatMessage
            {
                role = "system",
                content = prompt
            });
            initialized = true;
        }

        var userMessage = new ChatMessage
        {
            role = "user",
            content = inputField.text
        };

        chatHistory.Add(userMessage);
        AppendMessage(userMessage);

        inputField.text = "";
        inputField.enabled = false;
        sendButton.enabled = false;

        StartCoroutine(SendChatRequest());
    }


    private IEnumerator SendChatRequest()
    {
        var payload = new
        {
            model = "gpt-4.1",
            provider = "azureml",
            publisher = "openai",
            messages = chatHistory,
            temperature = 1.0,
            top_p = 1.0,
            max_tokens = 150
        };


        string json = JsonConvert.SerializeObject(payload);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(apiEndpoint, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", $"Bearer {githubToken}");
            req.SetRequestHeader("User-Agent", "Unity-App");

            Debug.Log("Payload: " + json);

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var response = JsonConvert.DeserializeObject<GitHubApiResponse>(req.downloadHandler.text);
                var reply = response.choices[0].message.content.Trim();

                var botMessage = new ChatMessage
                {
                    role = "assistant",
                    content = reply
                };

                chatHistory.Add(botMessage);
                AppendMessage(botMessage);
            }
            else
            {
                Debug.LogError($"GitHub API Error: {req.downloadHandler.text}");
                AppendMessage(new ChatMessage { role = "assistant", content = $"❌ API Error: {req.error}" });
            }

            inputField.enabled = true;
            sendButton.enabled = true;
        }
    }

    private void AppendMessage(ChatMessage message)
    {
        chatScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

        var item = Instantiate(message.role == "user" ? sent : received, chatScroll.content);
        item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.content;
        item.anchoredPosition = new Vector2(0, -height);
        LayoutRebuilder.ForceRebuildLayoutImmediate(item);
        height += item.sizeDelta.y;
        chatScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        chatScroll.verticalNormalizedPosition = 0;
    }
    public void SetNPCBehavior(string behavior)
    {
        if (!initialized)
        {
            this.prompt = behavior;
            chatHistory.Insert(0, new ChatMessage
            {
                role = "system",
                content = this.prompt
            });
            initialized = true;
            Debug.Log("NPC Behavior set to in GithubAI.cs : " + behavior);
        }
    }

}
