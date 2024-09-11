using UnityEngine;

namespace UI.Inventory
{
    [CreateAssetMenu(menuName = "Inventory/Item")]
    public class InventoryItemSO : ScriptableObject
    {
        [field: SerializeField] public string itemName { get; private set; }
        [field: SerializeField, TextArea] public string itemDescription { get; private set; }
        [field: SerializeField] public Vector2Int size { get; private set; }
        [field: SerializeField] public Sprite icon { get; private set; }
        [field: SerializeField] public int maxQuantity { get; private set; } = 1;
    
        public bool isCountable => maxQuantity > 1;
        public bool isSymmetrical => size.x == size.y;
    
        public bool IsSame(InventoryItemSO inventoryItemSo)
        {
            return inventoryItemSo == this;
        }
    }
}
