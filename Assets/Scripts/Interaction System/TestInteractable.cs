using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    public string label = "TEST";
    
    public string GetInteractionPrompt()
    {
        return label;
    }

    public bool Interact(InteractController interactController)
    {
        Debug.Log($"IM {label}!");
        return true;
    }
}