using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory.Inventory_Item
{
    public class WeaponInventoryItem : InventoryItem
    {
        private Weapon _weapon;

        private void OnDestroy()
        {
            if (_weapon)
                _weapon.OnAmmoChanged -= WeaponOnAmmoChanged;
        }

        public override void Set(InventoryItemSO inventoryItemSO, ItemGrid itemGrid, int quantity = 0)
        {
            this.inventoryItemSO = inventoryItemSO;
            
            GetComponent<Image>().sprite = inventoryItemSO.icon;
        
            GetRectTransform().sizeDelta = new(
                GetActualSize().x * itemGrid.tileSize.x,
                GetActualSize().y * itemGrid.tileSize.y
            );

            SetQuantity(1);
            UpdateQuantityText(quantity);
        }
        
        public override GameObject GetItem()
        {
            if (item)
                return item;
            
            item = Instantiate(inventoryItemSO.prefab, Vector3.zero, Quaternion.identity);
            if (item.TryGetComponent(out Weapon weapon))
            {
                _weapon = weapon;
                weapon.OnAmmoChanged += WeaponOnAmmoChanged;
            }
            else
                Debug.LogWarning($"{item.name} is not type of {typeof(Weapon)}!");
                    
            return item;
        }

        private void WeaponOnAmmoChanged(int ammo)
        {
            UpdateQuantityText(ammo);
        }
    }
}
