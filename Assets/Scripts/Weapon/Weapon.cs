using UnityEngine;

public class Weapon : MonoBehaviour
{
    [field: Header("Settings")]
    [field: SerializeField] public Vector3 aimPosition { get; private set; }
    [field: SerializeField] public Quaternion aimRotation { get; private set; }
    
    [SerializeField] private float _fireDelay = 0.3f;

    [field: Header("Recoil Settings")]
    [field: SerializeField] public Vector3 recoil { get; private set; }
    [field: SerializeField] public float recoilForce { get; private set; }
    [field: SerializeField] public float recoildSpeed { get; private set; }
    [field: SerializeField] public float recoilDuration { get; private set; }
    
    [Header("Bullet Settings")]
    [SerializeField] private float _bulletSpeed = 10f;
    [SerializeField] private float _flightDistance = 300f;
    
    [Header("Constraints")]
    [SerializeField] private Transform _bulletSpawn;
    [SerializeField] private Bullet _bullet;

    private float _lastShotTime;
    
    public bool Fire(Vector3 target)
    {
        if (Time.time >= _lastShotTime + _fireDelay)
        {
            _lastShotTime = Time.time;

            Vector3 direction = (target - _bulletSpawn.position).normalized;
            Bullet bullet = Instantiate(_bullet, _bulletSpawn.position, Quaternion.LookRotation(direction, Vector3.up));
            bullet.StartProjectile(_bulletSpeed, _flightDistance);
            return true;
        }

        return false;
    }
}
