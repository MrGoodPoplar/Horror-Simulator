using System;
using System.Collections.Generic;
using UI.Inventory.Inventory_Item;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
    [RequireComponent(typeof(RectTransform))]
    public class ItemGrid : MonoBehaviour
    {
        [field: Header("Tile Settings")]
        [field: SerializeField] public Vector2Int tileSize { get; private set; } = new (16, 16);
        [SerializeField] private Vector2Int _size = new (8, 8);
        [SerializeField] private Vector2 _scale = new(1, 1);
    
        [Header("Background Settings")]
        [SerializeField] private RectTransform _backgroundPrefab;
        [SerializeField] private Vector2 _backgroundScale = new(1.25f, 1.25f);
        [SerializeField] private Color _backgroundColor;
    
        [Header("Constraints")]
        [SerializeField] private PlayerInput _playerInput;

        public event EventHandler<InventoryItemEventArgs> OnItemInteract;
    
        #region InventoryItemEventArgs Class
        public class InventoryItemEventArgs : EventArgs
        {
            public InventoryItem inventoryItem { get; private set; }
            public bool grabbed { get; private set; }
        
            public InventoryItemEventArgs(InventoryItem inventoryItem, bool grabbed = false)
            {
                this.inventoryItem = inventoryItem;
                this.grabbed = grabbed;
            }
        }
        #endregion

        public Vector3 scale => _scale;
        public RectTransform rectTransform => _rectTransform;
    
        private RectTransform _rectTransform;
        private Vector2 _onGridPosition;
        private Vector2Int _tileGridPosition;

        private InventoryItem[,] _inventoryItemSlot;
    
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _inventoryItemSlot = new InventoryItem[_size.x, _size.y];

            _rectTransform.localScale = new(_scale.x, _scale.y);
            _rectTransform.sizeDelta = new(tileSize.x * _size.x, tileSize.y * _size.y);
        }

        private void Start()
        {
            if (_backgroundPrefab)
                SetBackground(_backgroundPrefab);
        }

        private void OnDestroy()
        {
            OnItemInteract = null;
        }

        private void SetBackground(RectTransform backgroundPrefab)
        {
            RectTransform background = Instantiate(backgroundPrefab, transform.parent);
            background.SetAsFirstSibling();
            background.localScale = new (_backgroundScale.x, _backgroundScale.y);
            background.sizeDelta = new(tileSize.x * _size.x, tileSize.y * _size.y);
            
            if (background.TryGetComponent(out Image image))
            {
                image.color = _backgroundColor;
            }
        
            Vector2 gridSize = new Vector2(_size.x * tileSize.x * _scale.x, _size.y * tileSize.y * _scale.y);
            Vector2 backgroundSize = new Vector2(_size.x * tileSize.x * _backgroundScale.x, _size.y * tileSize.y * _backgroundScale.y);
            Vector2 sizeDifference = backgroundSize - gridSize;
        
            background.localPosition = new(-sizeDifference.x / 2, sizeDifference.y / 2);
        }
    
        public Vector2Int GetTileGridPosition(Vector2 localScreenPosition)
        {
            float localX = localScreenPosition.x;
            float localY = -localScreenPosition.y;
            
            int gridX = (int)(localX / tileSize.x);
            int gridY = (int)(localY / tileSize.y);

            return new Vector2Int(gridX, gridY);
        }
        
        public virtual bool PlaceItem(InventoryItem inventoryItem, Vector2Int position, ref InventoryItem overlappedItem)
        {
            if (!IsItemInsideBoundary(position, inventoryItem.GetActualSize()))
                return false;

            if (IsOverlapping(position, inventoryItem.GetActualSize(), ref overlappedItem))
            {
                overlappedItem = null;
                return false;
            }

            if (overlappedItem)
            {
                if (!overlappedItem.IsFullQuantity() && !inventoryItem.IsFullQuantity() && inventoryItem.inventoryItemSO.guid == overlappedItem.inventoryItemSO.guid)
                    return false;
            
                ForgetItem(overlappedItem);
                OnItemInteract?.Invoke(this, new (overlappedItem, true));
            }

            OnItemInteract?.Invoke(this, new (inventoryItem));
            SetInventoryItemSlot(inventoryItem, position);

            inventoryItem.gridPosition = position;
            inventoryItem.transform.localPosition = GetPositionOnGrid(position);
            
            return true;
        }
        
        public void ForgetItem(InventoryItem inventoryItem)
        {
            if (Contains(inventoryItem))
                SetInventoryItemSlot(inventoryItem, inventoryItem.gridPosition, false);
        }

        public Vector3 GetPositionOnGrid(Vector2Int position)
        {
            return new(
                position.x * tileSize.x,
                -(position.y * tileSize.y)
            );
        }

        public virtual bool CanPlaceItem(InventoryItem inventoryItem, Vector2Int position)
        {
            if (!IsItemInsideBoundary(position, inventoryItem.GetActualSize()))
                return false;
        
            InventoryItem dummy = null;
        
            if (IsOverlapping(position, inventoryItem.GetActualSize(), ref dummy))
                return false;

            return true;
        }

        private void SetInventoryItemSlot(InventoryItem inventoryItem, Vector2Int position, bool set = true)
        {
            for (int x = 0; x < inventoryItem.GetActualSize().x; x++)
            {
                for (int y = 0; y < inventoryItem.GetActualSize().y; y++)
                {
                    _inventoryItemSlot[position.x + x, position.y + y] = set ? inventoryItem : null;
                }
            }
        }
    
        public InventoryItem PickUpItem(Vector2Int positionOnGrid)
        {
            InventoryItem inventoryItem = GetItem(positionOnGrid);
        
            if (inventoryItem)
            {
                SetInventoryItemSlot(inventoryItem, inventoryItem.gridPosition, false);
                OnItemInteract?.Invoke(this, new (inventoryItem, true));
            }

            return inventoryItem;
        }

        private bool BoundaryCheck(Vector2Int position)
        {
            if (position.x < 0 || position.y < 0)
                return false;

            if (position.x >= _size.x || position.y >= _size.y)
                return false;

            return true;
        }

        public bool IsItemInsideBoundary(Vector2Int position, Vector2Int size)
        {
            if (!BoundaryCheck(position))
                return false;

            if (!BoundaryCheck(position + new Vector2Int(size.x - 1, size.y - 1)))
                return false;
        
            return true;
        }
    
        private bool IsOverlapping(Vector2Int position, Vector2Int size, ref InventoryItem overlappedItem)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    InventoryItem inventoryItem = GetItem(new(position.x + x, position.y + y));
                
                    if (inventoryItem)
                    {
                        if (!overlappedItem)
                            overlappedItem = inventoryItem;
                        else if (overlappedItem != inventoryItem)
                            return true;
                    }
                }
            }
        
            return false;
        }
    
        private bool IsOverlapping(Vector2Int position, Vector2Int size)
        {
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    InventoryItem tileInventoryItem = GetItem(new Vector2Int(position.x + x, position.y + y));
            
                    if (tileInventoryItem != null)
                        return true;
                }
            }
    
            return false;
        }
        
        private bool IsOverlapping(Vector2Int position, InventoryItem inventoryItem)
        {
            for (int x = 0; x < inventoryItem.GetActualSize().x; x++)
            {
                for (int y = 0; y < inventoryItem.GetActualSize().y; y++)
                {
                    InventoryItem tileInventoryItem = GetItem(new Vector2Int(position.x + x, position.y + y));

                    if (tileInventoryItem != null && tileInventoryItem != inventoryItem)
                        return true;
                }
            }
    
            return false;
        }

        public InventoryItem GetItem(Vector2Int position)
        {
            if (position.x < 0 || position.x >= _inventoryItemSlot.GetLength(0)|| position.y < 0 || position.y >= _inventoryItemSlot.GetLength(1))
                return null;

            return _inventoryItemSlot[position.x, position.y];
        }

        public Vector2Int? FindFreeSlotForItem(Vector2Int itemSize)
        {
            for (int y = 0; y <= _size.y - itemSize.y; y++)
            {
                for (int x = 0; x <= _size.x - itemSize.x; x++)
                {
                    Vector2Int slotPosition = new Vector2Int(x, y);
            
                    if (!IsOverlapping(slotPosition, itemSize))
                    {
                        return slotPosition;
                    }
                }
            }
            
            return null;
        }

        public InventoryItem FindItem(string guid, bool notFull = false)
        {
            for (int y = 0; y < _size.y; y++)
            {
                for (int x = 0; x < _size.x; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    InventoryItem currentItem = GetItem(position);

                    if (currentItem && currentItem.inventoryItemSO.guid == guid && (!notFull || currentItem.quantity < currentItem.inventoryItemSO.maxQuantity))
                    { 
                        return currentItem;
                    }
                }
            }

            return null;
        }

        public bool Contains(InventoryItem inventoryItem)
        {
            for (int y = 0; y < _size.y; y++)
            {
                for (int x = 0; x < _size.x; x++)
                {
                    InventoryItem currentItem = GetItem(new (x, y));

                    if (currentItem == inventoryItem)
                        return true;
                }
            }

            return true;
        }
        
        public bool RemoveInventoryItem(string guid, int quantityToRemove = 1)
        {
            while (quantityToRemove > 0)
            {
                InventoryItem inventoryItem = FindItem(guid);

                if (inventoryItem)
                {
                    if (inventoryItem.quantity - quantityToRemove <= 0)
                        SetInventoryItemSlot(inventoryItem, inventoryItem.gridPosition, false);

                    int itemQuantity = inventoryItem.quantity;
                    inventoryItem.SetQuantity(itemQuantity - Mathf.Min(quantityToRemove, itemQuantity));

                    quantityToRemove -= Mathf.Min(quantityToRemove, itemQuantity);
                }
                else
                    break;
            }

            return quantityToRemove == 0;
        }

        public void Clear()
        {
            for (int y = 0; y < _size.y; y++)
            {
                for (int x = 0; x < _size.x; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    InventoryItem currentItem = GetItem(position);

                    if (currentItem)
                        Destroy(currentItem);
                }
            }
        }

        public int CountItem(string guid)
        {
            List<InventoryItem> countedItems = new();
            int total = 0;
            
            for (int y = 0; y < _size.y; y++)
            {
                for (int x = 0; x < _size.x; x++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    InventoryItem currentItem = GetItem(position);

                    if (currentItem && currentItem.inventoryItemSO.guid == guid && !countedItems.Contains(currentItem))
                    {
                        countedItems.Add(currentItem);
                        total += currentItem.quantity;
                    }
                }
            }

            return total;
        }

        public bool TryReplaceItem(InventoryItem inventoryItem, Vector2Int newPosition, bool rotate = false)
        {
            if (Contains(inventoryItem) && !IsOverlapping(newPosition, inventoryItem))
            {
                ForgetItem(inventoryItem);

                if (rotate)
                {
                    inventoryItem.Rotate(tileSize);
                    inventoryItem.SetPivotToDefault();
                }
                
                SetInventoryItemSlot(inventoryItem, newPosition);

                inventoryItem.gridPosition = newPosition;
                inventoryItem.transform.localPosition = GetPositionOnGrid(newPosition);
                
                return true;
            }

            return false;
        }
    }
}
