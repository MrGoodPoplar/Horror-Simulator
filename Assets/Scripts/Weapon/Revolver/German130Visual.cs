using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

[RequireComponent(typeof(Animator), typeof(Weapon), typeof(German130BulletVisual))]
public class German130Visual : MonoBehaviour, IWeaponReloadHandler
{
    private const string IS_AIMING = "isAiming";
    private const string VELOCITY = "velocity";
    private const string RELOAD = "reload";
    private const string FORCE_STOP_RELOAD = "forceStopReload";

    public event Action OnBulletInsert;
    public event Action OnCylinderSpin;
    public event Action<int> OnDropShells;
    
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

    [Header("Trigger Settings")]
    [SerializeField] private Transform _trigger;
    [SerializeField] private Vector3 _pressedRotation;
    [SerializeField] private float _pressInsideDuration = 0.1f;
    [SerializeField] private float _pressOutsideDuration = 0.2f;

    [Header("Effects")]
    [SerializeField] private VisualEffect _muzzleFlash;
    
    public bool isReloading { get; private set; }
    
    private Animator _animator;
    private ShooterController _shooterController;
    private German130BulletVisual _bulletVisual;
    private PlayerInput _playerInput;
    private Weapon _german130;
    private IMoveable _moveable;

    private int _currentChamberIndex;
    private int _emptyShellsInside;
    private string _currentReloadAnimationState;
    private bool _isReloadAnimationPlaying;
    private bool _isCylinderRotating;
    private bool _reloadingInterrupted;
    private bool _isHandlingReloadInterruption;
    private bool _reloadProcess;
    
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
        _shooterController = Player.Instance.shooterController;
        _moveable = Player.Instance.firstPersonController;
        _playerInput = Player.Instance.playerInput;
        
        _shooterController.OnReload += OnReloadPerformed;
        _shooterController.OnFire += OnFirePerformed;
        _shooterController.OnDryFire += OnDryFirePerformed;
        
        _playerInput.OnOpenHUD += OnOpenHudPerformed;
        
        Player.Instance.playerInput.OnFire += OnInputFirePerformed;
        
        _german130.OnHide += InterruptReloadAnimationAsync;
        
