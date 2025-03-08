using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class HoldingItemMovement : MonoBehaviour
{
    [Header("Overlap Settings")]
    [SerializeField] private HitPointer _hitPointer;

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

    [Header("Fade Settings")]
    [SerializeField] private Vector3 _hidePosition;
    [SerializeField] private float _itemIdlePositionThreshold = 0.1f;
    
    private Quaternion _initialRotation;
    private Vector3 _initialPosition;
    private Vector3 _idlePosition;
    private PlayerInput _playerInput;

    private HoldingItemController _holdingItemController;
    private FirstPersonController _firstPersonController;
    private ShooterController _shooterController;
    
    private float _timer;
    private float _recoilTimer;
    private float _recoilSpeed;
    private float _recoilDuration;
    private float _recoilForce;
    private bool _isAirborne;

    private void Start()
    {
        _shooterController = Player.Instance.shooterController;
        _firstPersonController = Player.Instance.firstPersonController;
        _holdingItemController = Player.Instance.holdingItemController;
        
        _holdingItemController.OnTake += HoldingItemOnTakePerformed;
        _holdingItemController.OnHideBefore += HoldingItemOnHideBeforePerformed;
        
        _initialPosition = transform.localPosition;
        _idlePosition = Player.Instance.holdingItemController.currentHoldable.IsUnityNull()
            ? _hidePosition
            : _idlePosition;
        
        _initialRotation = transform.localRotation;
        _playerInput = _firstPersonController.playerInput;
    }

    private void OnDestroy()
    {
        _holdingItemController.OnTake -= HoldingItemOnTakePerformed;
        _holdingItemController.OnHideBefore -= HoldingItemOnHideBeforePerformed;
    }

    private void Update()
    {
        HandleSway();

        if (_firstPersonController.isGrounded)
            HandleBobbing();
        
        HandleRecoil();
        HandleJumpSway();
        HandleOverlap();
    }

    private void HoldingItemOnTakePerformed(HoldableItem holdable)
    {
        _idlePosition = _initialPosition;
    }
    
    private async UniTask HoldingItemOnHideBeforePerformed(HoldableItem holdable)
    {
        _idlePosition = _hidePosition;
        await UniTask.WaitUntil(() => Vector3.Distance(transform.localPosition, _idlePosition) <= _itemIdlePositionThreshold);
    }

    private void HandleOverlap()
    {
        _holdingItemController.currentHoldable?.CheckCollisions(_hitPointer.layer);
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
        if (!Player.Instance.HUDController.isHUDView)
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
            transform.localPosition = Vector3.Lerp(transform.localPosition, _idlePosition + bobOffset, Time.deltaTime * currentSpeed);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _idlePosition, Time.deltaTime * bobSpeed);
        }
    }

    private void HandleRecoil()
    {
        if (_recoilTimer > 0)
        {
            _recoilTimer -= Time.deltaTime;
            float recoilForce = _shooterController.isAiming ? _recoilForce * _aimRecoilReducer : _recoilForce;
            Vector3 targetPosition = _idlePosition + Vector3.back * recoilForce;

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
            transform.localPosition = Vector3.Lerp(transform.localPosition, _idlePosition + jumpSwayOffset, Time.deltaTime * jumpSwaySpeed);
        }
        else if (_isAirborne)
        {
            _isAirborne = false;
            transform.localPosition = Vector3.Lerp(transform.localPosition, _idlePosition, Time.deltaTime * _jumpSwaySpeed);
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
