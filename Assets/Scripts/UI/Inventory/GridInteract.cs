using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ItemGrid))]
public class GridInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private InventoryController _inventoryController;
    private ItemGrid _itemGrid;

    private void Awake()
    {
        _itemGrid = GetComponent<ItemGrid>();
    }

    private void Start()
    {
        _inventoryController = Player.instance.inventoryController;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _inventoryController.SetItemGrid(_itemGrid);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _inventoryController.SetItemGrid(null);
    }
}
