using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    [field: SerializeField] public float holdDuration { get; private set; }
    [field: SerializeField] public InteractableVisualSO InteractableVisualSO { get; private set; }
    [SerializeField] private string _pickupText;

    public InteractionResponse Interact(InteractController interactController)
    {
        return new($"IM {_pickupText}!", true);
    }
}