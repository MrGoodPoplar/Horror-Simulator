using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IHoldable
{
    public Transform transform { get; }
    
    public void Take() {}

    public void Hide() {}

    public async UniTask TakeAsync() => Take();

    public async UniTask HideAsync() => Hide();
}