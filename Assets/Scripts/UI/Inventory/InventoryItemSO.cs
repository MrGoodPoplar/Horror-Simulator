using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "Inventory/Item")]
public class InventoryItemSO : ScriptableObject, IGuided
{
    [SerializeField] private LocalizedString _itemName;
    [field: SerializeField, TextArea] public string itemDescription { get; private set; }
    [field: SerializeField] public Vector2Int size { get; private set; }
    [field: SerializeField] public Sprite icon { get; private set; }
    [field: SerializeField] public int maxQuantity { get; private set; } = 1;
    [field: SerializeField, HideInInspector] public string guid { get; set; }

    public bool isStackable => maxQuantity > 1;
    public bool isSymmetrical => size.x == size.y;
    public string itemName => _itemName.GetLocalizedString();
}