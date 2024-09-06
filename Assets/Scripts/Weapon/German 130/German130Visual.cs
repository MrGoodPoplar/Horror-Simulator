using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Animator), typeof(Weapon))]
public class German130Visual : MonoBehaviour
{
    private const string IS_AIMING = "isAiming";
    private const string VELOCITY = "velocity";
    private const string RELOAD = "reload";
    private const string FORCE_STOP_RELOAD = "forceStopReload";

    [Header("Animation Settings")]
    [SerializeField] private float _transitionDuration = 0.26f;
    
    [Header("Cylinder Settings")]
    [SerializeField] private Transform _cylinder;
    [SerializeField] private float _cylinderRotationDuration = 0.3f;
    
    [Header("Bullet Pool Settings")]
    [SerializeField] private int _poolDefaultSize = 12;
    [SerializeField] private int _poolMaxSize = 24;
    [SerializeField] private bool _collectionCheck = false;
    
    [Header("Bullets Settings")]
    [SerializeField] private German130Bullet[] _bullets;
    [SerializeField] private BulletShell _bulletShell;
    [SerializeField] private float _bulletShellLifeSpan = 10f;
    
    private Animator _animator;
    private ShooterController _shooterController;
    private FirstPersonController _firstPersonController;
    private Weapon _german130;
    
    private int _currentChamberIndex;
    private string _currentReloadAnimationState;
    private bool _isReloadAnimationPlaying;
    private bool _isCylinderRotating;
    private int _shellsInside;
    private bool _reloadingInterrupted;
    
    private IObjectPool<BulletShell> _bulletShellPool;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _german130 = GetComponent<Weapon>();

        InitPoolObjects();
    }

    private void Start()
    {
        _shooterController = ShooterController.instance;
        _firstPersonController = FirstPersonController.instance;
        
        _shooterController.OnReload += OnReloadPerformed;
        _shooterController.OnFire += OnFirePerformed;
        
        _firstPersonController.playerInput.OnFire += OnInputFirePerformed;

        _shellsInside = _german130.bulletsInClip;
        HideBullets(_german130.clipSize - _german130.bulletsInClip);
    }
    
    private void OnDestroy()
    {
        _shooterController.OnReload -= OnReloadPerformed;
        _shooterController.OnFire -= OnFirePerformed;

        _firstPersonController.playerInput.OnFire -= OnInputFirePerformed;
    }

    private void Update()
    {
        _animator.SetBool(IS_AIMING, _shooterController.isAiming);
        _animator.SetFloat(VELOCITY, _firstPersonController.velocity);

        if (_isReloadAnimationPlaying && !AnimatorIsPlaying(_currentReloadAnimationState))
        {
            _isReloadAnimationPlaying = false;
            ToggleWeaponInteraction(true);
        }
    }
    
    private void InitPoolObjects()
    {
        _bulletShellPool = new ObjectPool<BulletShell>(CreateBulletShell, OnGetBulletShellFromPool, OnReleaseBulletShellToPool, OnDestroyPooledBulletShell,
            _collectionCheck, _poolDefaultSize, _poolMaxSize);
    }

    private BulletShell CreateBulletShell()
    {
        BulletShell bulletShell = Instantiate(_bulletShell);
        bulletShell.SetBulletShellPool(_bulletShellPool);
        return bulletShell;
    }

    private void OnGetBulletShellFromPool(BulletShell bulletShell)
    {
        bulletShell.gameObject.SetActive(true);
    }

    private void OnReleaseBulletShellToPool(BulletShell bulletShell)
    {
        bulletShell.gameObject.SetActive(false);
    }

    private void OnDestroyPooledBulletShell(BulletShell bulletShell)
    {
        Destroy(bulletShell);
    }
    
    private void AddBulletToClip() // Animation Event
    {
        _shellsInside++;
        _german130.SetBulletsInClip(_german130.bulletsInClip + 1);

        if (_reloadingInterrupted)
        {
            _reloadingInterrupted = false;
            _animator.SetTrigger(FORCE_STOP_RELOAD);
        }
    }

    private void DropShells(int count) // Animation Event
    {
        German130Bullet[] bullets = _bullets.Take(count).ToArray();
            
        foreach (German130Bullet bullet in bullets)
        {
            bullet.Hide();

            if (_shellsInside - 1 >= 0)
            {
                _bulletShell = _bulletShellPool.Get();
                _bulletShell.transform.position = bullet.transform.position;
                _bulletShell.transform.rotation = Quaternion.identity;;
                _bulletShell.Drop(_bulletShellLifeSpan);
                
                _shellsInside --;
            }
        }
    }

    private void ShowBullets(int count) // Animation Event
    {
        German130Bullet[] bullets = _bullets.Take(count).ToArray();

        foreach (German130Bullet bullet in bullets)
        {
            bullet.Show();
        }
    }

    private void HideBullets(int count) // Animation Event
    {
        German130Bullet[] bullets = _bullets.Take(count).ToArray();

        foreach (German130Bullet bullet in bullets)
        {
            bullet.Hide();
        }
    }
    
    private string GetReloadAnimationTrigger(int bullets)
    {
        return RELOAD + bullets;
    }
    
    private void OnFirePerformed()
    {
        _currentChamberIndex = (_currentChamberIndex + 1) % 6;
        RotateCylinder(_currentChamberIndex);
    }

    private async void OnReloadPerformed()
    {
        ToggleWeaponInteraction(false);
        
        _currentChamberIndex = 0;
        RotateCylinder(_currentChamberIndex, false);

        int bulletsToReload = _german130.clipSize - _german130.bulletsInClip;
        
        _animator.SetTrigger(GetReloadAnimationTrigger(bulletsToReload));
        await Task.Delay((int)(_transitionDuration * 1000));
        
        _currentReloadAnimationState = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        _isReloadAnimationPlaying = true;
    }

    private async void RotateCylinder(int chamberIndex, bool canReloadAfter = true)
    {
        if (_isCylinderRotating)
            return;

        _isCylinderRotating = true;
        _shooterController.canReload = false;

        int angle = 360 / _german130.clipSize;
        float targetAngle = -(chamberIndex * angle);
        Quaternion initialRotation = _cylinder.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < _cylinderRotationDuration)
        {
            elapsedTime += Time.deltaTime;
            _cylinder.localRotation = Quaternion.Lerp(initialRotation, targetRotation, elapsedTime / _cylinderRotationDuration);
            await Task.Yield();
        }

        _cylinder.localRotation = targetRotation;
        _isCylinderRotating = false;
        _shooterController.canReload = canReloadAfter;
    }
    
    bool AnimatorIsPlaying()
    {
        return _animator.GetCurrentAnimatorStateInfo(0).length > _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
    
    bool AnimatorIsPlaying(string stateName)
    {
        return AnimatorIsPlaying() && _animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    private void ToggleWeaponInteraction(bool toggle)
    {
        _shooterController.canAim = toggle;
        _shooterController.canReload = toggle;
        _shooterController.canFire = toggle;
    }
    
    private void OnInputFirePerformed()
    {
        if (_isReloadAnimationPlaying)
            _reloadingInterrupted = true;
    }
}
