using System;
using System.Linq;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    public event Predicate<bool> OnHUDStateChanged;

    public bool isHUDView { get; private set; }
    
    private Player _player;
    private PlayerInput _playerInput;
    
    private void Start()
    {
        _player = Player.Instance;
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
        ToggleHUDView(!isHUDView);
    }

    public void ToggleHUDView(bool toggle)
    {
        if (gameObject.activeSelf == toggle)
            return;
     
        bool allApproved = OnHUDStateChanged?.GetInvocationList()
            .Cast<Predicate<bool>>()
            .Select(d => d.Invoke(toggle))
            .All(result => result) ?? true;

        if (!allApproved)
            return;
        
        ToggleCursor(toggle);

        isHUDView = toggle;
        gameObject.SetActive(toggle);
        
        _player.firstPersonController.canMove = !toggle;
        _player.firstPersonController.canJump = !toggle;
        _player.hotbarController.canInteract = !toggle;
        _player.shooterController.ToggleWeaponInteraction(!toggle);
        
    }
}