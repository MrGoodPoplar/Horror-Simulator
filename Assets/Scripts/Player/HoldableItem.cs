using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class HoldableItem : MonoBehaviour
{
    [field: Header("Holding Settings")]
    [field: SerializeField] public Vector3 holdingPosition { get; private set; }
    [field: SerializeField] public Vector3 holdingRotation { get; private set; }
    [field: SerializeField] public Bounds bounds { get; private set; }
    
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
    
    public List<GameObject> CheckCollisions(LayerMask layerMask)
    {
        List<GameObject> collidingObjects = new List<GameObject>();

        Vector3 boxCenter = transform.TransformPoint(bounds.center);
        Vector3 boxSize = bounds.extents * 2;
        Quaternion rotation = transform.rotation;

        int hits = Physics.OverlapBoxNonAlloc(boxCenter, boxSize / 2, _collisions, rotation, layerMask);

        for (int i = 0; i < hits; i++)
        {
            if (_collisions[i].gameObject != gameObject)
            {
                collidingObjects.Add(_collisions[i].gameObject);
                Debug.Log($"COLLIDE: {gameObject.name}");
            }
        }

        return collidingObjects;
    }

    
    private void OnDrawGizmos()
    {
        Vector3 boxCenter = transform.TransformPoint(bounds.center);
        Vector3 boxSize = bounds.size; 
        
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
        Gizmos.matrix = Matrix4x4.identity;
    }

}
