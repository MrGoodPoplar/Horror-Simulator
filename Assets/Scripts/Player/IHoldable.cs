using UnityEngine;

public interface IHoldable
{
    public Transform transform { get; }
    
    public void OnHold();

    public void OnHide();
}