using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Animator), typeof(Weapon), typeof(German130BulletVisual))]
public class German130Visual : MonoBehaviour, IWeaponReloadHandler
{
    private const string IS_AIMING = "isAiming";
    private const string VELOCITY = "velocity";
    private const string RELOAD = "reload";
    private const string FORCE_STOP_RELOAD = "forceStopReload";

    [Header("Animation Settings")]
    [SerializeField] private float _transitionDuration = 0.26f;
    [SerializeField, Range(-180, 180)] private float _rotationAngleOffset = -60.0f;
    
    [Header("Cylinder Settings")]
    [SerializeField] private Transform _cylinderRotationConstraint;
    [SerializeField] private float _cylinderRotationDuration = 0.3f;
    
    [Header("Bullet Pool Settings")]
    [SerializeField] private int _poolDefaultSize = 12;
    [SerializeField] private int _poolMaxSize = 24;
    [SerializeField] private bool _collectionCheck;
    
    [Header("Bullets Settings")]
    [SerializeField] private BulletShell _bulletShell;
    [SerializeField] private float _bulletShellLifeSpan = 10f;
    
    public bool isReloading { get; private set; }
    
    private Animator _animator;
    private ShooterController _shooterController;
    private FirstPersonController _firstPersonController;
    private German130BulletVisual _bulletVisual;
    private PlayerInput _playerInput;
    private Weapon _german130;

    private int _currentChamberIndex;
    private string _currentReloadAnimationState;
    private bool _isReloadAnimationPlaying;
    private bool _isCylinderRotating;
    private int _emptyShellsInside;
    private bool _reloadingInterrupted;
    
    private IObjectPool<BulletShell> _bulletShellPool;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _german130 = GetComponent<Weapon>();
        _bulletVisual = GetComponent<German130BulletVisual>();
        
