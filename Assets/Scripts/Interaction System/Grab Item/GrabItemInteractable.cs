using System;
using System.Collections.Generic;
using Audio_System;
using UI.Inventory;
using UI.Inventory.Actions;
using UI.Inventory.Inventory_Item;
using UnityEngine;
using UnityEngine.Localization;
using Random = UnityEngine.Random;

namespace Interaction_System.Grab_Item
{
    public class GrabItemInteractable : GrabItemInteractable<GrabInteractableConstraints, GrabInteractableSounds> {}
    
    public abstract class GrabItemInteractable<T1, T2> : MonoBehaviour, IInteractable
        where T1 : GrabInteractableConstraints
        where T2 : GrabInteractableSounds
    {
        [field: Header("Settings")]
        [field: SerializeField] public float holdDuration { get; protected set; }
        [field: SerializeField] public SpriteAlignment spriteAlignment { get; private set; }
        [SerializeField] private bool _tempInventoryEnabled = true;
        [SerializeField] private CalculationType _calculationType = CalculationType.Exponential;
        [SerializeField] private OnGrabType _onGrabType = OnGrabType.Deactivate;
    
        [Header("Quantity Settings")]
        [SerializeField, Range(0, 1)] private float _maxQuantityChance = 0.2f;
        [SerializeField] private Vector2Int _quantityRange = new(1, 1);
    
        [Header("Localized Settings")]
        [SerializeField] private LocalizedString _unsuccessfulMessage;
        [SerializeField] private LocalizedString _notEnoughSpaceMessage;
        [SerializeField] private LocalizedString _labelText;

        [Header("Other")]
        [SerializeField] protected T1 _constraints;
        [SerializeField] protected T2 _sounds;
    
        #region Enums
        enum CalculationType
        {
            Probabilistic,
            Exponential,
            Static
        }

        enum OnGrabType
        {
            Destroy,
            Deactivate,
            Nothing
        }
        #endregion

        public InventoryItem insertedInventoryItem { get; private set; }
        public int quantity => _quantity;
    
        public event Action OnInteract;
        public event Action OnSet;
        public event Action OnQuantityUpdate; 

        private InventoryController _inventoryController;
        private int _quantity;
        private bool _isTempItemAdded;

        public virtual string GetInteractableName() => _labelText.GetLocalizedString();
    
        protected virtual void Awake() {}

        private void Start()
        {
            _quantity = GetQuantity();
            _inventoryController = Player.Instance.inventoryController;
        
            OnSet?.Invoke();
        }

        public virtual InteractionResponse Interact()
        {
            if (_isTempItemAdded)
                return HandleTemporaryItemInteraction();

            if (_quantity <= 0)
                return new(_unsuccessfulMessage.GetLocalizedString(), false, true);

            insertedInventoryItem = HandleInventoryItemInteraction();

            HandleInteractionResult(insertedInventoryItem);
            OnInteract?.Invoke();

            if (_quantity > 0)
                return new(_notEnoughSpaceMessage.GetLocalizedString(), true, true);

            return new(null, true);
        }

        private InteractionResponse HandleTemporaryItemInteraction()
        {
            Player.Instance.HUDController.ToggleHUDView(true);
            Player.Instance.HUDController.OnHUDStateChanged += OnHUDStateChangedPerformed;

            return new(null, true);
        }
    
        private void OnHUDStateChangedPerformed(bool state)
        {
            if (state)
                return;
        
            Forget();
        }

        private InventoryItem HandleInventoryItemInteraction()
        {
            var result = _inventoryController.AddItemToInventory(_constraints.inventoryItemSO, ref _quantity);
        
            if (_tempInventoryEnabled && _quantity > 0)
            {
                int dummyQuantity = _quantity;
                _isTempItemAdded = _inventoryController.AddItemToInventory(_constraints.inventoryItemSO, ref dummyQuantity, true);
            
                result = null;
            }

            return result;
        }

        private void HandleInteractionResult(bool result)
        {
            if (result)
            {
                HandleSuccessfulInteraction();
                PlayPickUpSound();
            }
        }
    
        public void Forget()
        {
            if (!_isTempItemAdded)
                return;

            if (_inventoryController.ItemExistsInTempInventory(_constraints.inventoryItemSO))
            {
                int added = _quantity - _inventoryController.GetItemCountInInventory(_constraints.inventoryItemSO, true);
                _quantity -= added;

                if (added > 0)
                {
                    PlayPickUpSound();
                    OnQuantityUpdate?.Invoke();
                }

                _inventoryController.RemoveInventoryItem(_constraints.inventoryItemSO, _quantity, true);
            }
            else
            {
                _quantity = 0;
                OnQuantityUpdate?.Invoke();
                HandleInteractionResult(true);
            }
        
            _isTempItemAdded = false;
            _isTempItemAdded = false;
            Player.Instance.HUDController.OnHUDStateChanged -= OnHUDStateChangedPerformed;
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
                case CalculationType.Static:
                    return _quantityRange.x;
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

        private void PlayPickUpSound()
        {
            SoundManager.Instance.CreateSound()
                .WithSoundData(_sounds.pickUpSoundSO.soundData)
                .WithRandomPitch()
                .WithPosition(transform.position)
                .Play();
        }
    
        public virtual InteractableVisualSO GetInteractableVisualSO()
        {
            return _constraints.interactableVisualSO;
        }

        public IReadOnlyList<InventoryItemAction> GetActions()
        {
            return _constraints.inventoryItemSO.actions;
        }
    }
}