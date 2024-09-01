using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput), typeof(FirstPersonController))]
public class ShooterController : MonoBehaviour
{
    public bool canAim { get; set; } = true;
    
    [Header("Aim Settings")]
    [SerializeField, Range(0, 10)] private float _aimSensitivityReducer = 2f;
    [SerializeField, Range(10, 150)] private float _aimFOV = 30f;
    [SerializeField] private float _timeToAim = 0.1f;

    [Header("Constraints")]
    [SerializeField] private Transform _target;

    private bool _isAimingTransition;
    private bool _isAiming;
    private float _defaultFOV;
    private float _defaultSensitivity;
    private PlayerInput _playerInput;
    private FirstPersonController _firstPersonController;
    [SerializeField] private Weapon _currentWeapon;
    
    private void Awake()
    {
        _firstPersonController = GetComponent<FirstPersonController>();
        _playerInput = GetComponent<PlayerInput>();
        
        _playerInput.OnFire += OnFire;

        _defaultFOV = _firstPersonController.cameraFOV;
        _defaultSensitivity = _firstPersonController.sensitivity;
    }

    private void OnDestroy()
    {
        _playerInput.OnFire -= OnFire;
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
        _currentWeapon.transform.localPosition = _isAiming ? _currentWeapon.aimPosition : Vector3.zero;
        _currentWeapon.transform.localRotation = _isAiming ? _currentWeapon.aimRotation : Quaternion.identity;
    }
    
    private async void HandleAiming()
    {
        bool aimStateChanged = _isAiming != _playerInput.isAiming;

        if (_isAimingTransition || !aimStateChanged)
            return;
        
        float elapsedTime = 0f;
        float targetFov = _playerInput.isAiming ? _aimFOV : _defaultFOV;
        float targetSensitivity = _playerInput.isAiming ? _defaultSensitivity / _aimSensitivityReducer : _defaultSensitivity;
        Vector3 targetPosition = _playerInput.isAiming ? _currentWeapon.aimPosition : Vector3.zero;
        Quaternion targetRotation = _playerInput.isAiming ? _currentWeapon.aimRotation : Quaternion.identity;
        
        _isAimingTransition = true;
        _isAiming = !_isAiming;
        
        while (elapsedTime < _timeToAim)
        {
            float t = elapsedTime / _timeToAim;
            elapsedTime += Time.deltaTime;

            _firstPersonController.cameraFOV = Mathf.Lerp(_firstPersonController.cameraFOV, targetFov, t);
            _firstPersonController.sensitivity = Mathf.Lerp(_firstPersonController.sensitivity, targetSensitivity, t);
            _currentWeapon.transform.localPosition = Vector3.Lerp(_currentWeapon.transform.localPosition, targetPosition, t);
            _currentWeapon.transform.localRotation = Quaternion.Lerp(_currentWeapon.transform.localRotation, targetRotation, t);

            await Task.Yield();
        }

        _isAimingTransition = false;

    }

    private void OnFire()
    {
        _currentWeapon.Fire(_target.position);
    }
}
