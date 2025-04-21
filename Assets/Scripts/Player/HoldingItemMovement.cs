using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class HoldingItemMovement : MonoBehaviour
{
    [Header("Overlap Settings")]
    [SerializeField] private OverlapPointer _overlapPointer;
    [SerializeField] private float _overlapMoveSpeed = 5f;
    [SerializeField, Range(0, 1)] private float _overlapMaxPushBack = 0.15f;
    
    [Header("Sway Settings")]
    [SerializeField, Range(0, 1)] private float _swayAmount = 0.5f;
    [SerializeField] private float _swaySpeed = 1f;
    [SerializeField, Range(0, 1)] private float _aimSwayAmount = 0.1f;
    [SerializeField] private float _aimSwaySpeed = 5f;

    [Header("Bobbing Settings")]
    [SerializeField, Range(0, 1)] private float _idleBobAmount;
    [SerializeField] private float _idleBobSpeed;
    [SerializeField, Range(0, 1)] private float _bobAmount;
    [SerializeField] private float _bobSpeed;
    [SerializeField, Range(0, 1)] private float _aimBobAmount;
    [SerializeField] private float _aimBobSpeed;

    [Header("Jump Sway Settings")]
    [SerializeField, Range(0, 1)] private float _jumpSwayAmount = 0.3f;
    [SerializeField] private float _jumpSwaySpeed = 3f;
    [SerializeField] private float _jumpSwayHorizontal = 0.1f;
    [SerializeField, Range(0, 1)] private float _aimJumpReducer = 0.5f;

    [Header("Recoil Settings")]
    [SerializeField, Range(0, 1)] private float _aimRecoilReducer = 0.7f;

    [Header("Fade Settings")]
    [SerializeField] private Vector3 _hidePosition;
    [SerializeField] private float _fadeDuration = 0.2f;

    private Quaternion _initialRotation;
    private Vector3 _initialPosition;
    private Vector3 _idlePosition;
    private PlayerInput _playerInput;
    private Collider[] _collisions = new Collider[4];

    private HoldingItemController _holdingItemController;
    private IMoveable _moveable;
    private ShooterController _shooterController;
    
    private float _timer;
    private float _recoilTimer;
    private float _recoilSpeed;
    private float _recoilDuration;
    private float _recoilForce;
    private bool _isAirborne;
    private bool _isFade;

    private void Start()
    {
        _shooterController = Player.Instance.shooterController;
        _moveable = Player.Instance.firstPersonController;
        _holdingItemController = Player.Instance.holdingItemController;
        
        _holdingItemController.OnTake += HoldingItemOnTakePerformed;
        _holdingItemController.OnHideBefore += HoldingItemOnHideBeforePerformed;
        
        _initialPosition = transform.localPosition;
        _idlePosition = Player.Instance.holdingItemController.currentHoldable.IsUnityNull()
            ? _hidePosition
            : _idlePosition;
        
        _initialRotation = transform.localRotation;
        _playerInput = Player.Instance.playerInput;
    }

    private void OnDestroy()
    {
        _holdingItemController.OnTake -= HoldingItemOnTakePerformed;
        _holdingItemController.OnHideBefore -= HoldingItemOnHideBeforePerformed;
    }

    private void Update()
    {
        if (_isFade)
            return;
        
        HandleSway();

        if (_moveable.isGrounded)
            HandleBobbing();
        
        HandleRecoil();
        HandleJumpSway();
        HandleOverlap();
    }

    private async void HoldingItemOnTakePerformed(HoldableItem holdable)
    {
        _idlePosition = _initialPosition;
        await FadeAsync();
    }
    
    private async UniTask HoldingItemOnHideBeforePerformed(HoldableItem holdable)
    {
        _idlePosition = _hidePosition;
        await FadeAsync();
    }

    private async UniTask FadeAsync()
    {
        _isFade = true;
        float timer = 0f;
        Vector3 startPos = transform.localPosition;

        while (timer < _fadeDuration)
        {
            float t = timer / _fadeDuration;
            transform.localPosition = Vector3.Lerp(startPos, _idlePosition, t);

            timer += Time.deltaTime;
            await UniTask.Yield();
        }

        transform.localPosition = _idlePosition;
        _isFade = false;
    }

    
    private void HandleOverlap()
    {
        if (!_holdingItemController.currentHoldable)
            return;

        if (_holdingItemController.currentHoldable.IsColliding(_overlapPointer.layer))
            _idlePosition.z = Mathf.Max(_idlePosition.z - _overlapMoveSpeed * Time.deltaTime, _initialPosition.z - _overlapMaxPushBack);
        else if (!IsColliding(_holdingItemController.currentHoldable))
            _idlePosition = Vector3.MoveTowards(_idlePosition, _initialPosition, _overlapMoveSpeed * Time.deltaTime);
    }
    
    private bool IsColliding(HoldableItem holdableItem)
    {
        if (!transform.parent)
            return false;
        
        Vector3 localDefaultPosition = _initialPosition + _holdingItemController.currentHoldable.transform.localPosition + _holdingItemController.currentHoldable.bounds.center;
        Vector3 boxCenter = transform.parent.TransformPoint( localDefaultPosition);
        Vector3 boxSize = holdableItem.bounds.extents * 2;
        Quaternion rotation = holdableItem.transform.rotation;

        int hits = Physics.OverlapBoxNonAlloc(boxCenter, boxSize / 2, _collisions, rotation, _overlapPointer.layer);
        return hits > 0 && _collisions[0].gameObject != gameObject;
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
        if (_playerInput.move.magnitude > 0)
        {
            float bobAmount = _shooterController.isAiming ? _aimBobAmount : _bobAmount;
            float bobSpeed = _shooterController.isAiming ? _aimBobSpeed : _bobSpeed;
            
            Bob(bobAmount, bobSpeed);
        }
        else if (!_shooterController.isAiming)
        {
            float bobSpeed = Vector3.Distance(transform.localPosition, _idlePosition) < _idleBobAmount ? _idleBobSpeed : _bobSpeed;
            
            Bob(_idleBobAmount, bobSpeed);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _idlePosition, Time.deltaTime * _bobSpeed);
        }
    }

    private void Bob(float amount, float speed)
    {
        _timer += Time.deltaTime * speed;
        float bobOffsetY = Mathf.Sin(_timer) * amount;
        Vector3 bobOffset = new Vector3(0, bobOffsetY, 0);
        
        transform.localPosition = Vector3.Lerp(transform.localPosition, _idlePosition + bobOffset, Time.deltaTime * speed);
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
        if (!_moveable.isGrounded)
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
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_holdingItemController?.currentHoldable)
        {
            Vector3 localDefaultPosition = _initialPosition + _holdingItemController.currentHoldable.transform.localPosition + _holdingItemController.currentHoldable.bounds.center;
            Vector3 boxCenter = transform.parent.TransformPoint( localDefaultPosition);
            Vector3 boxSize = _holdingItemController.currentHoldable.bounds.size; 
        
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(boxCenter, _holdingItemController.currentHoldable.transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
#endif
}
