using System;
using UI.Inventory.Inventory_Item;
using UnityEngine;

namespace Interaction_System.Grab_Item
{
    [Serializable]
    public record GrabInteractableConstraints
    {
        [field: SerializeField] public InteractableVisualSO interactableVisualSO { get; protected set; }
        [field: SerializeField] public InventoryItemSO inventoryItemSO { get; protected set; }
    }
}