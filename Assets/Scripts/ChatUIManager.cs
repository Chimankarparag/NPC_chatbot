// using UnityEngine;
// using UnityEngine.InputSystem;

// public class ChatUIManager : MonoBehaviour
// {
//     [Header("UI Elements")]
//     public GameObject chatCanvas;

//     [Header("References")]
//     public ServerRunner serverRunner;
//     public GameObject player;

//     private bool isPlayerNear = false;
//     private bool serverStarted = false;

//     void Start()
//     {
//         chatCanvas.SetActive(false);
//     }

//     public void EnableConversation()
//     {
//         isPlayerNear = true;

//         if (!serverStarted)
//         {
//             serverRunner.LaunchServerManually();
//             serverStarted = true;
//         }

//         chatCanvas.SetActive(true);

//         var playerInput = player.GetComponent<PlayerInput>();

//         if (playerInput != null)
//         {
//             playerInput.SwitchCurrentActionMap("UI");
//         }
//         else Debug.Log("PlayerInput not found");
//     }

//     public void DisableConversation()
//     {
//         isPlayerNear = false;
//         chatCanvas.SetActive(false);
//          var playerInput = player.GetComponent<PlayerInput>();

//         if (playerInput != null)
//         {
//             playerInput.SwitchCurrentActionMap("Player");
//         }
//         else Debug.Log("PlayerInput not found");
//     }
// }
