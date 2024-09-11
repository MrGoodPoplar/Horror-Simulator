using System;
using UI.Inventory;
using UnityEngine;
using Random = UnityEngine.Random;

public class GrabItemInteractable : MonoBehaviour, IInteractable
{
    [field: Header("Settings")]
    [SerializeField] private Vector2Int _quantityRange = new(1, 1);
    [SerializeField, Range(0, 1)] private float _maxQuantityChance = 0.2f;
    [SerializeField] private bool _destroyOnGrab;
    [SerializeField] private CalculationType _calculationType = CalculationType.Exponential;
    
    [field: Header("Constraints")]
    [field: SerializeField] public InteractableVisualSO InteractableVisualSO { get; private set; }
    [field: SerializeField] public InventoryItemSO inventoryItem { get; private set; }

    enum CalculationType
    {
        Probabilistic,
        Exponential
    }

    public int quantity => _quantity;
    
    public event Action OnInteract;
    public event Action OnQuantitySet;
    
    private int _quantity;

    private void Start()
    {
        _quantity = GetQuantity();
    }

    public bool Interact(InteractController interactController)
    {
        OnInteract?.Invoke();
        
        bool result = Player.instance.inventoryController.AddItemToInventory(inventoryItem, ref _quantity);

        if (_quantity > 0)
        {
            result = Player.instance.inventoryController.AddItemToInventory(inventoryItem, ref _quantity, true);
        }
        
        // if (result && _destroyOnGrab)
        //     Destroy(gameObject);
        // else if (result)
        //     gameObject.SetActive(false);
        
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