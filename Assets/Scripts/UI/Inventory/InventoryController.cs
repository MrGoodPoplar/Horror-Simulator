using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public ItemGrid itemGrid { get; set; }

    [SerializeField] private PlayerInput _playerInput;

    private InventoryItem _selectedItem;
    [SerializeField] private List<InventoryItemSO> _inventoryItemSOs;
    [SerializeField] private InventoryItem _inventoryItemPrefab;
    
    private void Start()
    {
        _playerInput.OnClick += OnClickPerformed;
    }

    private void OnDestroy()
    {
        _playerInput.OnClick -= OnClickPerformed;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q) && itemGrid)
        {
            InventoryItem inventoryItem = Instantiate(_inventoryItemPrefab);
            inventoryItem.Set(_inventoryItemSOs[Random.Range(0, _inventoryItemSOs.Count)], itemGrid);
            itemGrid.PlaceItem(inventoryItem, new (Random.Range(0, 8), Random.Range(0, 8)));
        }
        
        if (!_selectedItem)
            return;

        _selectedItem.transform.position = Input.mousePosition;
    }

    private void OnClickPerformed()
    {
        if (!itemGrid)
            return;

        Vector2Int positionOnGrid = itemGrid.GetTileGridPosition(Input.mousePosition);

        if (!_selectedItem)
            PickUpItem(positionOnGrid);
        else
            PlaceItem(positionOnGrid);
    }

    private void PlaceItem(Vector2Int positionOnGrid)
    {
        if (itemGrid.IsItemInsideBoundary(positionOnGrid, _selectedItem.inventoryItemSO.size))
            _selectedItem.SetPivotToDefault();
        
        if (itemGrid.PlaceItem(_selectedItem, positionOnGrid))
        {
            _selectedItem = null;
        }
    }

    private void PickUpItem(Vector2Int positionOnGrid)
    {
        _selectedItem = itemGrid.PickUpItem(positionOnGrid);
        _selectedItem?.SetPivotForTile(itemGrid.tileSize);
    }
}
