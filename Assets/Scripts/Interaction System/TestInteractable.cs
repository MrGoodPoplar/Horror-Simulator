using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    [field: SerializeField] public InteractableVisualSO InteractableVisualSO { get; private set; }
    [SerializeField] private string _pickupText;

    public bool Interact(InteractController interactController)
    {
        Debug.Log($"IM {_pickupText}!");
        return true;
    }
}