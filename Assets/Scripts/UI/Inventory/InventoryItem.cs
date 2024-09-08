using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class InventoryItem : MonoBehaviour
{
    public Vector2Int gridPosition { get; set; }
    public InventoryItemSO inventoryItemSO { get; private set; }
    
    private RectTransform _rectTransform;
    private Vector2 _defaultPivot;
    
    public RectTransform GetRectTransform()
    {
        if (_rectTransform)
            return _rectTransform;

        _rectTransform = GetComponent<RectTransform>();
        _defaultPivot = _rectTransform.pivot;
        
        return _rectTransform;
    }
    
    public void SetPivotForTile(Vector2Int tileSize)
    {
        SetPivot(new (
            1.0f / (2.0f * inventoryItemSO.size.x),
            1.0f - 1.0f / (2.0f * inventoryItemSO.size.y)
        ));
    }

    public void SetPivotToDefault()
    {
        SetPivot(_defaultPivot);
    }

    public void Set(InventoryItemSO inventoryItemSO, ItemGrid itemGrid)
    {
        this.inventoryItemSO = inventoryItemSO;
        
        GetComponent<Image>().sprite = inventoryItemSO.icon;

        GetRectTransform().sizeDelta = new(
            inventoryItemSO.size.x * itemGrid.tileSize.x,
            inventoryItemSO.size.y * itemGrid.tileSize.y
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
}
