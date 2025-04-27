using System;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class RigidBodyPush : MonoBehaviour
{
    [field: SerializeField] public bool canPush { get; set; }
    [field: SerializeField, Range(0, 5)] public float strength { get; set; } = 1.0f;
    
    private FirstPersonController _firstPersonController;
    private CapsuleCollider _capsuleCollider;
    private float _heightOffset;
    private Vector3 _centerOffset;
    
    private void Awake()
    {
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Start()
    {
        _firstPersonController = Player.Instance.firstPersonController;
        
        _heightOffset = _capsuleCollider.height - _firstPersonController.height;
        _centerOffset = _capsuleCollider.center - _firstPersonController.center;
    }

    private void Update()
    {
        _capsuleCollider.height = _firstPersonController.height + _heightOffset;
        _capsuleCollider.center = _firstPersonController.characterControllerCenter + _centerOffset;
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

        Vector3 moveInput = new Vector3(_firstPersonController.moveDirection.x, 0, _firstPersonController.moveDirection.z);
        Vector3 pushDirection = moveInput.sqrMagnitude > 0.1f 
            ? moveInput.normalized 
            : new Vector3(transform.forward.x, 0, transform.forward.z).normalized;

        body.AddForce(pushDirection * strength, ForceMode.Impulse);

        return true;
    }
}