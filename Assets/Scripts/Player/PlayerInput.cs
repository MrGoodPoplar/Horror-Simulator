using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public bool isSprinting { get; private set; }
    public bool isCrouching { get; private set; }
    public bool isAiming { get; private set; }
    public Vector2 look { get; private set; }
    public Vector2 move { get; private set; }

    public event Action OnJump;
    public event Action OnFire;
    
    private PlayerInputActions _playerInputActions;
    
    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
    }
    
    private void OnEnable()
    {
        _playerInputActions.Player.Jump.performed += OnJumpPerformed;
        _playerInputActions.Player.Fire.performed += OnFirePerformed;
        _playerInputActions.Enable();
    }

    private void OnDisable()
    {
        _playerInputActions.Player.Jump.performed -= OnJumpPerformed;
        _playerInputActions.Player.Jump.performed -= OnFirePerformed;
        _playerInputActions.Disable();
    }

    private void Update()
    {
        isSprinting = _playerInputActions.Player.Sprint.ReadValue<float>() > 0.5f;
        isCrouching = _playerInputActions.Player.Crouch.ReadValue<float>() > 0.5f;
        isAiming = _playerInputActions.Player.Aim.ReadValue<float>() > 0.5f;
        
        look = _playerInputActions.Player.Look.ReadValue<Vector2>();
        move = _playerInputActions.Player.Move.ReadValue<Vector2>();
    }

    private void OnJumpPerformed(InputAction.CallbackContext obj)
    {
        OnJump?.Invoke();
    }
    
    private void OnFirePerformed(InputAction.CallbackContext obj)
    {
        OnFire?.Invoke();
    }
}
