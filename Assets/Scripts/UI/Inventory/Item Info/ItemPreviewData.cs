using System;
using UnityEngine;

namespace UI.Inventory.Item_Preview
{
    [Serializable]
    public class ItemPreviewData
    {
        [field: SerializeField] public GameObject prefab { get; private set; }
        [field: SerializeField] public Vector3 scale { get; private set; }
        [field: SerializeField] public Vector3 rotation { get; private set; }
    }
}