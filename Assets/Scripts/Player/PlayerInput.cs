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
    public event Action OnReload;
    public event Action OnInteract;
    public event Action OnInteractCanceled;
    public event Action OnClick;
    public event Action OnRotate;
    public event Action OnOpenHUD;
    
    private PlayerInputActions _playerInputActions;
    
    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
    }
    
    private void OnEnable()
    {
        _playerInputActions.Player.Jump.performed += OnJumpPerformed;
        _playerInputActions.Player.Fire.performed += OnFirePerformed;
        _playerInputActions.Player.Reload.performed += OnReloadPerformed;
        _playerInputActions.Player.Interact.performed += OnInteractPerformed;
        _playerInputActions.Player.Interact.canceled += OnInteractCanceledPerfomed;
        _playerInputActions.Player.Click.performed += OnClickPerformed;
        _playerInputActions.Player.Rotate.performed += OnRotatePerformed;
        _playerInputActions.Player.OpenHUD.performed += OnOpenHUDPerfmormed;
        
        _playerInputActions.Enable();
    }

    private void OnDisable()
    {
        _playerInputActions.Player.Jump.performed -= OnJumpPerformed;
        _playerInputActions.Player.Jump.performed -= OnFirePerformed;
        _playerInputActions.Player.Reload.performed -= OnReloadPerformed;
        _playerInputActions.Player.Interact.performed -= OnInteractPerformed;
        _playerInputActions.Player.Interact.canceled += OnInteractCanceledPerfomed;
        _playerInputActions.Player.Click.performed -= OnClickPerformed;
        _playerInputActions.Player.Rotate.performed -= OnRotatePerformed;
        _playerInputActions.Player.OpenHUD.performed -= OnOpenHUDPerfmormed;

        _playerInputActions.Disable();
    }

    private void OnDestroy()
    {
        OnDisable();
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
    
    private void OnReloadPerformed(InputAction.CallbackContext obj)
    {
        OnReload?.Invoke();
    }
    
    private void OnInteractPerformed(InputAction.CallbackContext obj)
    {
        OnInteract?.Invoke();
    }
    
    private void OnInteractCanceledPerfomed(InputAction.CallbackContext obj)
    {
        OnInteractCanceled?.Invoke();
    }
    
    private void OnClickPerformed(InputAction.CallbackContext obj)
    {
        OnClick?.Invoke();
    }
    
    private void OnRotatePerformed(InputAction.CallbackContext obj)
    {
        OnRotate?.Invoke();
    }
    
    private void OnOpenHUDPerfmormed(InputAction.CallbackContext obj)
    {
        OnOpenHUD?.Invoke();
    }
}
