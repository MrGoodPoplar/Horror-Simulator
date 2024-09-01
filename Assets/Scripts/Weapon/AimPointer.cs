using UnityEngine;

public class AimPointer : MonoBehaviour
{
    [SerializeField] private LayerMask _hitLayers;
    [SerializeField] private Camera _camera;

    void Update()
    {
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _hitLayers))
        {
            transform.position = hit.point;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }
    }
}