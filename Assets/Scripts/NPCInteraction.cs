using UnityEngine;
using UnityEngine.UI;

public class NPCInteraction : MonoBehaviour
{
    public GameObject talkButton;
    public GameObject leaveButton;
    public GameObject chatCanvas;
    public GameObject player;

    private bool isPlayerNear = false;

    void Start()
    {
        talkButton.SetActive(false);
        leaveButton.SetActive(false);
        chatCanvas.SetActive(false);

        talkButton.GetComponent<Button>().onClick.AddListener(StartChat);
        leaveButton.GetComponent<Button>().onClick.AddListener(LeaveChat);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            talkButton.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            talkButton.SetActive(false);
        }
    }

    void StartChat()
    {
        chatCanvas.SetActive(true);
        talkButton.SetActive(false);
        leaveButton.SetActive(true);
        FreezePlayer();
    }

    void LeaveChat()
    {
        chatCanvas.SetActive(false);
        leaveButton.SetActive(false);
        if (isPlayerNear)
        {
            talkButton.SetActive(true);
        }
        UnfreezePlayer();
    }

    void FreezePlayer()
    {
        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
            controller.FreezePlayer(true);
    }

    void UnfreezePlayer()
    {
        var controller = player.GetComponent<PlayerController>();
        if (controller != null)
            controller.FreezePlayer(false);
    }

}
