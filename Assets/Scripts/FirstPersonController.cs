using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float jumpHeight = 1.2f;
    public float gravity = -19.62f;

    [Header("Look")]
    public float mouseSensitivity = 0.15f;
    public float verticalClamp = 85f;
    public Transform cameraHolder;

    CharacterController _cc;
    Vector2 _moveInput;
    Vector2 _lookInput;
    Vector3 _velocity;
    float _xRotation;
    bool _isSprinting;
    bool _jumpPressed;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Called by PlayerInput component (Send Messages mode)
    void OnMove(InputValue value)   => _moveInput   = value.Get<Vector2>();
    void OnLook(InputValue value)   => _lookInput   = value.Get<Vector2>();
    void OnSprint(InputValue value) => _isSprinting = value.isPressed;
    void OnJump(InputValue value)   { if (value.isPressed) _jumpPressed = true; }

    void Update()
    {
        HandleLook();
        HandleMovement();
    }

    void HandleLook()
    {
        float mouseX = _lookInput.x * mouseSensitivity;
        float mouseY = _lookInput.y * mouseSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -verticalClamp, verticalClamp);

        if (cameraHolder != null)
            cameraHolder.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        bool grounded = _cc.isGrounded;
        if (grounded && _velocity.y < 0f)
            _velocity.y = -2f;

        float speed = _isSprinting ? sprintSpeed : walkSpeed;
        Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        _cc.Move(move * speed * Time.deltaTime);

        if (_jumpPressed && grounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _jumpPressed = false;
        }
        else
        {
            _jumpPressed = false;
        }

        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }
}
