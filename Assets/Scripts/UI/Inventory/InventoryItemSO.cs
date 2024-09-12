using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace UI.Inventory
{
    [CreateAssetMenu(menuName = "Inventory/Item")]
    public class InventoryItemSO : ScriptableObject, IGuided
    {
        [field: SerializeField] public string itemName { get; private set; }
        [field: SerializeField, TextArea] public string itemDescription { get; private set; }
        [field: SerializeField] public Vector2Int size { get; private set; }
        [field: SerializeField] public Sprite icon { get; private set; }
        [field: SerializeField] public int maxQuantity { get; private set; } = 1;
        [field: SerializeField, HideInInspector] public string guid { get; set; }

        public bool isStackable => maxQuantity > 1;
        public bool isSymmetrical => size.x == size.y;
    }
}
