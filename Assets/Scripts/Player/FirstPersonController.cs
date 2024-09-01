using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class FirstPersonController : MonoBehaviour
{
    public bool canSprint { get; set; } = true;
    public bool canMove { get; set; } = true;
    public bool canJump { get; set; } = true;
    public bool canCrouch { get; set; } = true;
    public float cameraFOV
    {
        get => _playerCamera.fieldOfView;
        set => _playerCamera.fieldOfView = value;
    }

    [Header("Movement Settings")]
    [SerializeField] private float _walkSpeed = 3.0f;
    [SerializeField] private float _sprintSpeed = 5.0f;
    
    [Header("Look Settings")]
    [SerializeField, Range(1, 180)] private float _lookLimit = 80.0f;
    [field: SerializeField, Range(0.01f, 1)] public float sensitivity { get; set; } = 0.5f;

    [Header("Jump Settings")]
    [SerializeField] private float _gravity = 30.0f;
    [SerializeField] private float _jumpForce = 8.0f;
    
    [Header("Crouch Settings")]
    [SerializeField] private float _crouchSpeed = 1.5f;
    [SerializeField] private float _crouchingHeight = 1f;
    [SerializeField] private float _timeToCrouch = 0.3f;
    [SerializeField] private Vector3 _crouchingCenter = new (0, 0.5f, 0);
    [SerializeField] private Vector3 _standingCenter = new (0, 0, 0);

    [Header("Constraints")]
    [SerializeField] private Camera _playerCamera;
    
    private CharacterController _characterController;
    private PlayerInput _playerInput;
    
    private Vector2 _currentInput;
    private Vector3 _moveDirection;

    private float _rotationX;
    private bool _isCrouching;
    private bool _isCrouchingTransition;
    private float _standingHeight;
    private float _stepOffset;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();
        
        _playerInput.OnJump += OnJump;

        _stepOffset = _characterController.stepOffset;
        _standingHeight = _characterController.height;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        _playerInput.OnJump -= OnJump;
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
        float speed = _walkSpeed;
        
        if (_isCrouching)
        {
            speed = _crouchSpeed;
        }
        else if (canSprint && _playerInput.isSprinting)
        {
            speed = _sprintSpeed;
        }
        
        return speed;
    }
    
    private void HandleWalking()
    {
        _currentInput = _playerInput.move.normalized * GetMovementSpeed();
        
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
        bool crouchStateChanged = _playerInput.isCrouching != _isCrouching;
        bool cannotStandUp = _isCrouching && IsHittingCeiling(_standingHeight - _crouchingHeight);

        if (_isCrouchingTransition || !crouchStateChanged || cannotStandUp)
            return;

        _isCrouchingTransition = true;

        bool isCrouching = canCrouch && _playerInput.isCrouching && _characterController.isGrounded;
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
        _rotationX -= _playerInput.look.y * sensitivity;
        _rotationX = Mathf.Clamp(_rotationX, -_lookLimit, _lookLimit);
        
        _playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, _playerInput.look.x * sensitivity, 0);
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
    
    private void OnJump()
    {
        if (canJump && _characterController.isGrounded)
        {
            _moveDirection.y = _jumpForce;
        }
    }
}
