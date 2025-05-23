using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class InteractController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Range(0, 5)] private float _interactionRadius = 0.25f;
    [SerializeField, Range(0, 5)] private float _interactDistance = 3.0f;
    [SerializeField] private LayerMask _interactableMask;

    [Header("Constraints")]
    [SerializeField] private HitPointer _hitPointer;

    public event Action<IInteractable, InteractionResponse> OnInteract;
    public event Action<IInteractable, InteractionResponse> OnInteractCancelled;
    public event Action<IInteractable, InteractionResponse> OnInteractHover;
    public event Action<IInteractable, InteractionResponse> OnInteractUnhover;

    public bool isInteractableInRange { get; private set; }
    public float holdingProgress { get; private set; }
    
    private readonly Collider[] _colliders = new Collider[5];
    private IInteractable _interactable;
    private PlayerInput _playerInput;
    
    private Coroutine _holdCoroutine;
    private bool _isHolding;

    private void Start()
    {
        _playerInput = Player.Instance.playerInput;
        
        _playerInput.OnInteract += OnInteractPerformed;
        _playerInput.OnInteractCanceled += OnInteractCanceled;
    }

    private void OnDestroy()
    {
        _playerInput.OnInteract -= OnInteractPerformed;
        _playerInput.OnInteractCanceled -= OnInteractCanceled;
    }

    private void Update()
    {
        if (!Player.Instance.HUDController.isHUDView)
            FindInteractable();
    }

    private void FindInteractable()
    {
        if (Vector3.Distance(transform.position, _hitPointer.transform.position) > _interactDistance)
        {
            if (!_interactable.IsUnityNull())
            {
                OnInteractUnhover?.Invoke(_interactable, new InteractionResponse());
                _interactable.Forget();
                _interactable = null;
            }

            return;
        }

        if (TryGetClosestInteractable(out IInteractable closestInteractable))
        {
            if (_interactable != closestInteractable)
            {
                _interactable?.Forget();
                _interactable = closestInteractable;
                OnInteractHover?.Invoke(_interactable, new InteractionResponse());
                OnInteractCanceled();
            }
        }
        else if (_interactable != null)
        {
            OnInteractUnhover?.Invoke(_interactable, new InteractionResponse());
            OnInteractCanceled();
            _interactable.Forget();
            _interactable = null;
        }
    }
    
    private bool TryGetClosestInteractable(out IInteractable closestInteractable)
    {
        int colliderCount = Physics.OverlapSphereNonAlloc(_hitPointer.transform.position, _interactionRadius, _colliders, _interactableMask);
        float closestDistance = Mathf.Infinity;
        closestInteractable = null;
    
        isInteractableInRange = colliderCount > 0;

        if (!isInteractableInRange)
            return false;

        for (int i = 0; i < colliderCount; i++)
        {
            if (_colliders[i].TryGetComponent(out IInteractable interactable))
            {
                float distance = Vector3.Distance(_hitPointer.transform.position, _colliders[i].transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        return closestInteractable != null;
    }
    
    private void OnInteractPerformed()
    {
        if (_interactable != null && !Player.Instance.HUDController.isHUDView)
        {
            _isHolding = true;

            if (_holdCoroutine != null)
                StopCoroutine(_holdCoroutine);

            if (_interactable.instant)
                Interact(_interactable);
            else
                _holdCoroutine = StartCoroutine(HoldInteractionCoroutine(_interactable.holdDuration));
        }
    }

    private void OnInteractCanceled()
    {
        _isHolding = false;
        holdingProgress = 0f;
        
        if (_holdCoroutine != null)
        {
            StopCoroutine(_holdCoroutine);
            _holdCoroutine = null;
        }
        
        OnInteractCancelled?.Invoke(_interactable, new InteractionResponse());
    }

    private IEnumerator HoldInteractionCoroutine(float duration)
    {
        float holdTime = 0f;

        while (_isHolding && holdTime < duration)
        {
            holdTime += Time.deltaTime;
            holdingProgress = Mathf.Clamp01(holdTime / duration);
            yield return null;
        }

        if (_interactable != null && holdTime >= duration)
            Interact(_interactable);

        _holdCoroutine = null;
    }

    private void Interact(IInteractable interactable)
    {
        InteractionResponse response = interactable.Interact();
        if (!response.message.IsUnityNull())
            Debug.Log($"{response.message} -> {response.result}");
            
        OnInteract?.Invoke(interactable, response);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.75f);
        Gizmos.DrawWireSphere(_hitPointer.transform.position, _interactionRadius);
    }
}
