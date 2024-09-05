using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    public Vector3 position => transform.position;
    
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