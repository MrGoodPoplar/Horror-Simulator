
using UnityEngine;

public interface IInteractable
{
    public Transform transform { get; }
    public InteractableVisualSO InteractableVisualSO { get; }
    public float holdDuration { get; }
    
    public InteractionResponse Interact(InteractController interactController);
    
    public Vector3 GetAnchorPosition() => transform.position;
}
