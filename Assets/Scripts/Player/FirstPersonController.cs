using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class FirstPersonController : MonoBehaviour, IMoveable
{
    public event Action OnExhausted;
    public event Action OnLanded;
    
    public bool canSprint { get; set; } = true;
    public bool canMove { get; set; } = true;
    public bool canJump { get; set; } = true;
    public bool canCrouch { get; set; } = true;
    public bool canStepOffset { get; set; } = true;
    
    public PlayerInput playerInput { get; private set; }
    public bool isGrounded => _characterController.isGrounded;
    public float velocity => Mathf.Clamp01(_characterController.velocity.magnitude / _sprintSpeed);
    public float height => _characterController.height;
    public Vector3 characterControllerCenter => _characterController.center;
    public Vector3 moveDirection => _moveDirection;

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

    [Header("Stamina Settings")]
    [SerializeField] private float _staminaDuration = 15.0f;
    [SerializeField] private float _staminaRecoverySpeed = 1.5f;
    [SerializeField, Range(0, 1)] private float _minStaminaForSprint = 0.3f;
    [SerializeField, Range(0, 1)] private float _minPenaltyStaminaForSprint = 0.5f;

    [field: Header("Constraints")]
    [field: SerializeField] public Camera playerCamera { get; private set; }
    
    private CharacterController _characterController;
    
    private Vector2 _currentInput;
    private Vector3 _moveDirection;

    private float _rotationX;
    private bool _isSprinting;
    private bool _isCrouching;
    private bool _isCrouchingTransition;
    private bool _isStaminaPenalty;
    private bool _jumped;
    private float _standingHeight;
    private float _stepOffset;
    private float _currentStamina;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        
        playerInput.OnJump += OnJump;

        _stepOffset = _characterController.stepOffset;
        _standingHeight = _characterController.height;
        _currentStamina = _staminaDuration;

        if (!canStepOffset)
            _characterController.stepOffset = 0;
    }

    private void OnDestroy()
    {
        playerInput.OnJump -= OnJump;
    }

    private void Update()
    {
        if (canMove)
        {
            HandleCrouching();
            HandleWalking();
            HandleMouseLook();
        }
        else
        {
            _moveDirection = Vector3.zero + Vector3.up * _moveDirection.y;
        }
        
        ApplyFinalMovements();
    }

    public float CalculateMovementSpeed()
    {
        float speed = _isCrouching ? _crouchSpeed : _walkSpeed;
        float minStaminaForSprint = _isStaminaPenalty ? _minPenaltyStaminaForSprint : _minStaminaForSprint;
        
        if (canSprint && !_isCrouching && playerInput.isSprinting && (GetStaminaPercentage() > minStaminaForSprint || _isSprinting))
        {
            SetStamina(_currentStamina - Time.deltaTime);
            speed = _sprintSpeed;
            _isSprinting = _currentStamina > 0;
            _isStaminaPenalty = false;

            if (!_isSprinting)
            {
                _isStaminaPenalty = true;
                OnExhausted?.Invoke();
            }
        }
        else
        {
            _isSprinting = false;
            
            if (_currentStamina < _staminaDuration)
            {
                SetStamina(_currentStamina + Time.deltaTime * _staminaRecoverySpeed);
            }
        }

        return speed;
    }


    public float GetStaminaPercentage()
    {
        return _currentStamina > 0
            ? _currentStamina / _staminaDuration
            : 0;
    }

    private void SetStamina(float value)
    {
        _currentStamina = Mathf.Clamp(value, 0, _staminaDuration);
    }
    
    private void HandleWalking()
    {
        _currentInput = playerInput.move.normalized * CalculateMovementSpeed();
        
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
        bool crouchStateChanged = playerInput.isCrouching != _isCrouching;
        bool cannotStandUp = _isCrouching && IsHittingCeiling(_standingHeight - _crouchingHeight);

        if (_isCrouchingTransition || !crouchStateChanged || cannotStandUp)
            return;

        _isCrouchingTransition = true;

        bool isCrouching = canCrouch && playerInput.isCrouching;
        float elapsedTime = 0f;
        float targetHeight = isCrouching ? _crouchingHeight : _standingHeight;
        Vector3 targetCenter = isCrouching ? _crouchingCenter : _standingCenter;

        while (elapsedTime < _timeToCrouch)
        {
            _characterController.height = Mathf.Lerp(_characterController.height, targetHeight, elapsedTime / _timeToCrouch);
            _characterController.center = Vector3.Lerp(_characterController.center, targetCenter, elapsedTime / _timeToCrouch);
            
            // junk solution to update position according to skin width
            _characterController.SimpleMove(transform.forward * 0.05f);
            _characterController.SimpleMove(-transform.forward * 0.05f);

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
        _rotationX -= playerInput.look.y * sensitivity;
        _rotationX = Mathf.Clamp(_rotationX, -_lookLimit, _lookLimit);
        
        playerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, playerInput.look.x * sensitivity, 0);
    }
    
    private void ApplyFinalMovements()
    {
        if (!isGrounded)
        {
            _jumped = true;
            _moveDirection.y -= _gravity * Time.deltaTime;
            _characterController.stepOffset = 0;
        }
        else
        {
            if (_jumped)
            {
                OnLanded?.Invoke();
                _jumped = false;
            }
            
            _characterController.stepOffset = canStepOffset ? _stepOffset : 0;
        }

        _characterController.Move(_moveDirection * Time.deltaTime);
    }
    
    private void OnJump()
    {
        if (canJump && isGrounded)
        {
            _moveDirection.y = _jumpForce;
        }
    }
}
