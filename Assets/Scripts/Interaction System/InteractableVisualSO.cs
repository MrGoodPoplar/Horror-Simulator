using UnityEngine;

[CreateAssetMenu(menuName = "UI/Interactable Visual")]
public class InteractableVisualSO : ScriptableObject
{
    [field: SerializeField] public string text { get; private set; }
    [field: SerializeField] public Sprite sprite { get; private set; }
    [field: SerializeField] public VisualType visualType { get; private set; }
    
    public enum VisualType
    {
        Text,
        Icon
    }
}
