using System;
using UnityEngine;

public class OpenGrabItemInteractable : GrabItemInteractable
{
    public bool opened { get; private set; }
    
    public event Action OnOpen;
    public event Action OnClose;

    private float _defaultHoldDuration;

    protected override void Awake()
    {
        _defaultHoldDuration = holdDuration;
    }

    public override InteractionResponse Interact(InteractController interactController)
    {
        if (opened)
            return base.Interact(interactController);

        ChangeState();
            
        return new(null, true);
    }

    private void ChangeState()
    {
        if (opened)
        {
            OnClose?.Invoke();
            holdDuration = _defaultHoldDuration;
        }
        else
        {
            OnOpen?.Invoke();
            holdDuration = 0;
        }
        
        opened = !opened;
    }
}