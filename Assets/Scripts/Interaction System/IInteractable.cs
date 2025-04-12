
using UnityEngine;

public interface IInteractable
{
    public Transform transform { get; }
    public float holdDuration { get; }
    public SpriteAlignment spriteAlignment { get; }
    
    public bool instant => holdDuration <= 0;
    
    public InteractionResponse Interact();

    public void Forget();

    public InteractableVisualSO GetInteractableVisualSO();
    
    public Vector3 GetAnchorPosition() => transform.position;

    public string GetInteractableName() => GetInteractableVisualSO().interactLabelText;
}
