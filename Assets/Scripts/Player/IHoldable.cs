using UnityEngine;

public interface IHoldable
{
    public Transform transform { get; }
    
    public void Take() {}

    public void Hide() {}
}