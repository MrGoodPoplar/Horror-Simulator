using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance { get; private set; }

    [field: SerializeField] public FirstPersonController firstPersonController { get; private set; }
    [field: SerializeField] public ShooterController shooterController { get; private set; }
    [field: SerializeField] public InventoryController inventoryController { get; private set; }
    [field: SerializeField] public PlayerInput playerInput { get; private set; }

    [Header("Constraints")]
    [SerializeField] private Transform _hud;
    
    private bool _inventoryOpened;
    
    private void Awake()
    {
        if (instance)
            Destroy(gameObject);
        else
            instance = this;
        
        
    }

    private void Start()
    {
        ToggleHUDView(_inventoryOpened);
        
        playerInput.OnOpenHUD += OnOpenHUDPerformed;
    }

    private void OnDestroy()
    {
        playerInput.OnOpenHUD -= OnOpenHUDPerformed;
    }

    public static void ToggleCursor(bool toggle)
    {
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = toggle;
    }
    
    private void OnOpenHUDPerformed()
    {
        _inventoryOpened = !_inventoryOpened;
        ToggleHUDView(_inventoryOpened);
        
        // TODO: move to separate class and probably just pause game in inventory
        firstPersonController.canMove = !_inventoryOpened;
        shooterController.ToggleWeaponInteraction(!_inventoryOpened);
    }

    private void ToggleHUDView(bool toggle)
    {
        ToggleCursor(toggle);
        
        _hud.gameObject.SetActive(toggle);
    }
}
