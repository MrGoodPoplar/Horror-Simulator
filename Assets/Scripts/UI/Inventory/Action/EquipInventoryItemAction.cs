using System.ComponentModel;
using UI.Hotbar;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Inventory.Actions
{
    [CreateAssetMenu(menuName = "UI/Inventory/Actions/Equip")]
    public class EquipInventoryItemAction : InventoryItemAction
    {
        [field: SerializeField] public HotbarSlotSO hotbarSlotSO { get; private set; }
        
        protected override void OnActionPerformed(InputAction.CallbackContext obj)
        {
            var currentItem = Player.Instance.hotkeyPromptController?.currentOnHoverItem;

            if (currentItem)
                Player.Instance.hotbarController.EquipItem(currentItem, hotbarSlotSO.guid);
        }
    }
}