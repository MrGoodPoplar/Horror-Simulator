using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory.Inventory_Item
{
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public class InventoryItem : MonoBehaviour
    {
        [Header("Quantity Settings")]
        [SerializeField] private int _maxQuantityValueText = 99;
    
        [Header("Constraints")]
        [SerializeField] private TextMeshProUGUI _quantityText;
        [SerializeField] private Image _quantityBackground;

        public Vector2Int gridPosition { get; set; }
        public InventoryItemSO inventoryItemSO { get; protected set; }
        public bool rotated { get; private set; }
        public int quantity { get; private set; }

        protected GameObject item;
        
        private RectTransform _rectTransform;
        private Vector2 _defaultPivot;
        private Vector3 _defaultScale;

        private Vector3 _defaultQuantityAnchoredPosition;

        public RectTransform GetRectTransform()
        {
            if (_rectTransform)
                return _rectTransform;
        
            _rectTransform = GetComponent<RectTransform>();
            _defaultPivot = _rectTransform.pivot;
            _defaultScale = _rectTransform.localScale;
            _defaultQuantityAnchoredPosition = _quantityBackground.rectTransform.anchoredPosition;
        
            return _rectTransform;
        }

        public void SetParent(Transform parent, Vector3 scale = default)
        {
            GetRectTransform().SetParent(parent, false);
            GetRectTransform().localScale = scale != Vector3.zero ? scale : _defaultScale;
        }
    
        public void SetPivotForTile(Vector2Int tileSize)
        {
            SetPivot(new (
                1.0f / (2.0f * GetActualSize().x),
                1.0f - 1.0f / (2.0f * GetActualSize().y)
            ));
        }

        public void SetPivotCenter()
        {
            SetPivot(new (0.5f, 0.5f));
        }

        public void SetPivotToDefault()
        {
            SetPivot(new (
                rotated ? 1.0f : _defaultPivot.x,
                _defaultPivot.y
            ));
        }

        public virtual void Set(InventoryItemSO inventoryItemSO, ItemGrid itemGrid, int quantity = 1)
        {
            this.inventoryItemSO = inventoryItemSO;
            
            _quantityBackground.gameObject.SetActive(inventoryItemSO.isStackable);

            GetComponent<Image>().sprite = inventoryItemSO.icon;
        
            GetRectTransform().sizeDelta = new(
                GetActualSize().x * itemGrid.tileSize.x,
                GetActualSize().y * itemGrid.tileSize.y
            );

            SetQuantity(quantity);
        }

        public int AddQuantity(int value)
        {
            return SetQuantity(quantity + value);
        }
    
        public int SetQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                Destroy(gameObject);
        
            int maxCapacity = inventoryItemSO.maxQuantity;

            if (newQuantity <= maxCapacity)
            {
                quantity = newQuantity;
                UpdateQuantityText(quantity);
            
                return 0;
            }

            quantity = maxCapacity;
            UpdateQuantityText(quantity);

            int leftover = newQuantity - quantity;
            return leftover > 0 ? leftover : 0;
        }

        protected void UpdateQuantityText(int value)
        {
            value = value > _maxQuantityValueText ? _maxQuantityValueText : value;
            _quantityText.text = value.ToString();
        }
    
        private void SetPivot(Vector2 newPivot)
        {
            RectTransform rectTransform = GetRectTransform();
            Vector2 oldPivotOffset = rectTransform.pivot * rectTransform.rect.size;
        
            rectTransform.pivot = newPivot;
        
            Vector2 newPivotOffset = rectTransform.pivot * rectTransform.rect.size;

            rectTransform.position += (Vector3)(oldPivotOffset - newPivotOffset);
        }

        public void Rotate(Vector2Int tileSize)
        {
            rotated = !rotated;
            _rectTransform.rotation = Quaternion.Euler(0, 0, rotated ?  90.0f : 0);
            _quantityBackground.rectTransform.localRotation = Quaternion.Euler(0, 0, rotated ? -90.0f : 0);
            _quantityBackground.rectTransform.anchoredPosition = rotated
                ? Vector3.left * (tileSize.x * inventoryItemSO.size.x)
                : _defaultQuantityAnchoredPosition;
        }

        public Vector2Int GetActualSize()
        {
            return new(
                rotated ? inventoryItemSO.size.y : inventoryItemSO.size.x,
                rotated ? inventoryItemSO.size.x : inventoryItemSO.size.y
            );
        }

        public bool IsFullQuantity()
        {
            return quantity == inventoryItemSO.maxQuantity;
        }

        public virtual GameObject GetItem()
        {
            return item ? item : item = Instantiate(inventoryItemSO.prefab, Vector3.zero, Quaternion.identity);
        }
        
        public void RefreshVisual()
        {
            UpdateQuantityText(quantity);
        }
    }
}
