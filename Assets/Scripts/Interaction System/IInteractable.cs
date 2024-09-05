
using UnityEngine;

public interface IInteractable
{
    public Vector3 position { get; }
    
    public string GetInteractionPrompt();
    public bool Interact(InteractController interactController);
}
