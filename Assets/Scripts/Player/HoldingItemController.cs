using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class HoldingItemController : MonoBehaviour
{
    public event Func<HoldableItem, UniTask> OnHideBefore;
    public event Action<HoldableItem> OnTake;
    public event Action<HoldableItem> OnHideAfter;
    
    public HoldableItem currentHoldable { get; private set; }

    private bool _inProcess;
    
    public async UniTask TakeAsync(HoldableItem holdable)
    {
        if (_inProcess)
            return;

        if (!currentHoldable.IsUnityNull())
            await HideAsync();

        _inProcess = true;
        await holdable.TakeAsync();

        holdable.transform.gameObject.SetActive(true);
        holdable.transform.SetParent(transform, false);
        holdable.transform.SetLocalPositionAndRotation(holdable.holdingPosition, Quaternion.Euler(holdable.holdingRotation));

        OnTake?.Invoke(holdable);
        currentHoldable = holdable;
        _inProcess = false;
    }
    
    public async UniTask HideAsync()
    {
        if (_inProcess)
            return;

        _inProcess = true;
        
        await currentHoldable.HideAsync();

        var eventTasks = OnHideBefore != null
            ? OnHideBefore.GetInvocationList()
                .Select(d => d as Func<HoldableItem, UniTask>)
                .Where(d => d != null)
                .Select(d => d.Invoke(currentHoldable))
            : Enumerable.Empty<UniTask>();

        await UniTask.WhenAll(eventTasks);
        
        currentHoldable.transform.gameObject.SetActive(false);
        
        OnHideAfter?.Invoke(currentHoldable);
        currentHoldable = null;
        _inProcess = false;
    }
}