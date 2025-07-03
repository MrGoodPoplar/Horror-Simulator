using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UI.Inventory.Inventory_Item;
using UI.Inventory.Item_Info;
using Unity.VisualScripting;
using UnityEngine;

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
        [SerializeField] private Transform _inventoryContainer;
        [SerializeField] private Alert _inventoryItemWarning;

        public event Action<bool> OnStateChanged;
        
        public InventoryItem selectedItem => _selectedItem;
        public InventoryItem onHoverInventoryItem { get; private set; }
        public bool state { get; private set; } = true; // Debug true

        private RectTransform _canvasRect;
        private Camera _uiCamera;
        private ItemGrid _currentItemGrid;
        private InventoryItem _selectedItem;
        private InventoryItem _overlappedItem;

        private Vector2Int _positionOnGrid;
        private Vector2Int _tileSize;
        private bool _selectedItemPosUpdated;

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
            _inventoryItemWarning = Instantiate(_inventoryItemWarning);
            
            ToggleInventory(state);
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

            // Debug
            if (Input.GetKeyDown(KeyCode.Y))
                ToggleInventory(!state);
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
        
        [CanBeNull]
        private InventoryItem InsertItemToInventory(InventoryItemSO inventoryItemSO, int quantity, bool isTempInventory = false)
        {
            if (!state)
                return null;
            
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
                _selectedItemPosUpdated = true;
                onHoverInventoryItem = null;
                return;
            }
        
            Vector2Int newPositionOnGrid = GetTileGridPosition();

            if (_positionOnGrid == newPositionOnGrid && !_selectedItemPosUpdated)
                return;

            _positionOnGrid = newPositionOnGrid;
            onHoverInventoryItem = null;
            _selectedItemPosUpdated = false;
            
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
                SetSelectedItem(_overlappedItem, true);
                _overlappedItem = null;
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
            SetSelectedItem(_currentItemGrid.PickUpItem(positionOnGrid), true);
        }

        private void SetSelectedItem(InventoryItem inventoryItem, bool posUpdated = false)
        {
            _inventoryItemWarning.Hide();

            _selectedItem = inventoryItem;
            _selectedItemPosUpdated = posUpdated;
            
            if (_selectedItem)
            {
                _selectedItem.GetRectTransform().SetAsLastSibling();
                _selectedItem.SetPivotCenter();
            }
        }
        
        private void OnRotatePerformed()
        {
            if (!_selectedItem || _selectedItem.inventoryItemSO.isSymmetrical)
                return;

            _selectedItemPosUpdated = true;
            _selectedItem.Rotate(_tileSize);
            
            _inventoryItemWarning.Rotate(Quaternion.Euler(0, 0, _selectedItem.rotated ?  -90.0f : 0));
        }

        public bool ItemExistsInTempInventory(InventoryItemSO inventoryItemSO)
        {
            return !_tempInventoryItemGrid.FindItem(inventoryItemSO.guid).IsUnityNull();
        }

        public int GetItemCountInInventory(InventoryItemSO inventoryItemSO, bool isTempInventory = false)
        {
            if (!state)
                return 0;
            
            ItemGrid itemGrid = isTempInventory ? _tempInventoryItemGrid : _inventoryItemGrid;
            return itemGrid.CountItem(inventoryItemSO.guid);
        }
        
        private bool OnHUDStateChangedPerformed(bool state)
        {
            if (state || _selectedItem.IsUnityNull())
                return true;

            _inventoryItemWarning.Perform(_selectedItem.transform, Quaternion.Euler(0, 0, _selectedItem.rotated ?  -90.0f : 0));
            
            return false;
        }

        public bool IsOnHoverGridMain()
        {
            return _currentItemGrid == _inventoryItemGrid;
        }

        public void ToggleInventory(bool toggle)
        {
            if (Player.Instance.HUDController.isHUDView)
                return;
            
            OnStateChanged?.Invoke(toggle);
            
            state = toggle;
            _inventoryContainer.gameObject.SetActive(toggle);
        }
    }
}
