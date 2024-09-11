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

    [Header("Jump Sway Settings")]
    [SerializeField, Range(0, 1)] private float _jumpSwayAmount = 0.3f;
    [SerializeField] private float _jumpSwaySpeed = 3f;
    [SerializeField] private float _jumpSwayHorizontal = 0.1f;
    [SerializeField, Range(0, 1)] private float _aimJumpReducer = 0.5f;

    [Header("Recoil Settings")]
    [SerializeField, Range(0, 1)] private float _aimRecoilReducer = 0.7f;

    private Quaternion _initialRotation;
    private Vector3 _initialPosition;
    private PlayerInput _playerInput;
    
    private FirstPersonController _firstPersonController;
    private ShooterController _shooterController;
    
    private float _timer;
    private float _recoilTimer;
    private float _recoilSpeed = 0;
    private float _recoilDuration = 0;
    private float _recoilForce = 0;
    private bool _isAirborne = false;

    private void Start()
    {
        _shooterController = Player.instance.shooterController;
        _firstPersonController = Player.instance.firstPersonController;
        
        _initialPosition = transform.localPosition;
        _initialRotation = transform.localRotation;
        _playerInput = _firstPersonController.playerInput;
    }

    private void Update()
    {
        HandleSway();

        if (_firstPersonController.isGrounded)
            HandleBobbing();
        
        HandleRecoil();
        HandleJumpSway();
    }

    private void HandleSway()
    {
        Vector2 lookInput = _playerInput.look;
        float swayAmount = GetSwayAmount();
        float swaySpeed = _shooterController.isAiming ? _aimSwaySpeed : _swaySpeed;

        float swayX = lookInput.x * swayAmount;
        float swayY = lookInput.y * swayAmount;

        Quaternion swayRotation = Quaternion.Euler(-swayY, swayX, 0);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, _initialRotation * swayRotation, Time.deltaTime * swaySpeed);
    }

    private float GetSwayAmount()
    {
        if (!Player.instance.isHUDView)
            return _shooterController.isAiming ? _aimSwayAmount : _swayAmount;

        return 0;
    }

    private void HandleBobbing()
    {
        float bobAmount = _shooterController.isAiming ? _aimBobAmount : _bobAmount;
        float bobSpeed = _shooterController.isAiming ? _aimBobSpeed : _bobSpeed;

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
            float recoilForce = _shooterController.isAiming ? _recoilForce * _aimRecoilReducer : _recoilForce;
            Vector3 targetPosition = _initialPosition + Vector3.back * recoilForce;

            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * _recoilSpeed);
        }
    }

    private void HandleJumpSway()
    {
        if (!_firstPersonController.isGrounded)
        {
            if (!_isAirborne)
            {
                _isAirborne = true;
                _timer = 0;
            }

            float aimReducer = _shooterController.isAiming ? _aimJumpReducer : 1f;
            float jumpSwayAmount = _jumpSwayAmount * aimReducer;
            float jumpSwaySpeed = _jumpSwaySpeed * aimReducer;
            float jumpSwayHorizontal = _jumpSwayHorizontal * aimReducer;

            _timer += Time.deltaTime * jumpSwaySpeed;

            float jumpSwayY = Mathf.Sin(_timer) * jumpSwayAmount;
            float jumpSwayX = Mathf.Cos(_timer) * jumpSwayHorizontal;

            Vector3 jumpSwayOffset = new Vector3(jumpSwayX, jumpSwayY, 0);
            transform.localPosition = Vector3.Lerp(transform.localPosition, _initialPosition + jumpSwayOffset, Time.deltaTime * jumpSwaySpeed);
        }
        else if (_isAirborne)
        {
            _isAirborne = false;
            transform.localPosition = Vector3.Lerp(transform.localPosition, _initialPosition, Time.deltaTime * _jumpSwaySpeed);
        }
    }

    public void ApplyRecoil(float recoilForce, float recoilSpeed, float recoilDuration)
    {
        _recoilSpeed = recoilSpeed;
        _recoilDuration = recoilDuration;
        _recoilForce = recoilForce;
        _recoilTimer = _recoilDuration;
    }
}
