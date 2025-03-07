using System;
using Cysharp.Threading.Tasks;
using UI.Inventory;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(FirstPersonController))]
public class ShooterController : MonoBehaviour
{
    public event Action OnFire;
    public event Action OnDryFire;
    public event Action<int> OnReload;
    public event Action OnReloadEnd;
    
    public bool canAim { get; set; } = true;
    public bool canFire { get; set; } = true;
    public bool canReload { get; set; } = true;

    public int reservedBulletCount { get; private set; }
    public bool isAiming { get; private set; }
    
    [Header("Aim Settings")]
    [SerializeField, Range(0, 10)] private float _aimSensitivityReducer = 2f;
    [SerializeField, Range(10, 150)] private float _aimFOV = 30f;
    [SerializeField] private float _timeToAim = 0.1f;

    [Header("Constraints")]
    [SerializeField] private HitPointer _hitPointer;
    [SerializeField] private CameraRecoil _cameraRecoil;
    [SerializeField] private HoldingItemMovement _holdingItemMovement;

    private bool _isAimingTransition;
    private float _defaultFOV;
    private float _defaultSensitivity;
    private PlayerInput _playerInput;
    private FirstPersonController _firstPersonController;
    private InventoryController _inventoryController;
    private Weapon _currentWeapon;

    private void Start()
    {
        _playerInput = Player.Instance.playerInput;
        _inventoryController = Player.Instance.inventoryController;
        _firstPersonController = Player.Instance.firstPersonController;

        _playerInput.OnFire += OnFirePerformed;
        _playerInput.OnReload += OnReloadPerformed;
        
        Player.Instance.holdingItemController.OnTake += HoldingItemOnTakePerformed;
        Player.Instance.holdingItemController.OnHideAfter += HoldingItemOnHideAfterPerformed;

        _defaultFOV = _firstPersonController.playerCamera.fieldOfView;
        _defaultSensitivity = _firstPersonController.sensitivity;
    }

    private void OnDestroy()
    {
        _playerInput.OnFire -= OnFirePerformed;
        _playerInput.OnReload -= OnReloadPerformed;
        
        Player.Instance.holdingItemController.OnTake -= HoldingItemOnTakePerformed;
        Player.Instance.holdingItemController.OnHideAfter -= HoldingItemOnHideAfterPerformed;
    }

    private void Update()
    {
        HandleAiming().Forget();
    }
    
    private async UniTaskVoid HandleAiming()
    {
        bool aimStateChanged = isAiming != _playerInput.isAiming || !canAim || (isAiming && !_currentWeapon);
        
        if (_isAimingTransition || !aimStateChanged || (!canAim && !isAiming))
            return;

        bool aim = _playerInput.isAiming && canAim && _currentWeapon;
        float elapsedTime = 0f;
        float targetFov = aim ? _aimFOV : _defaultFOV;
        float targetSensitivity = aim ? _defaultSensitivity / _aimSensitivityReducer : _defaultSensitivity;
        Vector3 targetPosition = aim ? _currentWeapon.aimPosition : Vector3.zero;
        Quaternion targetRotation = aim ? _currentWeapon.aimRotation : Quaternion.identity;
        
        _isAimingTransition = true;
        isAiming = !isAiming && _currentWeapon;

        while (elapsedTime < _timeToAim)
        {
            float t = elapsedTime / _timeToAim;
            elapsedTime += Time.deltaTime;

            _firstPersonController.playerCamera.fieldOfView = Mathf.Lerp(_firstPersonController.playerCamera.fieldOfView, targetFov, t);
            _firstPersonController.sensitivity = Mathf.Lerp(_firstPersonController.sensitivity, targetSensitivity, t);

            if (_currentWeapon)
            {
                _currentWeapon.transform.localPosition = Vector3.Lerp(_currentWeapon.transform.localPosition, targetPosition, t);
                _currentWeapon.transform.localRotation = Quaternion.Lerp(_currentWeapon.transform.localRotation, targetRotation, t);
            }

            await UniTask.Yield();
        }

        _isAimingTransition = false;
    }
    
    private void OnFirePerformed()
    {
        if (canFire && _currentWeapon && _currentWeapon.Fire(_hitPointer.transform.position))
        {
            _cameraRecoil?.RecoilFire(_currentWeapon.recoil, _currentWeapon.recoilForce, _currentWeapon.recoilSpeed);
            _holdingItemMovement?.ApplyRecoil(_currentWeapon.recoilForce, _currentWeapon.recoilSpeed, _currentWeapon.recoilDuration);
            
            OnFire?.Invoke();
        }
    }
    
    private void OnReloadPerformed()
    {
        if (!_currentWeapon)
            return;
        
        int availableAmmoCount = GetAvailableAmmoCount();
        int totalToReloadCount = _currentWeapon.clipSize - _currentWeapon.bulletsInClip;
        int totalToReload = Mathf.Clamp(availableAmmoCount, 0, totalToReloadCount);

        if (canReload && totalToReload > 0)
            OnReload?.Invoke(totalToReload);
    }

    private int GetAvailableAmmoCount()
    {
        return _currentWeapon ? _inventoryController.GetItemCountInInventory(_currentWeapon.bulletItemSO) : 0;
    }

    public void ReserveBullets(int count)
    {
        if (_currentWeapon && _inventoryController.RemoveInventoryItem(_currentWeapon.bulletItemSO, count))
        {
            reservedBulletCount += count;
        }
    }

    public void RetrieveReserve()
    {
        if (_currentWeapon && reservedBulletCount > 0)
        {
            int toReturn = reservedBulletCount;
            _inventoryController.AddItemToInventory(_currentWeapon.bulletItemSO, ref toReturn);
            reservedBulletCount = 0;
        }
    }
    
    public bool TakeAmmo(int count, bool reserveForNext = false)
    {
        if (!_currentWeapon)
            return false;
        
        if (reservedBulletCount >= count)
        {
            reservedBulletCount -= count;
            if (reserveForNext && reservedBulletCount < count)
                ReserveBullets(count);
            
            return true;
        }
        
        int neededFromInventory = count - reservedBulletCount;
        bool enoughAmmo = _inventoryController.RemoveInventoryItem(_currentWeapon.bulletItemSO, neededFromInventory);

        if (enoughAmmo)
            reservedBulletCount = 0;

        return enoughAmmo;
    }


    public void ToggleWeaponInteraction(bool toggle)
    {
        if (!_currentWeapon)
            return;
        
        bool isReloading = !_currentWeapon.reloadHandler.IsUnityNull() && _currentWeapon.reloadHandler.isReloading;
        
        canAim = toggle && !isReloading;
        canReload = toggle;
        canFire = toggle && !isReloading;
    }

    public void ReloadEnd()
    {
        OnReloadEnd?.Invoke();
    }

    public void DryFire()
    {
        OnDryFire?.Invoke();
    }
    
    private void HoldingItemOnTakePerformed(HoldableItem holdable)
    {
        if (holdable.transform.TryGetComponent(out Weapon weapon))
        {
            _currentWeapon = weapon;

            _currentWeapon.transform.localPosition = Vector3.zero;
            _currentWeapon.transform.localRotation = Quaternion.identity;
        }
    }
    
    private void HoldingItemOnHideAfterPerformed(HoldableItem holdable)
    {
        if (holdable.transform.TryGetComponent(out Weapon weapon))
        {
            if (weapon != _currentWeapon)
                Debug.LogWarning($"Hidden holding weapon [{weapon.name}] is not the same as current weapon [{_currentWeapon.name}]!");
            
            _currentWeapon = null;
        }
    }
}
