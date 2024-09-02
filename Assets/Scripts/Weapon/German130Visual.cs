using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class German130Visual : MonoBehaviour
{
    private const string IS_AIMING = "isAiming";
    private const string VELOCITY = "velocity";
    
    private Animator _animator;
    private ShooterController _shooterController;
    private FirstPersonController _firstPersonController;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _shooterController = ShooterController.instance;
        _firstPersonController = FirstPersonController.instance;
    }

    private void Update()
    {
        _animator.SetBool(IS_AIMING, _shooterController.isAiming);
        _animator.SetFloat(VELOCITY, _firstPersonController.velocity);
    }
}
