using System;
using System.Collections.Generic;
using UI.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Hotbar
{
    public class HotbarController : MonoBehaviour
    {
        [SerializeField] private List<HotbarSlotSO> _hotbarSlots;

        private readonly List<Action<InputAction.CallbackContext>> _delegates = new();

        private HoldingItemController _holdingItemController;

        private void Start()
        {
            _holdingItemController = Player.instance.holdingItemController;
            
            InitHotbarSlots();
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

        private void OnHotkeyPerformed(HotbarSlotSO hotbarSlot, InputAction.CallbackContext context)
        {
            if (hotbarSlot.item.inventoryItemSO)
            {
                // if (ReferenceEquals(_holdingItemController.currentHoldable, hotbarSlot.item.inventoryItemSO))
                //     _holdingItemController.Hide();
                // else
                //     _holdingItemController.Hold(hotbarSlot.item.inventoryItemSO);
                
                // TODO: IHoldable implementation
            }
        }

        public void EquipItem(InventoryItem inventoryItem, string hotbarSlotGuid)
        {
            var hotbarSlot = _hotbarSlots.Find(slot => slot.guid == hotbarSlotGuid);
            
            if (hotbarSlot)
                hotbarSlot.item = inventoryItem;
            else
                Debug.LogWarning($"Hotbar Slot with guid {hotbarSlotGuid} doesn't exist!");
        }
    }
}