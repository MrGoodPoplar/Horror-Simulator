using System;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class HoldingItemController : MonoBehaviour
{
    public event Action<IHoldable> OnTake;
    public event Action<IHoldable> OnHide;
    
    public IHoldable currentHoldable { get; private set; }
    
    public async UniTask TakeAsync(IHoldable holdable)
    {
        if (!currentHoldable.IsUnityNull())
            await HideAsync();

        await holdable.TakeAsync();
        holdable.transform.gameObject.SetActive(true);
        holdable.transform.SetParent(transform, false);
        
        OnTake?.Invoke(holdable);
        currentHoldable = holdable;
    }
    
    public async UniTask HideAsync()
    {
        await currentHoldable.HideAsync();
        currentHoldable.transform.gameObject.SetActive(false);
        
        OnHide?.Invoke(currentHoldable);
        currentHoldable = null;
    }
}