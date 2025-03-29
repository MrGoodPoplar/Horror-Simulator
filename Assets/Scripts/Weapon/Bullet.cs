using System;
using Surface_System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    private IObjectPool<Bullet> _bulletPool;
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
            _bulletPool.Release(this);
        }
    }

    public void SetBulletPool(IObjectPool<Bullet> bulletPool)
    {
        _bulletPool = bulletPool;
    }
    
    public void StartProjectile(float speed, float destroyDistance)
    {
        _rb.velocity = transform.forward * speed;
        _startPosition = transform.position;
        _destroyDistance = destroyDistance;
    }

    private void OnCollisionEnter(Collision collision)
    {
        var data = Player.Instance.surfaceManager.GetImpactDetails(collision);
        var surfaceImpactHandler = new SurfaceImpactHandler(data);

        surfaceImpactHandler
            .PlaySound(data?.surfaceImpactSound.bulletImpactSounds)
            .PlayVfx();
        
        _bulletPool.Release(this);    
    }
}
