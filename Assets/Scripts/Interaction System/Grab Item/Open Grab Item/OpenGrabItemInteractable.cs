using System;
using Audio_System;

namespace Interaction_System.Grab_Item.Open_Grab_Item
{
    public class OpenGrabItemInteractable : GrabItemInteractable<OpenGrabInteractableConstraints, OpenGrabInteractableSounds>
    {
        public bool opened { get; private set; }
    
        public event Action OnOpen;
        public event Action OnClose;

        private float _defaultHoldDuration;

        protected override void Awake()
        {
            _defaultHoldDuration = holdDuration;
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
            }
            else // TODO: unreacheable state
            {
                OnOpen?.Invoke();
                holdDuration = 0;
            }
        
            SoundManager.Instance.CreateSound()
                .WithSoundData(_sounds.changeStateSoundSO.soundData)
                .WithRandomPitch()
                .WithPosition(transform.position)
                .Play();
        
            opened = !opened;
        }

        public override string GetInteractableName()
        {
            if (opened)
                return _constraints.inventoryItemSO.itemName;

            return base.GetInteractableName();
        }

        public override InteractableVisualSO GetInteractableVisualSO()
        {
            return opened ? _constraints.interactableVisualSO : _constraints.openInteractableVisualSO;
        }
    }
}