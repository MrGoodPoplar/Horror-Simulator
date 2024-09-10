
using UnityEngine;

public interface IInteractable
{
    public Transform transform { get; }
    public InteractableVisualSO InteractableVisualSO { get; }
    
    public bool Interact(InteractController interactController);
}
