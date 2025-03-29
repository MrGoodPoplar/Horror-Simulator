using System;
using UnityEngine;

public interface IMoveable
{
    public event Action OnLanded;
    
    public Transform transform { get; }
    public float velocity { get; }
    public bool isGrounded { get; }
}