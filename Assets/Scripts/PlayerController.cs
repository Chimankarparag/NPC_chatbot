using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float lookSensitivity = 2f;
    public Transform cameraTransform;

    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    public float groundCheckDistance = 0.4f;
    public Transform groundCheck;
    public LayerMask groundMask;

    private CharacterController controller;
    private PlayerInput input;
    private InputAction moveAction;
    private InputAction lookAction;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;

    private float cameraPitch = 0f;
    private bool isGrounded;
    private bool isFrozen = false;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
        moveAction = input.actions["Move"];
        lookAction = input.actions["Look"];
    }

    void Update()
    {
        if (isFrozen) return;

        // Gravity check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundMask);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;  // Small downward force to keep grounded

        moveInput = moveAction.ReadValue<Vector2>();
        lookInput = lookAction.ReadValue<Vector2>();

        Move();
        LookAround();

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Move()
    {
        Vector3 direction = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(direction * moveSpeed * Time.deltaTime);
    }

    void LookAround()
    {
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    public void FreezePlayer(bool freeze)
    {
        isFrozen = freeze;
        Cursor.lockState = freeze ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = freeze;
    }
}
