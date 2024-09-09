using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(FirstPersonController))]
public class ShooterController : MonoBehaviour
{
    public event Action OnFire;
    public event Action OnReload;
    
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
    [SerializeField] private Weapon _currentWeapon;
    
    private void Awake()
    {
        _firstPersonController = GetComponent<FirstPersonController>();
        _playerInput = _firstPersonController.playerInput;
        
        _playerInput.OnFire += OnFirePerformed;
        _playerInput.OnReload += OnReloadPerfomed;

        _defaultFOV = _firstPersonController.playerCamera.fieldOfView;
        _defaultSensitivity = _firstPersonController.sensitivity;
    }

    private void OnDestroy()
    {
        _playerInput.OnFire -= OnFirePerformed;
        _playerInput.OnReload -= OnReloadPerfomed;
    }

    private void Update()
    {
        HandleAiming();
    }

    private void OnAiming()
    {
        _currentWeapon.transform.localPosition = isAiming ? _currentWeapon.aimPosition : Vector3.zero;
        _currentWeapon.transform.localRotation = isAiming ? _currentWeapon.aimRotation : Quaternion.identity;
    }
    
    private async void HandleAiming()
    {
        bool aimStateChanged = isAiming != _playerInput.isAiming || isAiming && !canAim;

        if (_isAimingTransition || !aimStateChanged)
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

            await Task.Yield();
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
    
    private void OnReloadPerfomed()
    {
        if (canReload && _currentWeapon.bulletsInClip != _currentWeapon.clipSize)
            OnReload?.Invoke();
    }
}
