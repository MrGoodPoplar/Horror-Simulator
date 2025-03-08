using System.Collections.Generic;
using UnityEngine;

public class HitPointer : MonoBehaviour
{
    private const int RAYS = 3;
    
    [field: SerializeField] public LayerMask layer { get; private set; }
    [SerializeField] private LayerMask _hitLayers;
    [SerializeField] private Camera _camera;
    [SerializeField, Range(0, 0.5f)] private float _space = 0.05f;
    [SerializeField, Range(0, 5f)] private float _maxAdvancedCalculateDistance = 1f;

    private RaycastHit[] _raycastHits = new RaycastHit[RAYS];
    private bool _isAdvancedCalculation;
    private void Update()
    {
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _hitLayers))
        {
            transform.position = hit.point;
            _isAdvancedCalculation = Vector3.Distance(_camera.transform.position, hit.point) <= _maxAdvancedCalculateDistance;
            
            if (!_isAdvancedCalculation || !TryCalculateSurfaceNormal(out Vector3 normal))
                normal = hit.normal;
            
            transform.rotation = Quaternion.LookRotation(normal);
        }
    }
    
    private bool TryCalculateSurfaceNormal(out Vector3 normal)
    {
        normal = Vector3.zero;
        _raycastHits = GetRaycastHits();

        if (_raycastHits.Length < 3)
            return false;

        Vector3 p1 = _raycastHits[0].point;
        Vector3 p2 = _raycastHits[1].point;
        Vector3 p3 = _raycastHits[2].point;

        Vector3 v1 = p2 - p1;
        Vector3 v2 = p3 - p1;

        normal = Vector3.Cross(v1, v2).normalized;
        
        return true;
    }
    
    private RaycastHit[] GetRaycastHits()
    {
        Vector3 center = new Vector3(0.5f, 0.5f, 0);
        RaycastHit[] raycastHits = new RaycastHit[RAYS];
        
        for (int i = 0; i < RAYS; i++)
        {
            float angle = i * Mathf.PI * 2 / RAYS;
        
            float x = center.x + Mathf.Cos(angle) * _space;
            float y = center.y + Mathf.Sin(angle) * _space;
        
            x = Mathf.Clamp01(x);
            y = Mathf.Clamp01(y);
            
            Ray ray = _camera.ViewportPointToRay(new Vector3(x, y, 0));

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _hitLayers))
                raycastHits[i] = hit;
        }

        return raycastHits;
    }
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !_isAdvancedCalculation)
            return;

        Gizmos.color = Color.red;

        foreach (var hit in _raycastHits)
        {
            Gizmos.DrawSphere(hit.point, 0.05f);
        }
    }
}