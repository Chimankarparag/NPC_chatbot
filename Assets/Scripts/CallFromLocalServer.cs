using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class CallFromLocalServer : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] InputField inputField;
    [SerializeField] Button sendButton;
    [SerializeField] ScrollRect chatScroll;
    [SerializeField] GameObject userMessagePrefab;
    [SerializeField] GameObject botMessagePrefab;

    [Header("Server Config")]
    [SerializeField] string serverURL = "http://127.0.0.1:5001/api/chat";

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
    }

    void OnSendMessage()
    {
        if ( string.IsNullOrWhiteSpace(inputField.text)) return;

        var userMessage = inputField.text;
        AddMessageToUI(userMessage, true);
        inputField.text = "";
        sendButton.interactable = false;

        StartCoroutine(SendChatRequest(userMessage));
    }

    IEnumerator SendChatRequest(string userInput)
    {
        chatHistory.Add(new ChatMessage
        {
            role = "user",
            content = userInput
        });

        var requestData = new MessageRequest
        {
            messages = chatHistory
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(serverURL, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<MessageResponse>(req.downloadHandler.text);
                chatHistory.Add(new ChatMessage
                {
                    role = "assistant",
                    content = response.message
                });
                AddMessageToUI(response.message, false);
            }
            else
            {
                AddMessageToUI($"Error: {req.error}", false);
                Debug.LogError($"Request failed: {req.error}\n{req.downloadHandler.text}");
            }
        }

        sendButton.interactable = true;
    }

    float cumulativeHeight = 0f;  // Declare this at the class level

    void AddMessageToUI(string text, bool isUser)
    {
        GameObject prefab = isUser ? userMessagePrefab : botMessagePrefab;
        RectTransform message = Instantiate(prefab, chatScroll.content).GetComponent<RectTransform>();

        Text messageText = message.GetComponentInChildren<Text>();
        messageText.text = text;

        LayoutRebuilder.ForceRebuildLayoutImmediate(message); // Important for sizeDelta

        // Place it manually by stacking vertically
        message.anchoredPosition = new Vector2(0, -cumulativeHeight);
        cumulativeHeight += message.sizeDelta.y;

        chatScroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cumulativeHeight);
        chatScroll.verticalNormalizedPosition = 0;
    }


    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        chatScroll.verticalNormalizedPosition = 0;
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatScroll.content);
    }

    [System.Serializable]
    private class MessageRequest
    {
        public List<ChatMessage> messages;
    }

    [System.Serializable]
    private class MessageResponse
    {
        public string message;
    }

}