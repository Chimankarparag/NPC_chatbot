using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;


public class ChatUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject chatCanvas;
    public Button talkButton;
    public Button leaveButton;

    [Header("References")]
    public GameObject player;
    public ServerRunner serverRunner;

    private bool isPlayerNear = false;

    void Start()
    {
        chatCanvas.SetActive(false);
        talkButton.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);

        talkButton.onClick.AddListener(StartConversation);
        leaveButton.onClick.AddListener(StopConversation);
    }

    public void PlayerEnteredRange()
    {
        isPlayerNear = true;
        if (!ServerRunner.IsRunning)
        {
            talkButton.gameObject.SetActive(true);
            talkButton.interactable = true;
        }
    }

    public void PlayerExitedRange()
    {
        isPlayerNear = false;
        talkButton.gameObject.SetActive(false);
    }

    public void StartConversation()
    {
        if (!talkButton.onClick.GetPersistentEventCount().Equals(1))
        {
            talkButton.onClick.RemoveAllListeners();
            talkButton.onClick.AddListener(StartConversation);
            Debug.Log("Listener removed and added");
        }

        Debug.Log("Talk button clicked");
        StartCoroutine(HandleChatStart());
    }

    private IEnumerator HandleChatStart()
    {
        serverRunner.LaunchServerManually();

        float timeout = 1f, timer = 0f;
        while (!serverRunner.IsReady && timer < timeout)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        chatCanvas.SetActive(true);
        talkButton.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(true);
        FreezePlayer(true);
    }

    public void StopConversation()
    {
        serverRunner.ShutdownServerManually();
        chatCanvas.SetActive(false);
        leaveButton.gameObject.SetActive(false);

        if (isPlayerNear)
            talkButton.gameObject.SetActive(true);

        FreezePlayer(false);
        var playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null) Debug.Log("PlayerInput found");
        else Debug.Log("PlayerInput not found");
        playerInput.SwitchCurrentActionMap("Player");

    }

    private void FreezePlayer(bool freeze)
    {
        var playerInput = player.GetComponent<PlayerInput>();
        var controller = player.GetComponent<PlayerController>();
        if (controller)
            controller.FreezePlayer(freeze);
            
        if (playerInput != null)
        {
            Debug.Log("PlayerInput found");
            playerInput.SwitchCurrentActionMap("UI");
        }
        else Debug.Log("PlayerInput not found");

    }
}
