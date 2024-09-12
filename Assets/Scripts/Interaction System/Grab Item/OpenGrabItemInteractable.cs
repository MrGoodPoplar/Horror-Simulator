using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class OpenGrabItemInteractable : GrabItemInteractable
{
    [field: Header("Constraints")]
    [field: SerializeField] public InteractableVisualSO openInteractableVisualSO { get; private set; }
    
    public bool opened { get; private set; }
    
    public event Action OnOpen;
    public event Action OnClose;

    private float _defaultHoldDuration;
    private InteractableVisualSO _defaultInteractableVisualSO;

    protected override void Awake()
    {
        _defaultHoldDuration = holdDuration;
        _defaultInteractableVisualSO = interactableVisualSO;

        interactableVisualSO = openInteractableVisualSO;
    }

    public override InteractionResponse Interact()
    {
        if (opened)
            return base.Interact();

        ChangeState();
            
        return new(null, true, false, true);
    }

    private void ChangeState()
    {
        if (opened)
        {
            OnClose?.Invoke();
            holdDuration = _defaultHoldDuration;
            interactableVisualSO = openInteractableVisualSO;
        }
        else
        {
            OnOpen?.Invoke();
            holdDuration = 0;
            interactableVisualSO = _defaultInteractableVisualSO;
        }
        
        opened = !opened;
    }
}