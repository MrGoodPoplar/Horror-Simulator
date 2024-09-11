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

    public event EventHandler<InteractEventArgs> OnInteract;
    public event EventHandler<InteractEventArgs> OnInteractHover;
    public event EventHandler<InteractEventArgs> OnInteractUnhover;
    
    #region InteractEventArgs Class
    public class InteractEventArgs : EventArgs
    {
        public IInteractable interactable { get; private set; }
        
        public InteractEventArgs(IInteractable interactable)
        {
            this.interactable = interactable;
        }
    }
    #endregion

    public bool isInteractableInRange { get; private set; }
    public float holdingProgress { get; private set; }
    
    private readonly Collider[] _colliders = new Collider[3];
    private IInteractable _interactable;
    private PlayerInput _playerInput;
    
    private Coroutine _holdCoroutine;
    private bool _isHolding;

    private void Start()
    {
        _playerInput = Player.instance.playerInput;
        
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
        if (Vector3.Distance(transform.position, _hitPointer.transform.position) > _interactDistance)
        {
            if (isInteractableInRange)
            {
                OnInteractUnhover?.Invoke(null, new InteractEventArgs(_interactable));
                _interactable = null;
            }

            return;
        }
        
        isInteractableInRange = Physics.OverlapSphereNonAlloc(_hitPointer.transform.position, _interactionRadius, _colliders,_interactableMask) > 0;

        if (isInteractableInRange && _colliders[0].TryGetComponent(out IInteractable interactable))
        {
            if (_interactable != interactable)
            {
                _interactable = interactable;
                OnInteractHover?.Invoke(null, new InteractEventArgs(interactable));
                OnInteractCanceled();
            }
        }
        else if (_interactable != null)
        {
            OnInteractUnhover?.Invoke(null, new InteractEventArgs(_interactable));
            OnInteractCanceled();
            _interactable = null;
        }
    }
    
    private void OnInteractPerformed()
    {
        if (_interactable != null && !Player.instance.isHUDView)
        {
            _isHolding = true;

            if (_holdCoroutine != null)
                StopCoroutine(_holdCoroutine);

            _holdCoroutine = StartCoroutine(HoldInteractionCoroutine(_interactable.holdDuration));
        }
    }

    private void OnInteractCanceled()
    {
        _isHolding = false;

        if (_holdCoroutine != null)
        {
            StopCoroutine(_holdCoroutine);
            _holdCoroutine = null;
        }
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

        if (holdTime >= duration)
        {
            InteractionResponse response = _interactable.Interact(this);
            if (!response.message.IsUnityNull())
                Debug.Log($"{response.message} -> {response.result}");
            
            OnInteract?.Invoke(null, new InteractEventArgs(_interactable));
        }

        _holdCoroutine = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.75f);
        Gizmos.DrawWireSphere(_hitPointer.transform.position, _interactionRadius);
    }
}
