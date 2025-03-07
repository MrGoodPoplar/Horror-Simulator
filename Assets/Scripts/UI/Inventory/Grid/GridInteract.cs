using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Inventory
{
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
            _inventoryController = Player.Instance.inventoryController;
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
}
