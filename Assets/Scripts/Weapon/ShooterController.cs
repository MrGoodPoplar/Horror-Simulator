using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UI.Inventory;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(FirstPersonController))]
public class ShooterController : MonoBehaviour
{
    public event Action OnFire;
    public event Action<int> OnReload;
    
    public bool canAim { get; set; } = true;
    public bool canFire { get; set; } = true;
    public bool canReload { get; set; } = true;

    public bool isAiming { get; private set; }
    
    [Header("Aim Settings")]
    [SerializeField, Range(0, 10)] private float _aimSensitivityReducer = 2f;
    [SerializeField, Range(10, 150)] private float _aimFOV = 30f;
    [SerializeField] private float _timeToAim = 0.1f;

    [Header("Constraints")]
    [SerializeField] private HitPointer _hitPointer;
    [SerializeField] private CameraRecoil _cameraRecoil;
    [SerializeField] private WeaponMovement _weaponMovement;

    private bool _isAimingTransition;
    private float _defaultFOV;
    private float _defaultSensitivity;
    private PlayerInput _playerInput;
    private FirstPersonController _firstPersonController;
    private InventoryController _inventoryController;
    [SerializeField] private Weapon _currentWeapon; // SerializeField only for test purposes

    private void Start()
    {
        _playerInput = Player.instance.playerInput;
        _inventoryController = Player.instance.inventoryController;
        _firstPersonController = Player.instance.firstPersonController;

        _playerInput.OnFire += OnFirePerformed;
        _playerInput.OnReload += OnReloadPerformed;

        _defaultFOV = _firstPersonController.playerCamera.fieldOfView;
        _defaultSensitivity = _firstPersonController.sensitivity;
    }

    private void OnDestroy()
    {
        _playerInput.OnFire -= OnFirePerformed;
        _playerInput.OnReload -= OnReloadPerformed;
    }

    private void Update()
    {
        HandleAiming().Forget();
    }

    private void OnAiming()
    {
        _currentWeapon.transform.localPosition = isAiming ? _currentWeapon.aimPosition : Vector3.zero;
        _currentWeapon.transform.localRotation = isAiming ? _currentWeapon.aimRotation : Quaternion.identity;
    }
    
    private async UniTaskVoid HandleAiming()
    {
        bool aimStateChanged = isAiming != _playerInput.isAiming || !canAim;

        if (_isAimingTransition || !aimStateChanged || (!canAim && !isAiming))
            return;

        bool aim = _playerInput.isAiming && canAim;
        float elapsedTime = 0f;
        float targetFov = aim ? _aimFOV : _defaultFOV;
        float targetSensitivity = aim ? _defaultSensitivity / _aimSensitivityReducer : _defaultSensitivity;
        Vector3 targetPosition = aim ? _currentWeapon.aimPosition : Vector3.zero;
        Quaternion targetRotation = aim ? _currentWeapon.aimRotation : Quaternion.identity;
        
        _isAimingTransition = true;
        isAiming = !isAiming;
        
        while (elapsedTime < _timeToAim)
        {
            float t = elapsedTime / _timeToAim;
            elapsedTime += Time.deltaTime;

            _firstPersonController.playerCamera.fieldOfView = Mathf.Lerp(_firstPersonController.playerCamera.fieldOfView, targetFov, t);
            _firstPersonController.sensitivity = Mathf.Lerp(_firstPersonController.sensitivity, targetSensitivity, t);
            _currentWeapon.transform.localPosition = Vector3.Lerp(_currentWeapon.transform.localPosition, targetPosition, t);
            _currentWeapon.transform.localRotation = Quaternion.Lerp(_currentWeapon.transform.localRotation, targetRotation, t);

            await UniTask.Yield();
        }

        _isAimingTransition = false;

    }

    private void OnFirePerformed()
    {
        if (canFire && _currentWeapon.Fire(_hitPointer.transform.position))
        {
            _cameraRecoil?.RecoilFire(_currentWeapon.recoil, _currentWeapon.recoilForce, _currentWeapon.recoildSpeed);
            _weaponMovement?.ApplyRecoil(_currentWeapon.recoilForce, _currentWeapon.recoildSpeed, _currentWeapon.recoilDuration);
            
            OnFire?.Invoke();
        }
    }
    
    private void OnReloadPerformed()
    {
        int availableAmmoCount = GetAvailableAmmoCount();
        int totalToReloadCount = _currentWeapon.clipSize - _currentWeapon.bulletsInClip;
        int totalToReload = Mathf.Clamp(availableAmmoCount, 0, totalToReloadCount);

        if (canReload && totalToReload > 0)
            OnReload?.Invoke(totalToReload);
    }

    private int GetAvailableAmmoCount()
    {
        return _inventoryController.GetItemCountInInventory(_currentWeapon.bulletItemSO);
    }

    public bool TakeAmmo(int count)
    {
        if (GetAvailableAmmoCount() - count >= 0)
            return _inventoryController.RemoveInventoryItem(_currentWeapon.bulletItemSO, count);

        return false;
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
}
