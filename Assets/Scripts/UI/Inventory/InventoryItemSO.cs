using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Item")]
public class InventoryItemSO : ScriptableObject
{
    [field: SerializeField] public Vector2Int size { get; private set; }
    [field: SerializeField] public Sprite icon { get; private set; }
}
