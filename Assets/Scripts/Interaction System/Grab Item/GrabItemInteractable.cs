using System;
using UI.Inventory;
using UnityEngine;
using UnityEngine.Localization;
using Random = UnityEngine.Random;

public class GrabItemInteractable : MonoBehaviour, IInteractable
{
    [field: Header("Settings")]
    [field: SerializeField] public float holdDuration { get; protected set; }
    [SerializeField] private bool _tempInventoryEnabled = true;
    [SerializeField] private CalculationType _calculationType = CalculationType.Exponential;
    [SerializeField] private OnGrabType _onGrabType = OnGrabType.Deactivate;
    
    [Header("Quantity Settings")]
    [SerializeField, Range(0, 1)] private float _maxQuantityChance = 0.2f;
    [SerializeField] private Vector2Int _quantityRange = new(1, 1);
    
    [Header("Message Settings")]
    [SerializeField] private LocalizedString _unsuccessfulMessage;
    [SerializeField] private LocalizedString _notEnoughSpaceMessage;

    [field: Header("Constraints")]
    [field: SerializeField] public InteractableVisualSO interactableVisualSO { get; protected set; }
    [field: SerializeField] public InventoryItemSO inventoryItem { get; private set; }
    
    #region Enums
    enum CalculationType
    {
        Probabilistic,
        Exponential
    }

    enum OnGrabType
    {
        Destroy,
        Deactivate,
        Nothing
    }
    #endregion

    public int quantity => _quantity;
    
    public event Action OnInteract;
    public event Action OnSet;
    
    private int _quantity;

    protected virtual void Awake() { }

    private void Start()
    {
        _quantity = GetQuantity();
        OnSet?.Invoke();
    }

    public virtual InteractionResponse Interact()
    {
        if (_quantity <= 0)
            return new(_unsuccessfulMessage.GetLocalizedString(), false, true);
            
        bool result = Player.instance.inventoryController.AddItemToInventory(inventoryItem, ref _quantity);

        if (_tempInventoryEnabled && _quantity > 0)
            result = Player.instance.inventoryController.AddItemToInventory(inventoryItem, ref _quantity, true);

        if (result)
            HandleSuccessfulInteraction();

        OnInteract?.Invoke();

        if (_quantity > 0)
            return new(_notEnoughSpaceMessage.GetLocalizedString(), true, true);
        
        return new(null, true);
    }

    private void HandleSuccessfulInteraction()
    {
        switch (_onGrabType)
        {
            case OnGrabType.Destroy:
                Destroy(gameObject);
                break;
            case OnGrabType.Deactivate:
                gameObject.SetActive(false);
                break;
        }
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
        int totalQuantity = _quantityRange.x;
        float chanceToRaiseQuantity = 1.0f - _maxQuantityChance;

        for (int i = _quantityRange.x; i < _quantityRange.y; i++)
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
        
        return (int)(_quantityRange.x + adjustedValue * rangeDelta);
    }
}