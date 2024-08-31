using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    
    [field: Header("Main Settings")]
    [field: SerializeField] public bool canSprint { get; private set; } = true;
    [field: SerializeField] public bool canMove { get; private set; } = true;
    [field: SerializeField] public bool canJump { get; private set; } = true;
    [field: SerializeField] public bool canCrouch { get; private set; } = true;

    [Header("Movement Settings")]
    [SerializeField] private float _walkSpeed = 3.0f;
    [SerializeField] private float _sprintSpeed = 5.0f;
    [SerializeField, Range(0, 1)] private float _sprintingThreshold = 0.5f;
    
    [Header("Look Settings")]
    [SerializeField, Range(0.01f, 1)] private float _sensitivity = 0.5f;
    [SerializeField, Range(1, 180)] private float _lookLimit = 80.0f;
    
    [Header("Jump Settings")]
    [SerializeField] private float _gravity = 30.0f;
    [SerializeField] private float _jumpForce = 8.0f;
    
    [Header("Crouch Settings")]
    [SerializeField] private float _crouchSpeed = 1.5f;
    [SerializeField] private float _crouchingHeight = 1f;
    [SerializeField] private float _timeToCrouch = 0.3f;
    [SerializeField] private Vector3 _crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 _standingCenter = new Vector3(0, 0, 0);

    private Camera _playerCamera;
    private CharacterController _characterController;
    private PlayerInputActions _playerInputActions;
    
    private Vector2 _currentInput;
    private Vector3 _moveDirection;

    private float _rotationX;
    private bool _isCrouching = false;
    private bool _isCrouchingTransition = false;
    private float _standingHeight;
    private float _stepOffset;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        
        _playerCamera = GetComponentInChildren<Camera>();
        _characterController = GetComponent<CharacterController>();

        _stepOffset = _characterController.stepOffset;
        _standingHeight = _characterController.height;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        _playerInputActions.Player.Jump.performed += OnJump;
        _playerInputActions.Enable();
    }

    private void OnDisable()
    {
        _playerInputActions.Player.Jump.performed -= OnJump;
        _playerInputActions.Disable();
    }

    private void Update()
    {
        if (canMove)
        {
            HandleCrouching();
            HandleWalking();
            HandleMouseLook();
            
            ApplyFinalMovements();
        }
    }

    private float GetMovementSpeed()
    {
        if (_isCrouching)
        {
            return _crouchSpeed;
        }
        else if (canSprint)
        {
            float sprintValue = _playerInputActions.Player.Sprint.ReadValue<float>();
            bool isSprinting = sprintValue > _sprintingThreshold;
            
            return isSprinting ? _sprintSpeed : _walkSpeed;
        }
        
        return _walkSpeed;
    }

    private bool IsCrouchingInput()
    {
        return _playerInputActions.Player.Crouch.ReadValue<float>() > 0.5f;
    }
    
    private void HandleWalking()
    {
        _currentInput = _playerInputActions.Player.Move.ReadValue<Vector2>().normalized * GetMovementSpeed();
        
        _moveDirection = new Vector3(
            transform.right.x * _currentInput.x + transform.forward.x * _currentInput.y,
            _moveDirection.y,
            transform.right.z * _currentInput.x + transform.forward.z * _currentInput.y
        );
    }

    private bool IsHittingCeiling(float distance = 0)
    {
        Vector3 rayOrigin = transform.position + Vector3.up * (_characterController.height / 2f);
        float offset = 0.1f;
        
        return Physics.Raycast(rayOrigin, Vector3.up, distance + offset);
    }
    
    private async void HandleCrouching()
    {
        bool crouchStateChanged = IsCrouchingInput() != _isCrouching;
        bool cannotStandUp = _isCrouching && IsHittingCeiling(_standingHeight - _crouchingHeight);

        if (_isCrouchingTransition || !crouchStateChanged || cannotStandUp)
            return;

        _isCrouchingTransition = true;

        bool isCrouching = canCrouch && IsCrouchingInput() && _characterController.isGrounded;
        float elapsedTime = 0f;
        float targetHeight = isCrouching ? _crouchingHeight : _standingHeight;
        Vector3 targetCenter = isCrouching ? _crouchingCenter : _standingCenter;

        while (elapsedTime < _timeToCrouch)
        {
            _characterController.height = Mathf.Lerp(_characterController.height, targetHeight, elapsedTime / _timeToCrouch);
            _characterController.center = Vector3.Lerp(_characterController.center, targetCenter, elapsedTime / _timeToCrouch);

            elapsedTime += Time.deltaTime;

            await Task.Yield();
        }
        
        _characterController.height = targetHeight;
        _characterController.center = targetCenter;
        _isCrouchingTransition = false;
        _isCrouching = isCrouching;
    }
    
    private void HandleMouseLook()
    {
        _rotationX -= _playerInputActions.Player.Look.ReadValue<Vector2>().y * _sensitivity;
        _rotationX = Mathf.Clamp(_rotationX, -_lookLimit, _lookLimit);
        
        _playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, _playerInputActions.Player.Look.ReadValue<Vector2>().x * _sensitivity, 0);
    }
    
    private void ApplyFinalMovements()
    {
        if (!_characterController.isGrounded)
        {
            _moveDirection.y -= _gravity * Time.deltaTime;
            _characterController.stepOffset = 0;
        }
        else
        {
            _characterController.stepOffset = _stepOffset;
        }

        _characterController.Move(_moveDirection * Time.deltaTime);
    }
    
    private void OnJump(InputAction.CallbackContext obj)
    {
        if (canJump && _characterController.isGrounded)
        {
            _moveDirection.y = _jumpForce;
        }
    }
}
