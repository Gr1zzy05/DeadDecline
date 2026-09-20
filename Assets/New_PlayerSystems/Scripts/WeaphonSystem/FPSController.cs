using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpHeight = 1.2f;
    public float gravity = -15f; 

    [Header("Look")]
    public Transform playerCamera;
    public float mouseSensitivity = 0.2f;
    private float cameraPitch = 0f;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference lookAction;
    public InputActionReference jumpAction;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
        HandleGravityAndJump();
    }

    private void HandleLook()
    {
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
        
        // Pitch (Up/Down) applied to the camera
        cameraPitch -= lookInput.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);

        // Yaw (Left/Right) applied to the player body
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);
    }

    private void HandleMovement()
    {
        // Ground check using CharacterController's built-in flag
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small constant force to snap to the floor
        }

        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    private void HandleGravityAndJump()
    {
        if (jumpAction.action.triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}