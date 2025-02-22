using Cysharp.Threading.Tasks;
using UnityEngine;

public class HoldableItem : MonoBehaviour
{
    [field: Header("Holding Settings")]
    [field: SerializeField] public Vector3 holdingPosition { get; private set; }
    [field: SerializeField] public Vector3 holdingRotation { get; private set; }
    
    public virtual void Take() {}

    public virtual void Hide() {}

    public virtual async UniTask TakeAsync()
    {
        Take();
        await UniTask.CompletedTask;
    }

    public virtual async UniTask HideAsync()
    {
        Hide();
        await UniTask.CompletedTask;
    }
}
