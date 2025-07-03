using System;
using UI.Inventory;
using UnityEngine;

public class InventoryTogglerInteractable : MonoBehaviour, IInteractable
{
    [field: SerializeField] public float holdDuration { get; private set; }
    [field: SerializeField] public SpriteAlignment spriteAlignment { get; private set; }
    [SerializeField] private InteractableVisualSO _interactableVisualSO;

    private InventoryController _inventoryController;
    
    private void Start()
    {
        _inventoryController = Player.Instance.inventoryController;
    }
    
    public InteractionResponse Interact()
    {
        _inventoryController.ToggleInventory(!_inventoryController.state);
        return new();
    }

    public void Forget()
    {
        
    }

    public InteractableVisualSO GetInteractableVisualSO() => _interactableVisualSO;
}
