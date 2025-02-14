using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IHoldable
{
    public Transform transform { get; }
    
    public void Take() {}

    public void Hide() {}

    public async UniTask TakeAsync()
    {
        Take();
        await UniTask.CompletedTask;
    }

    public async UniTask HideAsync()
    {
        Hide();
        await UniTask.CompletedTask;
    }
}