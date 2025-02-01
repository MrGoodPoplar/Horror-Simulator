using System;
using UI.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Hotbar
{
    [CreateAssetMenu(menuName = "UI/Inventory/Hotbar Slot")]
    public class HotbarSlotSO : ScriptableObject, IGuided
    {
        [field: SerializeField] public InputActionReference inputActionReference { get; set; }
        [field: SerializeField, HideInInspector] public string guid { get; private set; }

        public InventoryItem item { get; set; }

        public void GenerateGUID()
        {
            guid = Guid.NewGuid().ToString();
        }
    }
}