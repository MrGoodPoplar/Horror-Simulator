using System;
using UnityEngine;

public class RigidBodyPush : MonoBehaviour
{
	[field: SerializeField] public LayerMask pushLayers { get; set; }
    [field: SerializeField] public bool canPush { get; set; }
    [field: SerializeField, Range(0, 5)] public float strength { get; set; } = 1.0f;

    [SerializeField, Range(-1, 0)] private float _ignoreHeightThreshold = -0.1f;
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
	    if (canPush)
	    {
		    PushRigidBodies(hit);
		    FirstPersonController.instance.canStepOffset = !IsInPushLayers(hit.gameObject.layer);
	    }
    }
    
    private bool IsInPushLayers(int layer)
    {
	    return ((1 << layer) & pushLayers) != 0;
    }
    
    private void PushRigidBodies(ControllerColliderHit hit)
    {
	    Rigidbody body = hit.collider.attachedRigidbody;
	    
    	if (body == null || body.isKinematic)
		    return;

    	var bodyLayerMask = 1 << body.gameObject.layer;
	    
    	if ((bodyLayerMask & pushLayers.value) == 0)
		    return;

    	if (hit.moveDirection.y < _ignoreHeightThreshold)
		    return;

    	Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);
    	body.AddForce(pushDir * strength, ForceMode.Impulse);
    }
}
