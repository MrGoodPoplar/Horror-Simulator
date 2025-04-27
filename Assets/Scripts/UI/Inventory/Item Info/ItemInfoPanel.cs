using System;
using JetBrains.Annotations;
using TMPro;
using UI.Inventory.Inventory_Item;
using UI.Inventory.Item_Preview;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Inventory.Item_Info
{
    public class ItemInfoPanel : MonoBehaviour
    {
        [SerializeField] private ItemPreview _itemPreview;
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _descriptionLabel;
        
        private InventoryItem _currentItem;
        private InventoryController _inventoryController;

        private void Start()
        {
            _inventoryController = Player.Instance.inventoryController;
            
            RefreshText();
        }

        private void Update()
        {
            HandleRefreshment();
        }
        
        private void HandleRefreshment()
        {
            if (GetCurrentItem() != _currentItem)
            {
                _currentItem = GetCurrentItem();
                _itemPreview.RefreshInfoPanel(_currentItem);

                RefreshText();
            }
        }
        
        [CanBeNull]
        private InventoryItem GetCurrentItem()
        {
            return _inventoryController.selectedItem ?? _inventoryController.onHoverInventoryItem;
        }

        private void RefreshText()
        {
            _nameLabel.text = _currentItem?.inventoryItemSO.itemName ?? String.Empty;
            _descriptionLabel.text = _currentItem?.inventoryItemSO.itemDescription ?? String.Empty;
        }
    }
}