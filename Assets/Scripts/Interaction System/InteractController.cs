using System;
using UnityEngine;

public class InteractController : MonoBehaviour
{
    public event EventHandler<InteractEventArgs> OnInteract;
    public event EventHandler<InteractEventArgs> OnInteractHover;
    public event EventHandler<InteractEventArgs> OnInteractUnhover;

    public class InteractEventArgs : EventArgs
    {
        public IInteractable interactable { get; private set; }
        
        public InteractEventArgs(IInteractable interactable)
        {
            this.interactable = interactable;
        }
    }

    public bool isInteractableInRange { get; private set; }
    
    [Header("Settings")]
    [SerializeField, Range(0, 5)] private float _interactionRadius = 0.25f;
    [SerializeField, Range(0, 5)] private float _interactDistance = 2.0f;
    [SerializeField] private LayerMask _interactableMask;

    [Header("Constraints")]
    [SerializeField] private HitPointer _hitPointer;

    private readonly Collider[] _colliders = new Collider[3];
    private IInteractable _interactable;
    private PlayerInput _playerInput;

    private void Start()
    {
        _playerInput = Player.instance.playerInput;
        
        _playerInput.OnInteract += OnInteractPerformed;
    }

    private void OnDestroy()
    {
        _playerInput.OnInteract -= OnInteractPerformed;
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
            }
        }
        else if (_interactable != null)
        {
            OnInteractUnhover?.Invoke(null, new InteractEventArgs(_interactable));
            _interactable = null;
        }
    }
    
    private void OnInteractPerformed()
    {
        if (_interactable != null)
        {
            _interactable.Interact(this);
            OnInteract?.Invoke(null, new InteractEventArgs(_interactable));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.75f);
        Gizmos.DrawWireSphere(_hitPointer.transform.position, _interactionRadius);
    }
}
