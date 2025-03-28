using UnityEditor;
using UnityEngine;

public class TestInteractable : MonoBehaviour, IInteractable
{
    [field: SerializeField] public float holdDuration { get; private set; }
    [field: SerializeField] public SpriteAlignment spriteAlignment { get; private set; }
    [field: SerializeField] public InteractableVisualSO interactableVisualSO { get; private set; }
    [SerializeField] private string _pickupText;

    public InteractionResponse Interact()
    {
        return new($"IM {_pickupText}!", true);
    }

    public void Forget()
    {
        
    }
    
    public virtual string GetInteractableName() => _pickupText;

}