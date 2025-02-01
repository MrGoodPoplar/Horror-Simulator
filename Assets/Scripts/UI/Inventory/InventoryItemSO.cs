using System;
using System.Collections.Generic;
using UI.Inventory.Actions;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "UI/Inventory/Item")]
public class InventoryItemSO : ScriptableObject, IGuided
{
    [SerializeField] private LocalizedString _itemName;
    [field: SerializeField, TextArea] public string itemDescription { get; private set; }
    [field: SerializeField] public Vector2Int size { get; private set; }
    [field: SerializeField] public Sprite icon { get; private set; }
    [field: SerializeField] public int maxQuantity { get; private set; } = 1;
    [field: SerializeField] public GameObject prefab { get; private set; }
    [field: SerializeField] private List<InventoryItemAction> _actions;
    [field: SerializeField, HideInInspector] public string guid { get; private set; }
    
    public bool isStackable => maxQuantity > 1;
    public bool isSymmetrical => size.x == size.y;
    public string itemName => _itemName.GetLocalizedString();
    public IReadOnlyList<InventoryItemAction> actions => _actions;
    
    public void GenerateGUID()
    {
        guid = Guid.NewGuid().ToString();
    }
}