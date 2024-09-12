using System.Collections.Generic;
using UnityEngine;

namespace UI.Inventory
{
    public class InventoryController : MonoBehaviour
    {
        [Header("Constraints")]
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private List<InventoryItemSO> _inventoryItemSOs;
        [SerializeField] private InventoryItem _inventoryItemPrefab;
        [SerializeField] private InventoryItemHighlight _itemHighlight;
        [SerializeField] private ItemGrid _inventoryItemGrid;
        [SerializeField] private ItemGrid _tempInventoryItemGrid;
        [SerializeField] private Transform _itemDragParent;
    
        private ItemGrid _itemGrid;
        private InventoryItem _selectedItem;
        private InventoryItem _overlappedItem;

        private Vector2Int _positionOnGrid;
        private Vector2Int _tileSize;
        private bool _itemStateUpdated;

        private void Start()
        {
            _playerInput.OnClick += OnClickPerformed;
            _playerInput.OnRotate += OnRotatePerformed;

            _inventoryItemGrid.OnItemInteract += OnItemInteractPerformed;
            _tempInventoryItemGrid.OnItemInteract += OnItemInteractPerformed;
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
            if (Input.GetKeyDown(KeyCode.Q) && _itemGrid)
            {
                InventoryItem inventoryItem = Instantiate(_inventoryItemPrefab);
                InventoryItemSO randomInventoryItemSO = _inventoryItemSOs[Random.Range(0, _inventoryItemSOs.Count)];
                inventoryItem.Set(randomInventoryItemSO, _itemGrid, Random.Range(1, randomInventoryItemSO.maxQuantity));

                Vector2Int? freeSlot = _itemGrid.FindFreeSlotForItem(inventoryItem.inventoryItemSO.size);

                if (freeSlot.HasValue)
                {
                    _itemGrid.PlaceItem(inventoryItem, freeSlot.Value, ref _overlappedItem);
                    inventoryItem.GetRectTransform().SetAsLastSibling();
                }
                else
                {
                    Debug.Log("No free slot available for the item.");
                    Destroy(inventoryItem.gameObject);
                }
            }

            if (Input.GetKeyDown(KeyCode.T) && _itemGrid)
            {
                InventoryItemSO randomInventoryItemSO = _inventoryItemSOs[Random.Range(0, _inventoryItemSOs.Count)];
                int quantity = Random.Range(1, 50);
                Debug.Log($"Stack: {randomInventoryItemSO.name}, Qty: {quantity}");

                if (TryStackItem(randomInventoryItemSO.guid, _itemGrid, ref quantity))
                {
                    Debug.Log($"Yep added! Left: [{quantity}]");
                }
                else
                {
                    bool resylt = AddItemToInventory(randomInventoryItemSO, ref quantity, true);
                    Debug.Log($"Leftover... [{quantity}] BUT! : {resylt}");
                }
            }
        
            HandleItemHighlight();
            HandleItemDrag();
        }

        public bool TryStackItem(string guid, ItemGrid itemGrid, ref int quantity)
        {
            while (quantity > 0)
            {
                InventoryItem inventoryItem = itemGrid?.FindItem(guid, true);

                if (!inventoryItem)
                    return false;
                
                quantity = inventoryItem.AddQuantity(quantity);
            }

            return true;
        }
        
        public bool AddItemToInventory(InventoryItemSO inventoryItemSO, ref int quantity, bool toTempInventory = false)
        {
            if (inventoryItemSO.isStackable && TryStackItem(inventoryItemSO.guid, _inventoryItemGrid, ref quantity))
                return true;
            
            while (quantity > 0)
            {
                int quantityToAdd = Mathf.Clamp(quantity, 1, inventoryItemSO.maxQuantity);
                bool isInserted = InsertItemToInventory(inventoryItemSO, quantityToAdd, toTempInventory);

                if (!isInserted)
                {
                    return false;
                }

                quantity -= quantityToAdd;
            }

            quantity = 0;
            return true;
        }

        private bool InsertItemToInventory(InventoryItemSO inventoryItemSO, int quantity, bool toTempInventory = false)
        {
            ItemGrid itemGrid = toTempInventory ? _tempInventoryItemGrid : _inventoryItemGrid;
            InventoryItem inventoryItem = Instantiate(_inventoryItemPrefab);
            inventoryItem.Set(inventoryItemSO, itemGrid, Mathf.Clamp(quantity, 1, inventoryItemSO.maxQuantity));

            Vector2Int? freeSlot = itemGrid.FindFreeSlotForItem(inventoryItem.inventoryItemSO.size);

            if (freeSlot.HasValue)
            {
                itemGrid.PlaceItem(inventoryItem, freeSlot.Value, ref _overlappedItem);
                inventoryItem.GetRectTransform().SetAsLastSibling();
                return true;
            }
        
            Destroy(inventoryItem.gameObject);
            return false;
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
        
        private void OnItemInteractPerformed(object sender, ItemGrid.InventoryItemEventArgs e)
        {
            if (sender is not ItemGrid)
                return;
            
            ItemGrid itemGrid = sender as ItemGrid;
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
        
            _selectedItem.transform.position = Input.mousePosition;
        }

        private void HandleItemHighlight()
        {
            if (!_itemGrid)
            {
                _itemHighlight.Hide();
                _itemStateUpdated = true;
                return;
            }
        
            Vector2Int newPositionOnGrid = GetTileGridPosition();

            if (_positionOnGrid == newPositionOnGrid && !_itemStateUpdated)
                return;

            _positionOnGrid = newPositionOnGrid;
            _itemStateUpdated = false;
        
            if (!_selectedItem)
            {
                InventoryItem itemToHighlight = _itemGrid.GetItem(_positionOnGrid);

                if (itemToHighlight)
                {
                    _itemHighlight.SetColor(_itemHighlight.defaultColor);
                
                    ToggleItemHighlight(
                        true,
                        itemToHighlight.GetActualSize(),
                        itemToHighlight.gridPosition
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
        }
    
        private void OnRotatePerformed()
        {
            if (!_selectedItem || _selectedItem.inventoryItemSO.isSymmetrical)
                return;

            _itemStateUpdated = true;
            _selectedItem.Rotate(_tileSize);
        }
    }
}
