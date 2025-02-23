using System.Collections.Generic;
using UnityEngine;

namespace UI.Inventory.Inventory_Item
{
    [CreateAssetMenu(menuName = "UI/Inventory/Inventory Item Binder")]
    public class InventoryItemBinder : ScriptableObject
    {
        [System.Serializable]
        public struct ItemPrefabEntry
        {
            public InventoryItemSO.ItemType ItemType;
            public InventoryItem Prefab;
        }

        public List<ItemPrefabEntry> ItemPrefabs;

        private Dictionary<InventoryItemSO.ItemType, InventoryItem> _prefabDictionary;

        private void OnEnable()
        {
            _prefabDictionary = new Dictionary<InventoryItemSO.ItemType, InventoryItem>();
            foreach (var entry in ItemPrefabs)
            {
                _prefabDictionary[entry.ItemType] = entry.Prefab;
            }
        }

        public InventoryItem GetPrefab(InventoryItemSO.ItemType itemType) => _prefabDictionary.GetValueOrDefault(itemType);
    }
}