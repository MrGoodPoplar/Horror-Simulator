using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance { get; private set; }

    [field: SerializeField] public FirstPersonController firstPersonController { get; private set; }
    [field: SerializeField] public ShooterController shooterController { get; private set; }
    [field: SerializeField] public InventoryController inventoryController { get; private set; }
    [field: SerializeField] public PlayerInput playerInput { get; private set; }

    private void Awake()
    {
        if (instance)
            Destroy(gameObject);
        else
            instance = this;
    }

    public static void ToggleCursor(bool toggle)
    {
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = toggle;
    }
}
