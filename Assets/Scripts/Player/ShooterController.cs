using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(FirstPersonController))]
public class ShooterController : MonoBehaviour
{
    public static ShooterController instance { get; private set; }
    
    public bool canAim { get; set; } = true;
    public bool isAiming { get; private set; }
    
    [Header("Aim Settings")]
    [SerializeField, Range(0, 10)] private float _aimSensitivityReducer = 2f;
    [SerializeField, Range(10, 150)] private float _aimFOV = 30f;
    [SerializeField] private float _timeToAim = 0.1f;

    [Header("Constraints")]
    [SerializeField] private Transform _target;

    [FormerlySerializedAs("_recoil")] [SerializeField] private CameraRecoil _cameraRecoil;
    [FormerlySerializedAs("_weaponSway")] [SerializeField] private WeaponMovement _weaponMovement;

    private bool _isAimingTransition;
    private float _defaultFOV;
    private float _defaultSensitivity;
    private PlayerInput _playerInput;
    private FirstPersonController _firstPersonController;
    [SerializeField] private Weapon _currentWeapon;
    
    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        
        _firstPersonController = GetComponent<FirstPersonController>();
        _playerInput = _firstPersonController.playerInput;
        
        _playerInput.OnFire += OnFirePerformed;

        _defaultFOV = _firstPersonController.playerCamera.fieldOfView;
        _defaultSensitivity = _firstPersonController.sensitivity;
    }

    private void OnDestroy()
    {
        _playerInput.OnFire -= OnFirePerformed;
    }

    private void Update()
    {
        if (canAim)
        {
            HandleAiming();
        }
    }

    private void OnAiming()
    {
        _currentWeapon.transform.localPosition = isAiming ? _currentWeapon.aimPosition : Vector3.zero;
        _currentWeapon.transform.localRotation = isAiming ? _currentWeapon.aimRotation : Quaternion.identity;
    }
    
    private async void HandleAiming()
    {
        bool aimStateChanged = isAiming != _playerInput.isAiming;

        if (_isAimingTransition || !aimStateChanged)
            return;
        
        float elapsedTime = 0f;
        float targetFov = _playerInput.isAiming ? _aimFOV : _defaultFOV;
        float targetSensitivity = _playerInput.isAiming ? _defaultSensitivity / _aimSensitivityReducer : _defaultSensitivity;
        Vector3 targetPosition = _playerInput.isAiming ? _currentWeapon.aimPosition : Vector3.zero;
        Quaternion targetRotation = _playerInput.isAiming ? _currentWeapon.aimRotation : Quaternion.identity;
        
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
        if (_currentWeapon.Fire(_target.position))
        {
            _cameraRecoil?.RecoilFire(_currentWeapon.recoil);
            _weaponMovement?.ApplyRecoil(Vector3.back * _currentWeapon.recoilForce, _currentWeapon.recoildSpeed, _currentWeapon.recoilDuration);
        }
    }
}
