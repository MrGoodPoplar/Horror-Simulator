using System;
using System.Collections.Generic;
using UI.Inventory.Actions;
using UI.Inventory.Item_Preview;
using UnityEngine;
using UnityEngine.Localization;

namespace UI.Inventory.Inventory_Item
{
    [CreateAssetMenu(menuName = "UI/Inventory/Items/Default")]
    public class InventoryItemSO : ScriptableObject, IGuided
    {
        [Header("Localization")]
        [SerializeField] private LocalizedString _itemName;
        [SerializeField] private LocalizedString _itemDescription;

        [field: Header("Parameters")]
        [field: SerializeField] public ItemType type { get; private set; }
        [field: SerializeField] public Vector2Int size { get; private set; }
        [field: SerializeField] public Sprite icon { get; private set; }
        [field: SerializeField] public int maxQuantity { get; private set; } = 1;
        [field: SerializeField] public GameObject prefab { get; private set; }
        [field: SerializeField] public ItemPreviewData previewData { get; private set; }
        [field: SerializeField] private List<InventoryItemAction> _actions;
        [field: SerializeField, HideInInspector] public string guid { get; private set; }

        public enum ItemType
        {
            Default,
            Weapon
        }
    
        public bool isStackable => maxQuantity > 1;
        public bool isSymmetrical => size.x == size.y;
        public string itemName => _itemName.GetLocalizedString();
        public string itemDescription => _itemDescription.GetLocalizedString();
        public IReadOnlyList<InventoryItemAction> actions => _actions;
    
        public void GenerateGuid()
        {
            guid = Guid.NewGuid().ToString();
        }
    }
}