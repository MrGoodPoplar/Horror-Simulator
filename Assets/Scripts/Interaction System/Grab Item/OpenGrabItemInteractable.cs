using System;
using Audio_System;
using Cysharp.Threading.Tasks;
using Prop;
using UnityEngine;

public class OpenGrabItemInteractable : GrabItemInteractable
{
    // TODO: refactor on SoundData example
    [field: Header("Constraints")]
    [field: SerializeField] public InteractableVisualSO openInteractableVisualSO { get; private set; }
    
    [Header("Sounds")]
    [SerializeField] private SoundSO _changeStateSoundSO;
    
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
        else // TODO: unreacheable state
        {
            OnOpen?.Invoke();
            holdDuration = 0;
            interactableVisualSO = _defaultInteractableVisualSO;
        }
        
        SoundManager.Instance.CreateSound()
            .WithSoundData(_changeStateSoundSO.sounds)
            .WithRandomPitch()
            .WithPosition(transform.position)
            .Play();
        
        opened = !opened;
    }

    public override string GetInteractableName()
    {
        if (opened)
            return inventoryItemSO.itemName;

        return base.GetInteractableName();
    }
}