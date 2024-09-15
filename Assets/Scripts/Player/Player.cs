using UI.Inventory;
using UnityEngine;

public class Player : MonoBehaviour
{
    [field: SerializeField] public FirstPersonController firstPersonController { get; private set; }
    [field: SerializeField] public ShooterController shooterController { get; private set; }
    [field: SerializeField] public InventoryController inventoryController { get; private set; }
    [field: SerializeField] public InteractController InteractController { get; private set; }
    [field: SerializeField] public PlayerInput playerInput { get; private set; }

    [Header("Constraints")]
    [SerializeField] private Transform _hud;
    
    public static Player instance { get; private set; }

    public bool isHUDView { get; private set; }
    
    private void Awake()
    {
        if (instance)
            Destroy(gameObject);
        else
            instance = this;
    }

    private void Start()
    {
        ToggleHUDView(isHUDView);
        
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
        isHUDView = !isHUDView;
        ToggleHUDView(isHUDView);
    }

    // TODO: move to separate class and probably just pause game in inventory
    // TODO: OnHUDstateChanged event needed
    public void ToggleHUDView(bool toggle)
    {
        ToggleCursor(toggle);

        isHUDView = toggle;
        _hud.gameObject.SetActive(toggle);
        firstPersonController.canMove = !toggle;
        shooterController.ToggleWeaponInteraction(!toggle);
    }
}
