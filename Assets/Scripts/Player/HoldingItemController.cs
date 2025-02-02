using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HoldingItemController : MonoBehaviour
{
    public event Action<IHoldable> OnTake;
    public event Action<IHoldable> OnHide;
    
    public IHoldable currentHoldable { get; private set; }
    
    public void Take(IHoldable holdable)
    {
        if (!currentHoldable.IsUnityNull())
            Hide();

        holdable.Take();
        holdable.transform.gameObject.SetActive(true);
        holdable.transform.SetParent(transform, false);
        
        OnTake?.Invoke(holdable);
        currentHoldable = holdable;
    }
    
    public void Hide()
    {
        currentHoldable.Hide();
        currentHoldable.transform.gameObject.SetActive(false);
        
        OnHide?.Invoke(currentHoldable);
        currentHoldable = null;
    }
}