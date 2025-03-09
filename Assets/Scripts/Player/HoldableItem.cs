using Cysharp.Threading.Tasks;
using UI.Hotbar;
using UnityEngine;

public class HoldableItem : MonoBehaviour
{
    [field: Header("Holding Settings")]
    [field: SerializeField] public Vector3 holdingPosition { get; private set; }
    [field: SerializeField] public Vector3 holdingRotation { get; private set; }
    [field: SerializeField] public Bounds bounds { get; private set; }
    
    public HotbarSlotSO hotbarSlotSO { get; set; }
    
    private readonly Collider[] _collisions = new Collider[4];
    
    public virtual void Take() {}

    public virtual void Hide() {}

    public virtual async UniTask TakeAsync()
    {
        Take();
        await UniTask.CompletedTask;
    }

    public virtual async UniTask HideAsync()
    {
        Hide();
        await UniTask.CompletedTask;
    }
    
    public bool IsColliding(LayerMask layer)
    {
        Vector3 boxCenter = transform.TransformPoint(bounds.center);
        Vector3 boxSize = bounds.extents * 2;
        Quaternion rotation = transform.rotation;

        int hits = Physics.OverlapBoxNonAlloc(boxCenter, boxSize / 2, _collisions, rotation, layer);
        return hits > 0 && _collisions[0].gameObject != gameObject;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 boxCenter = transform.TransformPoint(bounds.center);
        Vector3 boxSize = bounds.size; 
        
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}
