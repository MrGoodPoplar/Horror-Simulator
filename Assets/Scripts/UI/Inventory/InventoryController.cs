using System.Collections.Generic;
using UI.Inventory.Inventory_Item;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace UI.Inventory
{
    public class InventoryController : MonoBehaviour
    {
        [Header("Constraints")]
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private List<InventoryItemSO> _inventoryItemSOs;
        [SerializeField] private InventoryItemBinder _inventoryItemBinder;
        [SerializeField] private InventoryItemHighlight _itemHighlight;
        [SerializeField] private ItemGrid _inventoryItemGrid;
        [SerializeField] private GrabOnlyItemGrid _tempInventoryItemGrid;
        [SerializeField] private Transform _itemDragParent;
    
        public InventoryItem onHoverInventoryItem { get; private set; }
        
        private ItemGrid _itemGrid;
        private InventoryItem _selectedItem;
        private InventoryItem _overlappedItem;
        private InventoryItem _addedItemFromTempInventory;
        
        private Vector2Int _positionOnGrid;
        private Vector2Int _tileSize;
        private bool _itemStateUpdated;

        private void Start()
        {
            _playerInput.OnClick += OnClickPerformed;
            _playerInput.OnRotate += OnRotatePerformed;

            _inventoryItemGrid.OnItemInteract += OnItemInteractPerformed;
            _tempInventoryItemGrid.OnItemInteract += OnItemInteractPerformed;
            Player.Instance.HUDController.OnHUDStateChanged += OnHUDStateChangedPerformed;
            InventoryItemFactory.Initialize(_inventoryItemBinder);
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
            if (_itemGrid)
                _itemGrid.OnItemInteract -= OnItemInteractPerformed;
            
            _itemGrid = newItemGrid;

            if (_itemGrid)
            {
                _tileSize = _itemGrid.tileSize;
                _itemGrid.OnItemInteract += OnItemInteractPerformed;
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
        
            _selectedItem.transform.position = Input.mousePosition;
        }

        private void HandleItemHighlight()
        {
            if (!_itemGrid)
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
                onHoverInventoryItem = _itemGrid.GetItem(_positionOnGrid);

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
                _itemHighlight.SetColor(_itemGrid.CanPlaceItem(_selectedItem, _positionOnGrid)
                    ? _itemHighlight.allowedColor
                    : _itemHighlight.forbiddenColor);
            
                ToggleItemHighlight(
                    _itemGrid.IsItemInsideBoundary(_positionOnGrid, _selectedItem.GetActualSize()),
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
                _itemHighlight.SetParent(_itemGrid, true);
                _itemHighlight.SetSize(highlightSize, _itemGrid.tileSize);
                _itemHighlight.SetPosition(_itemGrid, position);
            }
            else
            {
                _itemHighlight.Hide();
            }
        }
    
        private void OnClickPerformed()
        {
            if (!_itemGrid)
                return;
        
            Vector2Int positionOnGrid = GetTileGridPosition();

            if (!_selectedItem)
                PickUpItem(positionOnGrid);
            else
                PlaceSelectedItem(positionOnGrid);
        }

        private Vector2Int GetTileGridPosition()
        {
            // TODO: add gamepad accessibility
            Vector2 pointerPosition = Input.mousePosition;
        
            if (_selectedItem)
            {
                pointerPosition.x -= _selectedItem.GetActualSize().x * (float)_itemGrid.tileSize.x / 2;
                pointerPosition.y += _selectedItem.GetActualSize().y * (float)_itemGrid.tileSize.y / 2;
            }
            
            return _itemGrid.GetTileGridPosition(pointerPosition);
        }

        private void PlaceSelectedItem(Vector2Int positionOnGrid)
        {
            if (_itemGrid.PlaceItem(_selectedItem, positionOnGrid, ref _overlappedItem))
            {
                _selectedItem = null;
                _itemStateUpdated = true;

                if (_overlappedItem)
                {
                    _selectedItem = _overlappedItem;
                    _overlappedItem = null;
                
                    _selectedItem.GetRectTransform().SetAsLastSibling();
                    _selectedItem.SetPivotCenter();
                    
                    if (_itemGrid == _tempInventoryItemGrid)
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
            _selectedItem = _itemGrid.PickUpItem(positionOnGrid);

            if (_selectedItem)
            {
                _itemStateUpdated = true;
                _selectedItem.GetRectTransform().SetAsLastSibling();
                _selectedItem.SetPivotCenter();
            }

            if (_itemGrid == _tempInventoryItemGrid)
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
            if (_addedItemFromTempInventory && !_selectedItem)
                _tempInventoryItemGrid.RemoveRelativeItem(_addedItemFromTempInventory);

            if (state || !_selectedItem)
                return;

            if (_selectedItem == _addedItemFromTempInventory)
            {
                PutItemInInventory(_addedItemFromTempInventory, _selectedItem.gridPosition, true);
            }
            else if (!PutSelectedItemToInventory())
            {
                Vector2Int? freeSlot = _tempInventoryItemGrid.FindFreeSlotForItem(_selectedItem.GetActualSize());

                if (freeSlot.HasValue)
                {
                    _inventoryItemGrid.ForgetItem(_addedItemFromTempInventory);
                    PutItemInInventory(_addedItemFromTempInventory, freeSlot.Value, true);
                    PutSelectedItemToInventory();
                }
                else
                {
                    Debug.LogWarning("Couldn't find free slot for item in the temporary inventory!");
                }
            }
            
            _tempInventoryItemGrid.ClearRelatives();
        }
        
        private bool PutSelectedItemToInventory()
        {
            Vector2Int? freeSlot = _inventoryItemGrid.FindFreeSlotForItem(_selectedItem.GetActualSize());

            if (freeSlot.HasValue)
            {
                PutItemInInventory(_selectedItem, freeSlot.Value);
                _selectedItem = null;
            }

            return freeSlot.HasValue;
        }

        public bool IsOnHoverGridMain()
        {
            return _itemGrid == _inventoryItemGrid;
        }
    }
}