        _emptyShellsInside = _german130.bulletsInClip;
        _bulletVisual.HideBullets(_german130.clipSize - _german130.bulletsInClip);
    }
    
    private void OnDestroy()
    {
        _shooterController.OnReload -= OnReloadPerformed;
        _shooterController.OnFire -= OnFirePerformed;
        _shooterController.OnDryFire -= OnDryFirePerformed;

        _playerInput.OnOpenHUD -= OnOpenHudPerformed;
        
        Player.Instance.playerInput.OnFire -= OnInputFirePerformed;
        
        _german130.OnHide -= InterruptReloadAnimationAsync;
    }

    private void Update()
    {
        HandleReloadingAnimation().Forget();
    }

    private async UniTaskVoid HandleReloadingAnimation()
    {
        if (_isHandlingReloadInterruption)
            return;
        
        _animator.SetBool(IS_AIMING, _shooterController.isAiming);
        _animator.SetFloat(VELOCITY, _moveable.speedVertical);

        if (_isReloadAnimationPlaying && !AnimatorIsPlaying(_currentReloadAnimationState))
        {
            _isHandlingReloadInterruption = true;
            
            if (_reloadingInterrupted)
                await SmoothBulletsReverseAsync(_cylinderRotationDuration / 2, _rotationAngleOffset);

            isReloading = false;
            _isReloadAnimationPlaying = false;
            _reloadingInterrupted = false;
            _isHandlingReloadInterruption = false;
            
            _shooterController.RetrieveReserve();
            _shooterController.ToggleWeaponInteraction(!Player.Instance.HUDController.isHUDView);
            _shooterController.ReloadEnd();
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

    private void OnGetBulletShellFromPool(BulletShell bulletShell) => bulletShell.gameObject.SetActive(true);

    private void OnReleaseBulletShellToPool(BulletShell bulletShell) => bulletShell.gameObject.SetActive(false);

    private void OnDestroyPooledBulletShell(BulletShell bulletShell) => Destroy(bulletShell);
    
    private void HandleReloadEnd() // Animation Event
    {
        _shooterController.RetrieveReserve();
        SmoothBulletsReverseAsync(_cylinderRotationDuration / 2, _rotationAngleOffset).Forget();
    }
    
    private void AddBulletToClip() // Animation Event
    {
        bool isAmmoAvailable = _shooterController.TakeAmmo(1);

        if (isAmmoAvailable)
        {
            _german130.SetBulletsInClip(_german130.bulletsInClip + 1);
            OnBulletInsert?.Invoke();
        }
        else
            InterruptReloadAnimation();

        if (_reloadingInterrupted)
            _animator.SetTrigger(FORCE_STOP_RELOAD);
    }
    
    private void DropShells(int count) // Animation Event
    {
        _bulletVisual.HideBullets(_emptyShellsInside, _german130.bulletsInClip);
        German130Bullet[] bullets = _bulletVisual.bullets.Take(_emptyShellsInside).ToArray();

        OnDropShells?.Invoke(_emptyShellsInside);
        
        foreach (German130Bullet bullet in bullets)
        {
            bullet.Hide();

            if (_emptyShellsInside - 1 >= 0)
            {
                _bulletShellPool.Get().DropAsync(bullet.transform.position, _bulletShellLifeSpan).Forget();
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
    
    private string GetReloadAnimationTrigger(int bullets)
    {
        return RELOAD + bullets;
    }
    
    private void OnFirePerformed()
    {
        PressTriggerAnimationAsync().Forget();
        _muzzleFlash.Play();
        
        _emptyShellsInside++;
        _currentChamberIndex = (_currentChamberIndex + 1) % 6;
        RotateCylinder(_currentChamberIndex, true, _rotationAngleOffset).Forget();;
    }

    private void OnDryFirePerformed()
    {
        PressTriggerAnimationAsync().Forget();
    }

    private void OnReloadPerformed(int totalToReload)
    {
        if (_shooterController.reservedBulletCount == 0)
            _shooterController.ReserveBullets(1);
        
        ReloadAsync(totalToReload).Forget();
    }

    private async UniTaskVoid ReloadAsync(int totalToReload)
    {
        _animator.ResetTrigger(FORCE_STOP_RELOAD);
        _reloadProcess = true;
        _reloadingInterrupted = false;
        isReloading = true;
        
        _shooterController.ToggleWeaponInteraction(false);
        RotateCylinder(_currentChamberIndex = 0, false).Forget();;
        
        _animator.SetTrigger(GetReloadAnimationTrigger(totalToReload));

        await UniTask.WaitForSeconds(_transitionDuration);
        
        SmoothBulletsReverseAsync(_cylinderRotationDuration / 2).Forget();
        
        var clipInfo = _animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
            _currentReloadAnimationState = clipInfo[0].clip.name;

        _isReloadAnimationPlaying = clipInfo.Length > 0;
        _reloadProcess = false;
    }

    private async UniTask SmoothBulletsReverseAsync(float duration, float angleOffset = 0)
    {
        if (_german130.bulletsInClip > 0)
        {
            await RotateByFullTurnsAsync(1, duration, angleOffset);
            ReverseBulletsActive(_german130.bulletsInClip);
        }
    }

    private async UniTask RotateByFullTurnsAsync(float fullTurns, float duration, float angleOffset = 0)
    {
        if (angleOffset < 360)
            OnCylinderSpin?.Invoke();
        
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
    
    private void OnInputFirePerformed() => InterruptReloadAnimation();

    private void OnOpenHudPerformed() => InterruptReloadAnimation();

    private void InterruptReloadAnimation() => _reloadingInterrupted = true;
    
    private async UniTask InterruptReloadAnimationAsync()
    { 
        InterruptReloadAnimation();
        await UniTask.WaitUntil(() => !_isReloadAnimationPlaying && !_reloadProcess);
    }

    private async UniTaskVoid PressTriggerAnimationAsync()
    {
        Quaternion initialRotation = _trigger.localRotation;
        Quaternion targetRotation = Quaternion.Euler(_pressedRotation);
        
        float elapsedTime = 0;
        while (elapsedTime < _pressInsideDuration)
        {
            elapsedTime += Time.deltaTime;
            _trigger.localRotation = Quaternion.Lerp(initialRotation, targetRotation, elapsedTime / _pressInsideDuration);
            await UniTask.Yield();
        }
        
        elapsedTime = 0;
        while (elapsedTime < _pressOutsideDuration)
        {
            elapsedTime += Time.deltaTime;
            _trigger.localRotation = Quaternion.Lerp(targetRotation, initialRotation, elapsedTime / _pressOutsideDuration);
            await UniTask.Yield();
        }
    }
}
