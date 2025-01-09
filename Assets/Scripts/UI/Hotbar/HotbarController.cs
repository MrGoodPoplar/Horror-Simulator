using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Hotbar
{
    public class HotbarController : MonoBehaviour
    {
        [SerializeField] private List<HotbarSlot> _hotbarSlots;

        private readonly List<Action<InputAction.CallbackContext>> _delegates = new();
        
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

        private void SubscribeToHotkeys()
        {
            foreach (HotbarSlot slot in _hotbarSlots)
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

        private void OnHotkeyPerformed(HotbarSlot hotbarSlot, InputAction.CallbackContext context)
        {
            Debug.Log($"Hotbar Slot triggered: {hotbarSlot}, {context.action.GetBindingDisplayString()} - pressed!");
        }
    }
}