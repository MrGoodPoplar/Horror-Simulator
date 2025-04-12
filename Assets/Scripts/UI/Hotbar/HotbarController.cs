using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Interaction_System.Grab_Item;
using UI.Inventory.Actions;
using UI.Inventory.Inventory_Item;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Hotbar
{
    public class HotbarController : MonoBehaviour
    {
        [SerializeField] private List<HotbarSlotSO> _hotbarSlots;
        
        public bool canInteract { get; set; } = true;
        
        private readonly List<Action<InputAction.CallbackContext>> _delegates = new();
        private readonly Dictionary<GameObject, HoldableItem> _holdableCache = new();
        private HoldingItemController _holdingItemController;

        private void Awake()
        {
            InitHotbarSlots();
        }

        private void Start()
        {
            _holdingItemController = Player.Instance.holdingItemController;
            
            Player.Instance.interactController.OnInteract += InteractControllerOnInteractPerformed;
        }

        private void OnDestroy()
        {
            UnsubscribeFromHotkeys();
        }

        private void OnEnable()
        {
            SubscribeToHotkeys();
        }

        private void OnDisable()
        {
            UnsubscribeFromHotkeys();
        }

        private void InitHotbarSlots()
        {
            for (var index = 0; index < _hotbarSlots.Count; index++)
            {
                _hotbarSlots[index] = Instantiate(_hotbarSlots[index]);
            }
        }

        private void SubscribeToHotkeys()
        {
            foreach (HotbarSlotSO slot in _hotbarSlots)
            {
                slot.inputActionReference.action.Enable();
                Action<InputAction.CallbackContext> action = ctx => OnHotkeyPerformed(slot, ctx);
                slot.inputActionReference.action.performed += action;
                _delegates.Add(action);
            }
        }

        private void UnsubscribeFromHotkeys()
        {
            for (int i = 0; i < _hotbarSlots.Count; i++)
            {
                var slot = _hotbarSlots[i];
                if (slot.inputActionReference?.action != null && _delegates.Count > i)
                {
                    slot.inputActionReference.action.performed -= _delegates[i];
                }
            }

            _delegates.Clear();
        }

        private async void OnHotkeyPerformed(HotbarSlotSO hotbarSlot, InputAction.CallbackContext context)
        {
            if (!canInteract || !hotbarSlot.item)
                return;
            
            if (TryGetHoldable(hotbarSlot.item, out HoldableItem holdable))
            {
                holdable.hotbarSlotSO = hotbarSlot;

                if (_holdingItemController.currentHoldable?.hotbarSlotSO == hotbarSlot)
                    await _holdingItemController.HideAsync();
                else
                    await _holdingItemController.TakeAsync(holdable);
            }
            else
                Debug.LogError($"{hotbarSlot.item.inventoryItemSO.name}'s prefab is not type of {typeof(HoldableItem)}!");
        }

        // TODO: remove from cache if item not in inventory!
        private bool TryGetHoldable(InventoryItem inventoryItem, out HoldableItem holdable)
        {
            var item = inventoryItem.GetItem();
            if (_holdableCache.TryGetValue(item, out holdable))
                return true;
            
            if (item.TryGetComponent(out holdable))
            {
                _holdableCache.Add(item, holdable);
                return true;
            }

            return false;
        }

        public bool EquipItem(InventoryItem inventoryItem, string hotbarSlotGuid, bool onlyEmpty = false)
        {
            if (TryGetHotbarSlot(hotbarSlotGuid, out var hotbarSlot))
            {
                if (!onlyEmpty || !hotbarSlot.item)
                    hotbarSlot.item = inventoryItem;

                // If this item already equipped
                return inventoryItem == hotbarSlot.item;
            }
            else
                Debug.LogWarning($"Hotbar Slot with guid {hotbarSlotGuid} doesn't exist!");

            return false;
        }

        private bool TryGetHotbarSlot(string hotbarSlotGuid, out HotbarSlotSO hotbarSlot)
        {
            return hotbarSlot = _hotbarSlots.Find(slot => slot.guid == hotbarSlotGuid);
        }
        
        private void InteractControllerOnInteractPerformed(IInteractable interactable, InteractionResponse response)
        {
            if (interactable is GrabItemInteractable<GrabInteractableConstraints, GrabInteractableSounds> grabItem && grabItem.insertedInventoryItem != null)
            {
                if (grabItem.GetActions().FirstOrDefault(action => action is EquipInventoryItemAction) is EquipInventoryItemAction equipAction)
                {
                    bool equipped = EquipItem(grabItem.insertedInventoryItem, equipAction.hotbarSlotSO.guid, true);

                    if (!_holdingItemController.currentHoldable && TryGetHoldable(grabItem.insertedInventoryItem, out HoldableItem holdable) && equipped)
                    {
                        holdable.hotbarSlotSO = equipAction.hotbarSlotSO;
                        _holdingItemController.TakeAsync(holdable).Forget();
                    }
                }
            }
        }
    }
}