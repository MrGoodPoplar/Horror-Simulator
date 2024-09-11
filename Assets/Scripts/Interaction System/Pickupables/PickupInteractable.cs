using UnityEngine;

public class PickupInteractable : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private string _pickupText;
    [SerializeField] private Vector2Int _quantityRange = new(1, 1);
    [SerializeField, Range(0, 1)] private float _maxQuantityChance = 0.2f;
    [SerializeField] private CalculationType _calculationType = CalculationType.Exponential;
    [SerializeField] private bool _destroyOnGrab = false;
    
    enum CalculationType
    {
        Probabilistic,
        Exponential
    }
    
    [field: Header("Constraints")]
    [field: SerializeField] public InteractableVisualSO InteractableVisualSO { get; private set; }
    [field: SerializeField] public InventoryItemSO inventoryItem { get; private set; }

    private int _quantityLeftover;
    
    public bool Interact(InteractController interactController)
    {
        int quantity = _quantityLeftover == 0 ? GetQuantity() : _quantityLeftover;
        bool result = Player.instance.inventoryController.AddItemToInventory(inventoryItem, quantity, out _quantityLeftover);

        if (_quantityLeftover > 0)
        {
            Debug.Log($"Leftover {_quantityLeftover} Adding to temp...");
            result = Player.instance.inventoryController.AddItemToInventory(inventoryItem, quantity, out _quantityLeftover, true);
        }
        
        if (result && _destroyOnGrab)
            Destroy(gameObject);
        else if (result)
            gameObject.SetActive(false);
        
        return true;
    }

    private int GetQuantity()
    {
        switch (_calculationType)
        {
            case CalculationType.Exponential: 
                return GetExponentialQuantity();
            case CalculationType.Probabilistic: 
                return GetProbabilisticQuantity();
            default:
                return 0;
        }
    }
    
    private int GetProbabilisticQuantity()
    {
        int totalQuantity = (int)_quantityRange.x;
        float chanceToRaiseQuantity = 1.0f - _maxQuantityChance;

        for (int i = (int)_quantityRange.x; i < (int)_quantityRange.y; i++)
        {
            if (Random.value > chanceToRaiseQuantity)
            {
                totalQuantity++;
            }
        }

        return Mathf.Clamp(totalQuantity, _quantityRange.x, _quantityRange.y);
    }
    
    private int GetExponentialQuantity()
    {
        float maxQuantityChance = 1.0f - _maxQuantityChance;
        float randomValue = Random.value;
        float adjustedValue = Mathf.Pow(randomValue, maxQuantityChance);

        int rangeDelta = _quantityRange.y - _quantityRange.x;
        int quantity = (int)(_quantityRange.x + adjustedValue * rangeDelta);

        return quantity;
    }
}