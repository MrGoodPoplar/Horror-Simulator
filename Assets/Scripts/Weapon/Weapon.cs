using System;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour
{
    public int bulletsInClip { get; private set; }
    
    [field: Header("Settings")]
    [field: SerializeField] public Vector3 aimPosition { get; private set; }
    [field: SerializeField] public Quaternion aimRotation { get; private set; }
    [field: SerializeField] public int clipSize { get; private set; }
    
    [SerializeField] private float _fireDelay = 0.3f;

    [field: Header("Recoil Settings")]
    [field: SerializeField] public Vector3 recoil { get; private set; }
    [field: SerializeField] public float recoilForce { get; private set; }
    [field: SerializeField] public float recoildSpeed { get; private set; }
    [field: SerializeField] public float recoilDuration { get; private set; }
    
    [Header("Bullet Settings")]
    [SerializeField] private float _bulletSpeed = 10f;
    [SerializeField] private float _flightDistance = 300f;
    
    [Header("Accuracy Settings")]
    [SerializeField] private Vector2 _accuracyDeviation = new (0.5f, 0.5f);

    [Header("Bullet Pool Settings")]
    [SerializeField] private int _poolDefaultSize = 10;
    [SerializeField] private int _poolMaxSize = 100;
    [SerializeField] private bool _collectionCheck = false;
    
    [Header("Constraints")]
    [SerializeField] private Transform _bulletSpawn;
    [SerializeField] private Bullet _bullet;
    
    private float _lastShotTime;
    private IObjectPool<Bullet> _bulletPool;

    private void Awake()
    {
        InitPoolObjects();
    }

    public bool Fire(Vector3 target)
    {
        if (Time.time >= _lastShotTime + _fireDelay && bulletsInClip > 0)
        {
            _lastShotTime = Time.time;

            Vector3 direction = (target - _bulletSpawn.position).normalized;
            direction = ApplyAccuracyDeviation(direction, target);

            Bullet bullet = _bulletPool.Get();
            bullet.transform.position = _bulletSpawn.position;
            bullet.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            
            bullet.StartProjectile(_bulletSpeed, _flightDistance);
            SetBulletsInClip(bulletsInClip - 1);
                
            return true;
        }
        
        return false;
    }

    private void InitPoolObjects()
    {
        _bulletPool = new ObjectPool<Bullet>(CreateBullet, OnGetBulletFromPool, OnReleaseBulletToPool, OnDestroyPooledBullet,
            _collectionCheck, _poolDefaultSize, _poolMaxSize);
    }

    private Vector3 ApplyAccuracyDeviation(Vector3 direction, Vector3 target)
    {
        float distanceToTarget = Vector3.Distance(_bulletSpawn.position, target);

        float accuracyFactor = Mathf.Clamp01(distanceToTarget / _flightDistance);
        float xDeviation = Random.Range(-_accuracyDeviation.x, _accuracyDeviation.x) * accuracyFactor;
        float yDeviation = Random.Range(-_accuracyDeviation.y, _accuracyDeviation.y) * accuracyFactor;

        Vector3 deviation = new Vector3(xDeviation, yDeviation, 0);
        return Quaternion.Euler(deviation) * direction;
    }

    private Bullet CreateBullet()
    {
        Bullet bullet = Instantiate(_bullet);
        bullet.SetBulletPool(_bulletPool);
        return bullet;
    }

    private void OnGetBulletFromPool(Bullet bullet)
    {
        bullet.gameObject.SetActive(true);
    }

    private void OnReleaseBulletToPool(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    private void OnDestroyPooledBullet(Bullet bullet)
    {
        Destroy(bullet.gameObject);
    }
    
    public void SetBulletsInClip(int value)
    {
        bulletsInClip = Mathf.Clamp(value, 0, clipSize);
    }
}