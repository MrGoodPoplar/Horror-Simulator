using System.Collections.Generic;
using UnityEngine;

namespace UI.Inventory
{
    public class GrabOnlyItemGrid : ItemGrid
    {
        [SerializeField] private List<InventoryItem> _relativeItems = new();

        public override bool PlaceItem(InventoryItem inventoryItem, Vector2Int position, ref InventoryItem overlappedItem)
        {
            if (!IsRelativeItem(inventoryItem))
                return false;
            
            return base.PlaceItem(inventoryItem, position, ref overlappedItem);
        }

        public override bool CanPlaceItem(InventoryItem inventoryItem, Vector2Int position)
        {
            if (!IsRelativeItem(inventoryItem))
                return false;
            
            return base.CanPlaceItem(inventoryItem, position);
        }

        public bool IsRelativeItem(InventoryItem inventoryItem)
        {
            return _relativeItems.Contains(inventoryItem);
        }

        public void AddRelativeItem(InventoryItem inventoryItem)
        {
            _relativeItems.Add(inventoryItem);
        }

        public void RemoveRelativeItem(InventoryItem inventoryItem)
        {
            _relativeItems.Remove(inventoryItem);
        }

        public void ClearRelatives()
        {
            _relativeItems.Clear();
            Clear();
        }
    }
}