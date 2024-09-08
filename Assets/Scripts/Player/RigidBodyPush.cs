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
            bool isPushed = PushRigidBodies(hit);

            FirstPersonController.instance.canStepOffset = !isPushed;
        }
    }

    private bool IsInPushLayers(int layer)
    {
        return ((1 << layer) & pushLayers) != 0;
    }

    private bool PushRigidBodies(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        if (body == null || body.isKinematic)
            return false;

        if (!IsInPushLayers(body.gameObject.layer))
            return false;

        if (hit.moveDirection.y < _ignoreHeightThreshold)
            return false;

        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);
        body.AddForceAtPosition(pushDir * strength, hit.point, ForceMode.Impulse);

        return true;
    }
}
