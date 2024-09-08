using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public ItemGrid itemGrid { get; set; }

    [Header("Constraints")]
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private List<InventoryItemSO> _inventoryItemSOs;
    [SerializeField] private InventoryItem _inventoryItemPrefab;
    [SerializeField] private InventoryItemHighlight _itemHighlight;

    private InventoryItem _selectedItem;
    private InventoryItem _overlappedItem;

    private Vector2Int _positionOnGrid;
    private bool _itemRotationUpdated;

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
        if (Input.GetKeyDown(KeyCode.Q) && itemGrid)
        {
            InventoryItem inventoryItem = Instantiate(_inventoryItemPrefab);
            inventoryItem.Set(_inventoryItemSOs[Random.Range(0, _inventoryItemSOs.Count)], itemGrid);
            itemGrid.PlaceItem(inventoryItem, new (Random.Range(0, 8), Random.Range(0, 8)), ref _overlappedItem);
            inventoryItem.GetRectTransform().SetAsLastSibling();
        }
        
        HandleItemHighlight();
        HandleItemDrag();
    }

    private void HandleItemDrag()
    {
        if (!_selectedItem)
            return;
        
        _selectedItem.transform.position = Input.mousePosition;
    }

    private void HandleItemHighlight()
    {
        if (!itemGrid)
        {
            _itemHighlight.Hide();
            return;
        }
        
        Vector2Int newPositionOnGrid = GetTileGridPosition();

        if (_positionOnGrid == newPositionOnGrid && !_itemRotationUpdated)
            return;

        _positionOnGrid = newPositionOnGrid;
        _itemRotationUpdated = false;
        
        if (!_selectedItem)
        {
            InventoryItem itemToHighlight = itemGrid.GetItem(_positionOnGrid);

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
            _itemHighlight.SetColor(itemGrid.CanPlaceItem(_selectedItem, _positionOnGrid)
                ? _itemHighlight.allowedColor
                : _itemHighlight.forbiddenColor);
            
            ToggleItemHighlight(
                itemGrid.IsItemInsideBoundary(_positionOnGrid, _selectedItem.GetActualSize()),
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
            _itemHighlight.SetParent(itemGrid, true);
            _itemHighlight.SetSize(highlightSize, itemGrid.tileSize);
            _itemHighlight.SetPosition(itemGrid, position);
        }
        else
        {
            _itemHighlight.Hide();
        }
    }
    
    private void OnClickPerformed()
    {
        if (!itemGrid)
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
            pointerPosition.x -= (_selectedItem.GetActualSize().x - 1) * (float)itemGrid.tileSize.x / 2;
            pointerPosition.x -= (_selectedItem.GetActualSize().y - 1) * (float)itemGrid.tileSize.y / 2;
        }
        
        return itemGrid.GetTileGridPosition(pointerPosition);
    }

    private void PlaceItem(Vector2Int positionOnGrid)
    {
        if (itemGrid.CanPlaceItem(_selectedItem, positionOnGrid))
            _selectedItem.SetPivotToDefault();
        
        if (itemGrid.PlaceItem(_selectedItem, positionOnGrid, ref _overlappedItem))
        {
            _selectedItem = null;

            if (_overlappedItem)
            {
                _selectedItem = _overlappedItem;
                _overlappedItem = null;
                
                _selectedItem.GetRectTransform().SetAsLastSibling();
                _selectedItem.SetPivotCenter();
            }
        }
    }

    private void PickUpItem(Vector2Int positionOnGrid)
    {
        _selectedItem = itemGrid.PickUpItem(positionOnGrid);
        
        _selectedItem?.GetRectTransform().SetAsLastSibling();
        _selectedItem?.SetPivotCenter();
    }
    
    private void OnRotatePerformed()
    {
        if (!_selectedItem)
            return;

        _itemRotationUpdated = true;
        _selectedItem.Rotate();
    }
}
