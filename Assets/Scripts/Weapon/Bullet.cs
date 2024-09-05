using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    private Rigidbody _rb;
    private Vector3 _startPosition;
    private float _destroyDistance;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float distanceTraveled = Vector3.Distance(_startPosition, transform.position);
        
        if (distanceTraveled >= _destroyDistance)
        {
            Destroy(gameObject);
        }
    }
    
    public void StartProjectile(float speed, float destroyDistance)
    {
        _rb.velocity = transform.forward * speed;
        _startPosition = transform.position;
        _destroyDistance = destroyDistance;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(transform.position);
        Destroy(gameObject);
    }
}
