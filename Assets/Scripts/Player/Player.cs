using UI;
using UI.Hotbar;
using UI.Hotkey_Prompts;
using UI.Inventory;
using UnityEngine;

public class Player : MonoBehaviour
{
    [field: Header("Controllers")]
    [field: SerializeField] public FirstPersonController firstPersonController { get; private set; }
    [field: SerializeField] public ShooterController shooterController { get; private set; }
    [field: SerializeField] public InventoryController inventoryController { get; private set; }
    [field: SerializeField] public InteractController interactController { get; private set; }
    [field: SerializeField] public HUDController HUDController { get; private set; }
    [field: SerializeField] public PlayerInput playerInput { get; private set; }
    [field: SerializeField] public HotbarController hotbarController { get; private set; }
    [field: SerializeField] public HotkeyPromptController hotkeyPromptController { get; private set; }
    [field: SerializeField] public HoldingItemController holdingItemController { get; private set; }
    
    [field: Header("Configurations")]
    [field: SerializeField] public InputBindingSpriteBinder inputBindingSpriteBinder { get; private set; }
    
    public static Player instance { get; private set; }
    
    private void Awake()
    {
        if (instance)
            Destroy(gameObject);
        else
            instance = this;
    }
}
