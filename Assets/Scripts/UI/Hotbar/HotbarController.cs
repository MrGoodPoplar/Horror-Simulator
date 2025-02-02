using System;
using System.Collections.Generic;
using UI.Inventory;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Hotbar
{
    public class HotbarController : MonoBehaviour
    {
        [SerializeField] private List<HotbarSlotSO> _hotbarSlots;
        
        public bool canInteract { get; set; } = true;
        
        private readonly List<Action<InputAction.CallbackContext>> _delegates = new();
        private Dictionary<string, IHoldable> _holdableCache = new();
        private HoldingItemController _holdingItemController;

        private void Awake()
        {
            InitHotbarSlots();
        }

        private void Start()
        {
            _holdingItemController = Player.instance.holdingItemController;
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
            if (!canInteract || !hotbarSlot.item)
                return;
            
            if (TryGetHoldable(hotbarSlot.item.inventoryItemSO, out IHoldable holdable))
            {
                if (holdable == _holdingItemController.currentHoldable)
                    _holdingItemController.Hide();
                else
                    _holdingItemController.Take(holdable);
            }
            else
                Debug.LogError($"{hotbarSlot.item.inventoryItemSO.name}'s prefab is not type of {typeof(IHoldable)}!");
        }

        private bool TryGetHoldable(InventoryItemSO inventoryItemSO, out IHoldable holdable)
        {
            if (_holdableCache.TryGetValue(inventoryItemSO.guid, out holdable))
                return true;
            
            var item = Instantiate(inventoryItemSO.prefab, Vector3.zero, Quaternion.identity);
            if (item.TryGetComponent(out holdable))
            {
                _holdableCache.Add(inventoryItemSO.guid, holdable);
                return true;
            }

            return false;
        }

        public void EquipItem(InventoryItem inventoryItem, string hotbarSlotGuid)
        {
            var hotbarSlot = _hotbarSlots.Find(slot => slot.guid == hotbarSlotGuid);
            
            if (hotbarSlot)
                hotbarSlot.item = inventoryItem;
            else
                Debug.LogWarning($"Hotbar Slot with guid {hotbarSlotGuid} doesn't exist!");
        }
        
        // TODO: handle hiding weapon during reload
        // TODO: camera bobbing bug if no item in hands
    }
}