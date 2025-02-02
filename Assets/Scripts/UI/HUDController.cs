using System;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    public event Action<bool> OnHUDStateChanged;

    public bool isHUDView { get; private set; }
    
    private Player _player;
    private PlayerInput _playerInput;
    
    private void Start()
    {
        _player = Player.instance;
        _playerInput = _player.playerInput;
        
        _playerInput.OnOpenHUD += OnOpenHUDPerformed;
        
        ToggleHUDView(isHUDView);
    }

    private void OnDestroy()
    {
        _playerInput.OnOpenHUD -= OnOpenHUDPerformed;
    }

    public static void ToggleCursor(bool toggle)
    {
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = toggle;
    }
    
    private void OnOpenHUDPerformed()
    {
        isHUDView = !isHUDView;
        ToggleHUDView(isHUDView);
    }

    public void ToggleHUDView(bool toggle)
    {
        if (gameObject.activeSelf == toggle)
            return;
        
        ToggleCursor(toggle);

        isHUDView = toggle;
        gameObject.SetActive(toggle);
        _player.firstPersonController.canMove = !toggle;
        _player.firstPersonController.canJump = !toggle;
        _player.hotbarController.canInteract = !toggle;
        _player.shooterController.ToggleWeaponInteraction(!toggle);
        
        OnHUDStateChanged?.Invoke(toggle);
    }
}