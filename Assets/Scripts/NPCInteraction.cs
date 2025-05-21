using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public ChatUIManager chatManager;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            chatManager.EnableConversation();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            chatManager.DisableConversation();
        }
    }
}
