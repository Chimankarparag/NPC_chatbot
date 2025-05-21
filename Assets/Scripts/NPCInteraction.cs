using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NPCInteraction : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcPrompt;

    [Header("References")]
    public GameObject chatCanvas;
    public ServerRunner serverRunner;
    public GameObject player;
    public Button leaveButton;
    private bool isPlayerNear = false;
    private bool isChatting = false;
    private bool serverStarted = false;
    void Start()
    {
        chatCanvas.SetActive(false);
        leaveButton.gameObject.SetActive(false);
        leaveButton.onClick.AddListener(LeaveChat);

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;

            if (!ServerRunner.IsRunning)
                serverRunner.LaunchServerManually();

            if (!isChatting)
            {
                // serverRunner.SendPromptToServer(npcPrompt);
                EnableConversation();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (!isChatting) chatCanvas.SetActive(false);
        }
    }
    public void EnableConversation()
    {

        isChatting = true;
        chatCanvas.SetActive(true);
        leaveButton.gameObject.SetActive(true);
        SwitchToUIMap();
    }

    private void LeaveChat()
    {
        isChatting = false;
        chatCanvas.SetActive(false);
        leaveButton.gameObject.SetActive(false);
        SwitchToPlayerMap();
    }


    private void SwitchToUIMap()
    {
        var input = player.GetComponent<PlayerInput>();
        input?.SwitchCurrentActionMap("UI");
        Debug.Log("Switched to UI map");
    }

    private void SwitchToPlayerMap()
    {
        var input = player.GetComponent<PlayerInput>();
        input?.SwitchCurrentActionMap("Player");
         Debug.Log("Switched to Player map");
    }

}
