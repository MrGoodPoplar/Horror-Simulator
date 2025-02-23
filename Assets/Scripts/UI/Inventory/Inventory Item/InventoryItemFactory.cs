using UnityEngine;

namespace UI.Inventory.Inventory_Item
{
    public static class InventoryItemFactory
    {
        private static InventoryItemBinder _inventoryItemBinder;
        private static bool _initialized;
        
        public static void Initialize(InventoryItemBinder inventoryItemBinder)
        {
            _inventoryItemBinder = inventoryItemBinder;
            _initialized = true;
        }
        
        public static InventoryItem CreateInventoryItem(InventoryItemSO inventoryItemSO, ItemGrid itemGrid, int quantity)
        {
            if (!_initialized)
            {
                Debug.LogError($"{typeof(InventoryItemFactory)} is not initialized!");
                return null;
            }
            
            InventoryItem inventoryItem = Object.Instantiate(_inventoryItemBinder.GetPrefab(inventoryItemSO.type));

            int maxQuantity = inventoryItemSO.type == InventoryItemSO.ItemType.Weapon
                ? inventoryItemSO.prefab.GetComponent<Weapon>().bulletsInClip
                : inventoryItemSO.maxQuantity;

            inventoryItem.Set(inventoryItemSO, itemGrid, Mathf.Clamp(quantity, 1, maxQuantity));

            return inventoryItem;
        }
    }

}