using System.Collections.Generic;
using UI.Inventory.Inventory_Item;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace UI.Inventory
{
    public class InventoryController : MonoBehaviour
    {
        [Header("Constraints")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private List<InventoryItemSO> _inventoryItemSOs;
        [SerializeField] private InventoryItemBinder _inventoryItemBinder;
        [SerializeField] private InventoryItemHighlight _itemHighlight;
        [SerializeField] private ItemGrid _inventoryItemGrid;
        [SerializeField] private GrabOnlyItemGrid _tempInventoryItemGrid;
        [SerializeField] private Transform _itemDragParent;

        public InventoryItem selectedItem => _selectedItem;
        public InventoryItem onHoverInventoryItem { get; private set; }

        private RectTransform _canvasRect;
        private Camera _uiCamera;
        private ItemGrid _currentItemGrid;
        private InventoryItem _selectedItem;
        private InventoryItem _overlappedItem;
        private InventoryItem _addedItemFromTempInventory;
        private InventoryItem _lastMovedItem;
        
        private Vector2Int _positionOnGrid;
        private Vector2Int _tileSize;
        private Vector2Int _lastMovedItemGridPos;
        private bool _itemStateUpdated;
        private bool _lastMovedItemRotated;

        private void Start()
        {
            _playerInput.OnClick += OnClickPerformed;
            _playerInput.OnRotate += OnRotatePerformed;

            _inventoryItemGrid.OnItemInteract += OnItemInteractPerformed;
            _tempInventoryItemGrid.OnItemInteract += OnItemInteractPerformed;
            Player.Instance.HUDController.OnHUDStateChanged += OnHUDStateChangedPerformed;
            InventoryItemFactory.Initialize(_inventoryItemBinder);

            _uiCamera = _canvas.worldCamera;
            _canvasRect = _canvas.GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            _playerInput.OnClick -= OnClickPerformed;
            _playerInput.OnRotate -= OnRotatePerformed;
            
            _inventoryItemGrid.OnItemInteract -= OnItemInteractPerformed;
            _tempInventoryItemGrid.OnItemInteract -= OnItemInteractPerformed;
        }

        private void Update()
        {
            HandleItemHighlight();
            HandleItemDrag();
        }

        public InventoryItem TryStackItem(string guid, ItemGrid itemGrid, ref int quantity)
        {
            InventoryItem inventoryItem = null;

            while (quantity > 0)
            {
                inventoryItem = itemGrid?.FindItem(guid, true);

                if (!inventoryItem)
                    return null;

                quantity = inventoryItem.AddQuantity(quantity);
            }

            return inventoryItem;
        }

        
        public InventoryItem AddItemToInventory(InventoryItemSO inventoryItemSO, ref int quantity, bool isTempInventory = false)
        {
            var inventoryItem = TryStackItem(inventoryItemSO.guid, _inventoryItemGrid, ref quantity);
            
            if (inventoryItemSO.isStackable && inventoryItem)
                return inventoryItem;
            
            while (quantity > 0)
            {
                int quantityToAdd = Mathf.Clamp(quantity, 1, inventoryItemSO.maxQuantity); 
                inventoryItem = InsertItemToInventory(inventoryItemSO, quantityToAdd, isTempInventory);
                
                if (!inventoryItem)
                    return null;

                quantity -= quantityToAdd;
            }

            quantity = 0;

            return inventoryItem;
        }

        public bool RemoveInventoryItem(InventoryItemSO inventoryItemSO, int quantity = 1, bool isTempInventory = false)
        {
            var itemGrid = isTempInventory ? _tempInventoryItemGrid : _inventoryItemGrid;
            return itemGrid.RemoveInventoryItem(inventoryItemSO.guid, quantity);
        }
        
        private InventoryItem InsertItemToInventory(InventoryItemSO inventoryItemSO, int quantity, bool isTempInventory = false)
        {
            var itemGrid = isTempInventory ? _tempInventoryItemGrid : _inventoryItemGrid;
            Vector2Int? freeSlot = itemGrid.FindFreeSlotForItem(inventoryItemSO.size);
            bool rotated = false;
            
            if (!freeSlot.HasValue && !inventoryItemSO.isSymmetrical)
            {
                freeSlot = itemGrid.FindFreeSlotForItem(new (inventoryItemSO.size.y, inventoryItemSO.size.x));
                rotated = true;
            }
            
            if (freeSlot.HasValue)
            {
                var inventoryItem = InventoryItemFactory.CreateInventoryItem(inventoryItemSO, itemGrid, quantity);
                
                if (rotated)
                    inventoryItem.Rotate(itemGrid.tileSize);
                
                PutItemInInventory(inventoryItem, freeSlot.Value, isTempInventory);
                return inventoryItem;
            }
        
            return null;
        }

        private void PutItemInInventory(InventoryItem inventoryItem, Vector2Int positionOnGrid, bool isTempInventory = false)
        {
            ItemGrid itemGrid = isTempInventory ? _tempInventoryItemGrid : _inventoryItemGrid;

            if (isTempInventory)
                _tempInventoryItemGrid.AddRelativeItem(inventoryItem);

            itemGrid.PlaceItem(inventoryItem, positionOnGrid, ref _overlappedItem);
            inventoryItem.GetRectTransform().SetAsLastSibling();
        }

        public void SetItemGrid(ItemGrid newItemGrid)
        {
            if (_currentItemGrid)
                _currentItemGrid.OnItemInteract -= OnItemInteractPerformed;
            
            _currentItemGrid = newItemGrid;

            if (_currentItemGrid)
            {
                _tileSize = _currentItemGrid.tileSize;
                _currentItemGrid.OnItemInteract += OnItemInteractPerformed;
            }
        }
        
        // TODO: may be called twice, when item from temp inventory set to default one
        private void OnItemInteractPerformed(object sender, ItemGrid.InventoryItemEventArgs e)
        {
            if (sender is not ItemGrid itemGrid)
                return;

            InventoryItem inventoryItem = e.inventoryItem;
            
            if (e.grabbed)
            {
                inventoryItem.SetParent(_itemDragParent, itemGrid.scale);
                return;
            }

            if (itemGrid == _inventoryItemGrid && _tempInventoryItemGrid.IsRelativeItem(inventoryItem))
                _addedItemFromTempInventory = inventoryItem;
            
            inventoryItem.SetPivotToDefault();
            inventoryItem.SetParent(itemGrid.rectTransform);
        }

        private void HandleItemDrag()
        {
            if (!_selectedItem)
                return;
        
            Vector2 pointerPosition = Input.mousePosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect,
                pointerPosition,
                _uiCamera,
                out Vector2 localPointerPosition
            );
            
            _selectedItem.transform.localPosition  = localPointerPosition;
        }

        private void HandleItemHighlight()
        {
            if (!_currentItemGrid)
            {
                _itemHighlight.Hide();
                _itemStateUpdated = true;
                onHoverInventoryItem = null;
                return;
            }
        
            Vector2Int newPositionOnGrid = GetTileGridPosition();

            if (_positionOnGrid == newPositionOnGrid && !_itemStateUpdated)
                return;

            _positionOnGrid = newPositionOnGrid;
            onHoverInventoryItem = null;
            _itemStateUpdated = false;
            
            if (!_selectedItem)
            {
                onHoverInventoryItem = _currentItemGrid.GetItem(_positionOnGrid);

                if (onHoverInventoryItem)
                {
                    _itemHighlight.SetColor(_itemHighlight.defaultColor);
                
                    ToggleItemHighlight(
                        true,
                        onHoverInventoryItem.GetActualSize(),
                        onHoverInventoryItem.gridPosition
                    );
                }
                else
                {
                    ToggleItemHighlight(false);
                }
            }
            else
            {
                _itemHighlight.SetColor(_currentItemGrid.CanPlaceItem(_selectedItem, _positionOnGrid)
                    ? _itemHighlight.allowedColor
                    : _itemHighlight.forbiddenColor);
            
                ToggleItemHighlight(
                    _currentItemGrid.IsItemInsideBoundary(_positionOnGrid, _selectedItem.GetActualSize()),
                    _selectedItem.GetActualSize(),
                    _positionOnGrid
                );
            }
        }

        private void ToggleItemHighlight(bool toggle, Vector2Int highlightSize = default, Vector2Int position = default)
        {
            if (toggle)
            {
                _itemHighlight.Show();
                _itemHighlight.SetParent(_currentItemGrid, true);
                _itemHighlight.SetSize(highlightSize, _currentItemGrid.tileSize);
                _itemHighlight.SetPosition(_currentItemGrid, position);
            }
            else
            {
                _itemHighlight.Hide();
            }
        }
    
        private void OnClickPerformed()
        {
            if (!_currentItemGrid)
                return;
        
            Vector2Int positionOnGrid = GetTileGridPosition();

            if (!_selectedItem)
                PickUpItem(positionOnGrid);
            else
                PlaceSelectedItem(positionOnGrid);
        }

        private Vector2Int GetTileGridPosition()
        {
            Vector2 pointerPosition = Input.mousePosition;

            if (_selectedItem)
            {
                pointerPosition.x -= _selectedItem.GetActualSize().x * _currentItemGrid.tileSize.x / 2f;
                pointerPosition.y += _selectedItem.GetActualSize().y * _currentItemGrid.tileSize.y / 2f;
            }
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _currentItemGrid.rectTransform,
                pointerPosition,
                _uiCamera,
                out Vector2 localPoint
            );
            
            return _currentItemGrid.GetTileGridPosition(localPoint);
        }

        private void PlaceSelectedItem(Vector2Int positionOnGrid)
        {
            if (_currentItemGrid.PlaceItem(_selectedItem, positionOnGrid, ref _overlappedItem))
            {
                _selectedItem = null;
                _itemStateUpdated = true;

                if (_overlappedItem)
                {
                    _selectedItem = _overlappedItem;
                    _overlappedItem = null;
                
                    _selectedItem.GetRectTransform().SetAsLastSibling();
                    _selectedItem.SetPivotCenter();
                    
                    if (_currentItemGrid == _tempInventoryItemGrid)
                        _addedItemFromTempInventory = _selectedItem;
                }
            }
            else if (_overlappedItem)
            {
                int leftover = _overlappedItem.AddQuantity(_selectedItem.quantity);
                _selectedItem.SetQuantity(leftover);
                
                _overlappedItem = null;
            }
        }

        private void PickUpItem(Vector2Int positionOnGrid)
        {
            _selectedItem = _currentItemGrid.PickUpItem(positionOnGrid);

            if (_selectedItem)
            {
                _itemStateUpdated = true;
                _selectedItem.GetRectTransform().SetAsLastSibling();
                _selectedItem.SetPivotCenter();
                
                _lastMovedItemGridPos = _selectedItem.gridPosition;
                _lastMovedItem = _selectedItem;
                _lastMovedItemRotated = _selectedItem.rotated;
            }

            if (_currentItemGrid == _tempInventoryItemGrid)
                _addedItemFromTempInventory = _selectedItem;
        }
        
        private void OnRotatePerformed()
        {
            if (!_selectedItem || _selectedItem.inventoryItemSO.isSymmetrical)
                return;

            _itemStateUpdated = true;
            _selectedItem.Rotate(_tileSize);
        }

        public bool ItemExistsInTempInventory(InventoryItemSO inventoryItemSO)
        {
            return !_tempInventoryItemGrid.FindItem(inventoryItemSO.guid).IsUnityNull();
        }

        public int GetItemCountInInventory(InventoryItemSO inventoryItemSO, bool isTempInventory = false)
        {
            ItemGrid itemGrid = isTempInventory ? _tempInventoryItemGrid : _inventoryItemGrid;
            return itemGrid.CountItem(inventoryItemSO.guid);
        }
        
        private void OnHUDStateChangedPerformed(bool state)
        {
            if (!state)
                HandleSelectedItemReturn();
        }

        private void HandleSelectedItemReturn()
        {
            if (_addedItemFromTempInventory && !_selectedItem)
                _tempInventoryItemGrid.RemoveRelativeItem(_addedItemFromTempInventory);

            if (!_selectedItem)
                return;

            bool putSucceed = false;
            
            if (_selectedItem != _addedItemFromTempInventory)
                putSucceed = TryPutSelectedItemToInventory();

            if (!putSucceed)
            {
                if (_addedItemFromTempInventory)
                    HandleSelectedItemReturnAsTemp();
                else
                    HandleSelectedItemReturnAsMain();
            }
            
            _tempInventoryItemGrid.ClearRelatives();
        }

        private void HandleSelectedItemReturnAsMain()
        {
            if (_lastMovedItem)
            {
                bool rotate = _lastMovedItem.rotated != _lastMovedItemRotated;
                
                if (_inventoryItemGrid.TryReplaceItem(_lastMovedItem, _lastMovedItemGridPos, rotate))
                    TryPutSelectedItemToInventory();
            }
            else
            {
                Debug.LogError("Couldn't fit selected item back to main inventory!");
            }
        }
        
        private void HandleSelectedItemReturnAsTemp()
        {
            if (_selectedItem == _addedItemFromTempInventory)
            {
                PutItemInInventory(_addedItemFromTempInventory, _selectedItem.gridPosition, true);
            }
            else
            {
                Vector2Int? freeSlot = _tempInventoryItemGrid.FindFreeSlotForItem(_selectedItem.GetActualSize());
                
                if (freeSlot.HasValue)
                {
                    _inventoryItemGrid.ForgetItem(_addedItemFromTempInventory);
                    PutItemInInventory(_addedItemFromTempInventory, freeSlot.Value, true);
                    TryPutSelectedItemToInventory();
                }
                else
                {
                    Debug.LogWarning("Couldn't find free slot for item in the temporary inventory!");
                }
            }
        }
        
        private bool TryPutSelectedItemToInventory()
        {
            Vector2Int? freeSlot = _inventoryItemGrid.FindFreeSlotForItem(_selectedItem.GetActualSize());

            if (!freeSlot.HasValue && !_selectedItem.inventoryItemSO.isSymmetrical)
            {
                _selectedItem.Rotate(_tileSize);
                freeSlot = _inventoryItemGrid.FindFreeSlotForItem(_selectedItem.GetActualSize());
            }
            
            if (freeSlot.HasValue)
            {
                PutItemInInventory(_selectedItem, freeSlot.Value);
                _selectedItem = null;
            }

            return freeSlot.HasValue;
        }

        public bool IsOnHoverGridMain()
        {
            return _currentItemGrid == _inventoryItemGrid;
        }
    }
}
