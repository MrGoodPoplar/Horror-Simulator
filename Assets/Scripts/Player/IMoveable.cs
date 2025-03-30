using System;
using UnityEngine;

public interface IMoveable
{
    public event Action OnLanded;
    
    public Transform transform { get; }
    public float speed { get; }
    public float speedHorizontal { get; }
    public float speedVertical { get; }

    public bool isGrounded { get; }
}