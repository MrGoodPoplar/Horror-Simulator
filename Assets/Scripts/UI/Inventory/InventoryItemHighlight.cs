using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class InventoryItemHighlight : MonoBehaviour
{
    [field: SerializeField] public Color defaultColor { get; private set;}
    [field: SerializeField] public Color allowedColor { get; private set;}
    [field: SerializeField] public Color forbiddenColor { get; private set;}
    
    private RectTransform _rectTransform;
    private Image _image;
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();

        SetColor(defaultColor);
    }

    public void SetColor(Color color)
    {
        _image.color = color;
    }

    public void SetSize(Vector2Int size, Vector2Int tileSize)
    {
        _rectTransform.sizeDelta = new(
            size.x * tileSize.x,
            size.y * tileSize.y
        );
    }

    public void SetPosition(ItemGrid targetGrid, Vector2Int gridPosition)
    {
        _rectTransform.localPosition = targetGrid.GetPositionOnGrid(gridPosition);
    }

    public void SetParent(ItemGrid targetGrid, bool asFirstSibling = false)
    {
        _rectTransform.SetParent(targetGrid.rectTransform, false);

        if (asFirstSibling)
            _rectTransform.SetAsFirstSibling();
    }

    public void Show()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
}
