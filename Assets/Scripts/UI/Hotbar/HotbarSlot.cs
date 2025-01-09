using UI.Inventory;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Hotbar
{
    [System.Serializable]
    public class HotbarSlot
    {
        public InventoryItem item { get; set; }
        [field: SerializeField] public InputActionReference inputActionReference { get; set; }
        
        public HotbarSlot(InventoryItem item, InputActionReference inputActionReference)
        {
            this.item = item;
            this.inputActionReference = inputActionReference;
        }
    }
}