
using UnityEngine;

public interface IInteractable
{
    public Transform transform { get; }
    
    public string GetInteractionPrompt();
    public bool Interact(InteractController interactController);
}
