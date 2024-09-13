using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(menuName = "UI/Interactable Visual")]
public class InteractableVisualSO : ScriptableObject
{
    [SerializeField] private LocalizedString _interactLabelText;
    [field: SerializeField] public float interactScaleEffect { get; private set; } = 0.9f;
    [field: SerializeField] public float interactDurationEffect { get; private set; } = 0.1f;
    [field: SerializeField] public Sprite sprite { get; private set; }
    [field: SerializeField] public VisualType visualType { get; private set; }
    
    public enum VisualType
    {
        Text,
        Icon
    }

    public bool interactEffectEnabled => interactDurationEffect > 0;
    public string interactLabelText => _interactLabelText?.GetLocalizedString();
}