        InitPoolObjects();
    }

    private void Start()
    {
        _shooterController = Player.instance.shooterController;
        _firstPersonController = Player.instance.firstPersonController;
        _playerInput = Player.instance.playerInput;
        
        _shooterController.OnReload += OnReloadPerformed;
        _shooterController.OnFire += OnFirePerformed;
        _playerInput.OnOpenHUD += OnOpenHudPerformed;
        
        _firstPersonController.playerInput.OnFire += OnInputFirePerformed;

        _emptyShellsInside = _german130.bulletsInClip;
        _bulletVisual.HideBullets(_german130.clipSize - _german130.bulletsInClip);
    }
    
    private void OnDestroy()
    {
        _shooterController.OnReload -= OnReloadPerformed;
        _shooterController.OnFire -= OnFirePerformed;
        _playerInput.OnOpenHUD -= OnOpenHudPerformed;

        _firstPersonController.playerInput.OnFire -= OnInputFirePerformed;
    }

    private void Update()
    {
        HandleReloadingAnimation();
    }

    private void HandleReloadingAnimation()
    {
        _animator.SetBool(IS_AIMING, _shooterController.isAiming);
        _animator.SetFloat(VELOCITY, _firstPersonController.velocity);

        if (_isReloadAnimationPlaying && !AnimatorIsPlaying(_currentReloadAnimationState))
        {
            if (_reloadingInterrupted)
                SmoothBulletsReverse(_rotationAngleOffset);

            isReloading = false;
            _isReloadAnimationPlaying = false;
            _reloadingInterrupted = false;
            _shooterController.ToggleWeaponInteraction(!Player.instance.HUDController.isHUDView);
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
    
    private void HandleReloadEnd() // Animation Event
    {
        SmoothBulletsReverse(_rotationAngleOffset);
    }
    
    private void AddBulletToClip() // Animation Event
    {
        bool isAmmoAvaiable = _shooterController.TakeAmmo(1);

        if (isAmmoAvaiable)
            _german130.SetBulletsInClip(_german130.bulletsInClip + 1);
        else
            InterruptReloadAnimation();

        if (_reloadingInterrupted)
            _animator.SetTrigger(FORCE_STOP_RELOAD);
    }

    private void DropShells() // Animation Event
    {
        German130Bullet[] bullets = _bulletVisual.bullets.Take(_emptyShellsInside).ToArray();
            
        foreach (German130Bullet bullet in bullets)
        {
            bullet.Hide();

            if (_emptyShellsInside - 1 >= 0)
            {
                _bulletShell = _bulletShellPool.Get();
                _bulletShell.DropAsync(bullet.transform.position, _bulletShellLifeSpan).Forget();
                
                _emptyShellsInside --;
            }
        }
    }

    private void ReverseBulletsActive(int count)
    {
        for (int i = _bulletVisual.bullets.Length - 1; i >= 0; i--)
        {
            German130Bullet bullet = _bulletVisual.bullets[i];

            if (count > 0)
            {
                bullet.Show();
                count--;
            }
            else
            {
                bullet.Hide();
            }
        }
    }
    
    private void ShowBullets(int count) // Animation Event
    {
        _bulletVisual.ShowBullets(count);
    }

    private void HideBullets(int count) // Animation Event
    {
        _bulletVisual.HideBullets(count);
    }
    
    private string GetReloadAnimationTrigger(int bullets)
    {
        return RELOAD + bullets;
    }
    
    private void OnFirePerformed()
    {
        _emptyShellsInside++;
        _currentChamberIndex = (_currentChamberIndex + 1) % 6;
        RotateCylinder(_currentChamberIndex, true, _rotationAngleOffset).Forget();;
    }

    private void OnReloadPerformed(int totalToReload)
    {
        ReloadAsync(totalToReload).Forget();
    }

    private async UniTaskVoid ReloadAsync(int totalToReload)
    {
        _reloadingInterrupted = false;
        isReloading = true;
        
        _shooterController.ToggleWeaponInteraction(false);
        RotateCylinder(_currentChamberIndex = 0, false).Forget();;
        
        _animator.SetTrigger(GetReloadAnimationTrigger(totalToReload));

        await UniTask.WaitForSeconds(_transitionDuration);
        
        SmoothBulletsReverse();
        
        _currentReloadAnimationState = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        _isReloadAnimationPlaying = true;
    }

    private void SmoothBulletsReverse(float angleOffset = 0)
    {
        if (_german130.bulletsInClip > 0)
        {
            RotateByFullTurns(1, _cylinderRotationDuration / 2, angleOffset).Forget();;
            ReverseBulletsActive(_german130.bulletsInClip);
        }
    }

    private async UniTaskVoid RotateByFullTurns(float fullTurns, float duration, float angleOffset = 0)
    {
        float totalRotation = fullTurns * 360f + angleOffset;
        float elapsedTime = 0f;
        float startRotationY = _cylinderRotationConstraint.localRotation.eulerAngles.y;
        float targetRotationY = startRotationY + totalRotation;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float currentRotationY = Mathf.Lerp(startRotationY, targetRotationY, t);
            _cylinderRotationConstraint.localRotation = Quaternion.Euler(0f, currentRotationY, 0f);
            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }

        _cylinderRotationConstraint.localRotation = Quaternion.Euler(0f, targetRotationY, 0f);
    }

    
    private async UniTaskVoid RotateCylinder(int chamberIndex, bool canReloadAfter = true, float angleOffset = 0)
    {
        if (_isCylinderRotating)
            return;

        _isCylinderRotating = true;
        _shooterController.canReload = false;

        float angle = 360.0f / _german130.clipSize;
        float targetAngle = -(chamberIndex * angle) + angleOffset;
        
        Quaternion initialRotation = _cylinderRotationConstraint.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
        
        float elapsedTime = 0;
        while (elapsedTime < _cylinderRotationDuration)
        {
            elapsedTime += Time.deltaTime;
            _cylinderRotationConstraint.localRotation = Quaternion.Lerp(initialRotation, targetRotation, elapsedTime / _cylinderRotationDuration);
            await UniTask.Yield();
        }

        _cylinderRotationConstraint.localRotation = targetRotation;
        _isCylinderRotating = false;
        _shooterController.canReload = canReloadAfter;
    }

    
    private bool AnimatorIsPlaying()
    {
        return _animator.GetCurrentAnimatorStateInfo(0).length > _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
    
    private bool AnimatorIsPlaying(string stateName)
    {
        return AnimatorIsPlaying() && _animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }
    
    private void OnInputFirePerformed()
    {
        InterruptReloadAnimation();
    }

    private void OnOpenHudPerformed()
    {
        InterruptReloadAnimation();
    }

    private void InterruptReloadAnimation()
    {
       _reloadingInterrupted = true;
    }
}
