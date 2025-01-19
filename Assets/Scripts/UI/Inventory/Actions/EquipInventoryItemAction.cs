using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Inventory.Actions
{
    [CreateAssetMenu(menuName = "Inventory/Actions/Equip")]
    public class EquipInventoryItemAction : InventoryItemAction
    {
        protected override void OnActionPerformed(InputAction.CallbackContext obj)
        {
            Debug.Log($"{GetActionName()} is clicked!");
        }
    }
}