using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpHeight = 1.2f;
    public float gravity = -15f; 

    [Header("Gameplay footstep noise")]
    [Min(0)] public float footstepLoudness = 1f;
    [Min(0)] public float footstepFullVolumeRadius = 2f;
    [Min(0.1f)] public float footstepMaxRadius = 10f;
    [Min(0.1f)] public float stepDistance = 1.5f;
    private float distanceSinceStep;

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
        cameraPitch -= lookInput.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);
    }

    private void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        Vector3 before = transform.position;
        controller.Move(move * moveSpeed * Time.deltaTime);
        Vector3 travelled = transform.position - before;
        travelled.y = 0;
        if (isGrounded && travelled.sqrMagnitude > 0.000001f)
        {
            distanceSinceStep += travelled.magnitude;
            if (distanceSinceStep >= Mathf.Max(0.1f, stepDistance))
            {
                distanceSinceStep = 0;
                NoiseSystem.Emit(transform.position, footstepLoudness, footstepFullVolumeRadius,
                    footstepMaxRadius, transform, NoiseSystem.NoiseKind.Footstep);
            }
        }
        else distanceSinceStep = 0;
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