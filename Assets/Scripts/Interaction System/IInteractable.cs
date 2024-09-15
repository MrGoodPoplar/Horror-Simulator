
using UnityEngine;

public interface IInteractable
{
    public Transform transform { get; }
    public InteractableVisualSO interactableVisualSO { get; }
    public float holdDuration { get; }
    
    public bool instant => holdDuration <= 0;
    
    public InteractionResponse Interact();

    public void Forget();
    
    public Vector3 GetAnchorPosition() => transform.position;

    public string GetInteractableName() => interactableVisualSO.interactLabelText;
}
