using System.Collections.Generic;
using JetBrains.Annotations;
using UI.Inventory;
using UI.Inventory.Inventory_Item;
using UI.Inventory.Item_Preview;
using UnityEngine;

namespace UI
{
    public class ItemPreview : MonoBehaviour
    {
        [SerializeField] private Vector3 _rotationSpeed = new (0f, 30f, 0f);
        [SerializeField] private LayerMask _uiLayer;
        
        private InventoryItem _currentItem;
        private InventoryController _inventoryController;
        private GameObject _currentPreviewObj;
        private Dictionary<InventoryItemSO, GameObject> _previewDataCache;

        private void Start()
        {
            _inventoryController = Player.Instance.inventoryController;
            _previewDataCache = new();
        }
        
        private void Update()
        {
            HandleRotation();
            HandleRefreshment();
        }

        private void HandleRotation()
        {
            transform.Rotate(_rotationSpeed * Time.deltaTime);
        }

        private void HandleRefreshment()
        {
            if (GetCurrentItem() != _currentItem)
            {
                _currentItem = GetCurrentItem();
                RefreshInfoPanel();
            }
        }

        [CanBeNull]
        private InventoryItem GetCurrentItem()
        {
            return _inventoryController.selectedItem ?? _inventoryController.onHoverInventoryItem;
        }

        private void RefreshInfoPanel()
        {
            _currentPreviewObj?.SetActive(false);
            _currentPreviewObj = null;
            
            if (_currentItem)
            {
                if (!_previewDataCache.TryGetValue(_currentItem.inventoryItemSO, out GameObject previewObj))
                    previewObj = CreateNewPreviewObj(_currentItem.inventoryItemSO);

                _currentPreviewObj = previewObj;
                _currentPreviewObj.SetActive(true);
            }
        }

        private GameObject CreateNewPreviewObj(InventoryItemSO inventoryItemSO)
        {
            ItemPreviewData previewData = inventoryItemSO.previewData;
            GameObject previewObj = Instantiate(previewData.prefab, transform);
            
            previewObj.transform.localScale = previewData.scale;
            previewObj.transform.localRotation = previewData.rotation;
            
            SetLayerRecursively(previewObj, Mathf.RoundToInt(Mathf.Log(_uiLayer.value, 2)));
            
            _previewDataCache.Add(inventoryItemSO, previewObj);
            
            return previewObj;
        }
        
        public void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (!obj)
                return;

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}