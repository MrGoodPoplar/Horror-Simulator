using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class InventoryItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _quantityText;
    [SerializeField] private Image _quantityBackground;

    public Vector2Int gridPosition { get; set; }
    public InventoryItemSO inventoryItemSO { get; private set; }
    public bool rotated { get; private set; }
    
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

    public void Set(InventoryItemSO inventoryItemSO, ItemGrid itemGrid)
    {
        this.inventoryItemSO = inventoryItemSO;
        
        _quantityBackground.gameObject.SetActive(inventoryItemSO.countable);

        GetComponent<Image>().sprite = inventoryItemSO.icon;

        GetRectTransform().sizeDelta = new(
            GetActualSize().x * itemGrid.tileSize.x,
            GetActualSize().y * itemGrid.tileSize.y
        );
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
            ? Vector3.left * tileSize.x * inventoryItemSO.size.x
            : _defaultQuantityAnchoredPosition;
    }

    public Vector2Int GetActualSize()
    {
        return new(
            rotated ? inventoryItemSO.size.y : inventoryItemSO.size.x,
            rotated ? inventoryItemSO.size.x : inventoryItemSO.size.y
        );
    }
}
