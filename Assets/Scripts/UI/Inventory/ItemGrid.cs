using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ItemGrid : MonoBehaviour
{
    [field: Header("Tile Settings")]
    [field: SerializeField] public Vector2Int tileSize { get; private set; } = new (16, 16);
    [SerializeField] private Vector2Int _size = new (8, 8);
    [SerializeField] private Vector2 _scale = new(1, 1);
    
    [Header("Constraints")]
    [SerializeField] private PlayerInput _playerInput;

    public RectTransform rectTransform => _rectTransform;
    
    private RectTransform _rectTransform;
    private Vector2 _onGridPosition;
    private Vector2Int _tileGridPosition;

    private InventoryItem[,] _inventoryItemSlot;
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        _inventoryItemSlot = new InventoryItem[_size.x, _size.y];

        _rectTransform.localScale = new (_scale.x, _scale.y);
        _rectTransform.sizeDelta = new(tileSize.x * _size.x, tileSize.y * _size.y);
    }
    
    public Vector2Int GetTileGridPosition(Vector2 mousePosition)
    {
        _onGridPosition.x = mousePosition.x - _rectTransform.position.x;
        _onGridPosition.y = _rectTransform.position.y - mousePosition.y;

        _tileGridPosition.x = (int)(_onGridPosition.x / (tileSize.x * _scale.x));
        _tileGridPosition.y = (int)(_onGridPosition.y / (tileSize.y * _scale.y));

        return _tileGridPosition;
    }

    public bool PlaceItem(InventoryItem inventoryItem, Vector2Int position, ref InventoryItem overlappedItem)
    {
        if (!IsItemInsideBoundary(position, inventoryItem.GetActualSize()))
            return false;

        if (IsOverlapping(position, inventoryItem.GetActualSize(), ref overlappedItem))
        {
            overlappedItem = null;
            return false;
        }

        if (overlappedItem)
            SetInventoryItemSlot(overlappedItem, overlappedItem.gridPosition, false);
        
        inventoryItem.SetParent(_rectTransform);
        
        SetInventoryItemSlot(inventoryItem, position, true);

        inventoryItem.gridPosition = position;
        inventoryItem.transform.localPosition = GetPositionOnGrid(position);

        return true;
    }

    public Vector3 GetPositionOnGrid(Vector2Int position)
    {
        return new(
            position.x * tileSize.x,
            -(position.y * tileSize.y)
        );
    }

    public bool CanPlaceItem(InventoryItem inventoryItem, Vector2Int position)
    {
        if (!IsItemInsideBoundary(position, inventoryItem.GetActualSize()))
            return false;

        InventoryItem dummyItem = null;
        
        if (IsOverlapping(position, inventoryItem.GetActualSize(), ref dummyItem))
            return false;

        return true;
    }

    private void SetInventoryItemSlot(InventoryItem inventoryItem, Vector2Int position, bool set)
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
        InventoryItem inventoryItem = _inventoryItemSlot[positionOnGrid.x, positionOnGrid.y];
        
        if (inventoryItem)
        {
            SetInventoryItemSlot(inventoryItem, inventoryItem.gridPosition, false);
            inventoryItem.SetParent(_rectTransform.parent, _scale);
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

    public InventoryItem GetItem(Vector2Int position)
    {
        if (position.x < 0 || position.x >= _inventoryItemSlot.GetLength(0)|| position.y < 0 || position.y >= _inventoryItemSlot.GetLength(1))
            return null;

        return _inventoryItemSlot[position.x, position.y];
    }
}
