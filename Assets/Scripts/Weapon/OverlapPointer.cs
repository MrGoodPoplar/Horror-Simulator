using UnityEngine;

public class OverlapPointer : MonoBehaviour
{
    [field: SerializeField] public LayerMask layer { get; private set; }
    [SerializeField] private LayerMask _hitLayers;
    
    private void Start()
    {
        transform.SetParent(null);
    }
    
    private void Update()
    {
        if (!Player.Instance.holdingItemController.currentHoldable)
            return;

        Transform holdableTransform = Player.Instance.holdingItemController.currentHoldable.transform;
        Ray ray = new Ray(holdableTransform.position, holdableTransform.rotation * Vector3.forward);
        
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _hitLayers))
            return;
        
        transform.position = hit.point;
        transform.rotation = Quaternion.LookRotation(hit.normal);
    }
}