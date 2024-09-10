using System;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class RigidBodyPush : MonoBehaviour
{
    [field: SerializeField] public bool canPush { get; set; }
    [field: SerializeField, Range(0, 5)] public float strength { get; set; } = 1.0f;
    
    private FirstPersonController _firstPersonController;
    private CapsuleCollider _capsuleCollider;
    
    private void Awake()
    {
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Start()
    {
        _firstPersonController = Player.instance.firstPersonController;
    }

    private void Update()
    {
        _capsuleCollider.height = _firstPersonController.height;
        _capsuleCollider.center = _firstPersonController.characterControllerCenter;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canPush)
        {
            PushRigidBodies(other);
        }
    }

    private bool PushRigidBodies(Collider other)
    {
        Rigidbody body = other.attachedRigidbody;

        if (body == null || body.isKinematic)
            return false;
        
        Vector3 pushDir = new Vector3(transform.forward.x, 0.0f, transform.forward.z).normalized;

        body.AddForce(pushDir * strength, ForceMode.Impulse);

        return true;
    }
}