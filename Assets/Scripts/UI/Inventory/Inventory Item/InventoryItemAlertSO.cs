using UnityEngine;

namespace UI.Inventory.Inventory_Item
{
    [CreateAssetMenu(menuName = "UI/Inventory/Item Alert")]
    public class InventoryItemAlertSO : ScriptableObject
    {
        [field: SerializeField] public Sprite sprite { get; private set; }
        [field: SerializeField] public float fadeSpeed { get; private set; }
        [field: SerializeField] public float duration { get; private set; }
    }
}