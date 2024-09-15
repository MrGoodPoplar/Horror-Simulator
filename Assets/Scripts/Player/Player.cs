using UI.Inventory;
using UnityEngine;

public class Player : MonoBehaviour
{
    [field: SerializeField] public FirstPersonController firstPersonController { get; private set; }
    [field: SerializeField] public ShooterController shooterController { get; private set; }
    [field: SerializeField] public InventoryController inventoryController { get; private set; }
    [field: SerializeField] public InteractController InteractController { get; private set; }
    [field: SerializeField] public HUDController HUDController { get; private set; }
    [field: SerializeField] public PlayerInput playerInput { get; private set; }
    
    public static Player instance { get; private set; }
    
    private void Awake()
    {
        if (instance)
            Destroy(gameObject);
        else
            instance = this;
    }
}
