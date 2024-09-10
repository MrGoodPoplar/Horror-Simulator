using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [Header("Constraints")]
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private List<InventoryItemSO> _inventoryItemSOs;
    [SerializeField] private InventoryItem _inventoryItemPrefab;
    [SerializeField] private InventoryItemHighlight _itemHighlight;
    [SerializeField] private ItemGrid _inventoryItemGrid;
    
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
    }

    private void OnDestroy()
    {
        _playerInput.OnClick -= OnClickPerformed;
        _playerInput.OnRotate -= OnRotatePerformed;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && _itemGrid)
        {
            // Instantiate a new inventory item
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
        
        HandleItemHighlight();
        HandleItemDrag();
    }

    public bool AddItemToInventory(InventoryItemSO inventoryItemSO, int quantity)
    {
        while (quantity > 0)
        {
            int quantityToAdd = Mathf.Clamp(quantity, 1, inventoryItemSO.maxQuantity);
            bool isInserted = InsertItemToInventory(inventoryItemSO, quantityToAdd);

            if (!isInserted)
                return false;

            quantity -= quantityToAdd;
        }

        return true;
    }

    public bool InsertItemToInventory(InventoryItemSO inventoryItemSO, int quantity)
    {
        InventoryItem inventoryItem = Instantiate(_inventoryItemPrefab);
        inventoryItem.Set(inventoryItemSO, _inventoryItemGrid, Mathf.Clamp(quantity, 1, inventoryItemSO.maxQuantity));

        Vector2Int? freeSlot = _inventoryItemGrid.FindFreeSlotForItem(inventoryItem.inventoryItemSO.size);

        if (freeSlot.HasValue)
        {
            _inventoryItemGrid.PlaceItem(inventoryItem, freeSlot.Value, ref _overlappedItem);
            inventoryItem.GetRectTransform().SetAsLastSibling();
            return true;
        }
        
        Destroy(inventoryItem.gameObject);
        return false;
    }

    public void SetItemGrid(ItemGrid newItemGrid)
    {
        _itemGrid = newItemGrid;

        if (_itemGrid)
        {
            _tileSize = _itemGrid.tileSize;
        }
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
            _itemHighlight.SetColor(_itemGrid.CanPlaceItem(_selectedItem.GetActualSize(), _positionOnGrid)
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
            PlaceItem(positionOnGrid);
    }

    private Vector2Int GetTileGridPosition()
    {
        Vector2 pointerPosition = Input.mousePosition; // TODO: gamepad support
        
        if (_selectedItem)
        {
            pointerPosition.x -= (_selectedItem.GetActualSize().x - 1) * (float)_itemGrid.tileSize.x / 2;
            pointerPosition.x -= (_selectedItem.GetActualSize().y - 1) * (float)_itemGrid.tileSize.y / 2;
        }
        
        return _itemGrid.GetTileGridPosition(pointerPosition);
    }

    private void PlaceItem(Vector2Int positionOnGrid)
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
