using System;
using UnityEngine;

public class HitPointer : MonoBehaviour
{
    [SerializeField] protected LayerMask _hitLayers;
    [SerializeField] protected Camera _camera;

    private void Start()
    {
        transform.SetParent(null);
    }

    private void Update()
    {
        FollowHitPoint();
    }

    private void FollowHitPoint()
    {
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _hitLayers))
            transform.position = hit.point;
    }
}