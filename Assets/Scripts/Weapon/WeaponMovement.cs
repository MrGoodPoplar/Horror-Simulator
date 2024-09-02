using UnityEngine;

public class WeaponMovement : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField, Range(0, 1)] private float _swayAmount = 0.5f;
    [SerializeField] private float _swaySpeed = 1f;
    [SerializeField, Range(0, 1)] private float _aimSwayAmount = 0.1f;
    [SerializeField] private float _aimSwaySpeed = 5f;

    [Header("Bobbing Settings")]
    [SerializeField, Range(0, 1)] private float _bobAmount = 0.05f;
    [SerializeField] private float _bobSpeed = 1f;
    [SerializeField, Range(0, 1)] private float _aimBobAmount = 0.02f;
    [SerializeField] private float _aimBobSpeed = 0.5f;

    private Quaternion _initialRotation;
    private Vector3 _initialPosition;
    private PlayerInput _playerInput;
    private FirstPersonController _firstPersonController;
    
    private float _timer;
    private float _recoilTimer;
    private float _recoilSpeed = 0;
    private float _recoilDuration = 0;
    private Vector3 _recoilOffset;

    private void Start()
    {
        _initialPosition = transform.localPosition;
        _initialRotation = transform.localRotation;
        
        _firstPersonController = FirstPersonController.instance;
        _playerInput = _firstPersonController.playerInput;
    }

    private void Update()
    {
        HandleSway();
        HandleBobbing();
        HandleRecoil();
    }

    private void HandleSway()
    {
        Vector2 lookInput = _playerInput.look;
        float swayAmount = ShooterController.instance.isAiming ? _aimSwayAmount : _swayAmount;
        float swaySpeed = ShooterController.instance.isAiming ? _aimSwaySpeed : _swaySpeed;
        
        float swayX = lookInput.x * swayAmount;
        float swayY = lookInput.y * swayAmount;

        Quaternion swayRotation = Quaternion.Euler(-swayY, swayX, 0);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, _initialRotation * swayRotation, Time.deltaTime * swaySpeed);
    }

    private void HandleBobbing()
    {
        float bobAmount = ShooterController.instance.isAiming ? _aimBobAmount : _bobAmount;
        float bobSpeed = ShooterController.instance.isAiming ? _aimBobSpeed : _bobSpeed;

        if (_playerInput.move.magnitude > 0)
        {
            float currentSpeed = bobSpeed * _firstPersonController.velocity;
            _timer += Time.deltaTime * currentSpeed;
            float bobOffsetY = Mathf.Sin(_timer) * bobAmount;
            Vector3 bobOffset = new Vector3(0, bobOffsetY, 0);
            transform.localPosition = Vector3.Lerp(transform.localPosition, _initialPosition + bobOffset, Time.deltaTime * currentSpeed);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _initialPosition, Time.deltaTime * bobSpeed);
        }
    }

    private void HandleRecoil()
    {
        if (_recoilTimer > 0)
        {
            _recoilTimer -= Time.deltaTime;
            Vector3 targetPosition = _initialPosition + _recoilOffset;
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * _recoilSpeed);
            
            if (_recoilTimer <= 0)
                _recoilOffset = Vector3.zero;
        }
    }

    public void ApplyRecoil(Vector3 recoilDirection, float recoilSpeed, float recoilDuration)
    {
        _recoilSpeed = recoilSpeed;
        _recoilDuration = recoilDuration;
        _recoilOffset = recoilDirection;
        _recoilTimer = _recoilDuration;
    }
}