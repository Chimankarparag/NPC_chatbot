using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NPCInteraction : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcBehaviour;
    public bool useDirectAPI = false;

    [Header("References")]
    public GameObject chatCanvas;
    public ServerRunner serverRunner;
    public GithubAI githubAI;
    public CallFromLocalServer localServerCaller;  
    public GameObject player;
    public Button leaveButton;


    private bool isPlayerNear = false;
    private bool isChatting = false;

    void Start()
    {
        chatCanvas.SetActive(false);
        leaveButton.gameObject.SetActive(false);
        leaveButton.onClick.AddListener(LeaveChat);

        if (useDirectAPI && githubAI != null)
        {
            if (npcBehaviour == null || npcBehaviour == "")
            {
                Debug.LogWarning("NPC Behaviour is not set. Please set it in the inspector.");
                return;
            }
            githubAI.SetNPCBehavior(npcBehaviour); // just set the system prompt

        }
        if (!useDirectAPI && localServerCaller != null)
        {
            localServerCaller.systemPrompt = npcBehaviour;
            localServerCaller.SendMessageFromNPC("Hello"); // initial message to NPC
        }


        }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;

            //This code bloock is for launching the server if it's not running
            if (!useDirectAPI)
            {
                if (serverRunner != null)
                {
                    if (!ServerRunner.IsRunning)
                        serverRunner.LaunchServerManually();
                }
                else
                {
                    Debug.LogWarning("ServerRunner reference is missing.");
                }
            }

             EnableConversation();
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
        var input = player?.GetComponent<PlayerInput>();
        input?.SwitchCurrentActionMap("UI");
        Debug.Log("Switched to UI map");
    }

    private void SwitchToPlayerMap()
    {
        var input = player?.GetComponent<PlayerInput>();
        input?.SwitchCurrentActionMap("Player");
        Debug.Log("Switched to Player map");
    }
}
