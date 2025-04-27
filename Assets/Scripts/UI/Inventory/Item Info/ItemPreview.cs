using System.Collections.Generic;
using UI.Inventory.Inventory_Item;
using UI.Inventory.Item_Preview;
using UnityEngine;

namespace UI.Inventory.Item_Info
{
    public class ItemPreview : MonoBehaviour
    {
        [SerializeField] private ItemPreviewData _defaultPreviewData;
        [SerializeField] private Vector3 _rotationSpeed = new (0f, 30f, 0f);
        [SerializeField] private LayerMask _uiLayer;
        
        private GameObject _currentPreviewObj;
        private Dictionary<ItemPreviewData, GameObject> _previewDataCache;

        private void Start()
        {
            _previewDataCache = new();
            
            _currentPreviewObj = CreateNewPreviewObj(_defaultPreviewData);
        }
        
        private void Update()
        {
            if (_currentPreviewObj)
                HandleRotation();
        }

        private void HandleRotation()
        {
            transform.Rotate(_rotationSpeed * Time.deltaTime);
        }

        public void RefreshInfoPanel(InventoryItem inventoryItem = null)
        {
            _currentPreviewObj?.SetActive(false);

            if (inventoryItem)
            {
                if (!_previewDataCache.TryGetValue(inventoryItem.inventoryItemSO.previewData, out GameObject previewObj))
                    previewObj = CreateNewPreviewObj(inventoryItem.inventoryItemSO.previewData);

                _currentPreviewObj = previewObj;
            }
            else
            {
                _currentPreviewObj = _previewDataCache[_defaultPreviewData];
            }
            
            _currentPreviewObj.SetActive(true);
        }

        private GameObject CreateNewPreviewObj(ItemPreviewData previewData)
        {
            GameObject previewObj = Instantiate(previewData.prefab, transform);
            
            previewObj.transform.localScale = previewData.scale;
            previewObj.transform.localRotation = Quaternion.Euler(previewData.rotation);
            
            SetLayerRecursively(previewObj, Mathf.RoundToInt(Mathf.Log(_uiLayer.value, 2)));
            
            _previewDataCache.Add(previewData, previewObj);
            
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