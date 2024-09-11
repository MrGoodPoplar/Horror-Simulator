using System;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(GrabItemInteractable))]
public class Mag357AmmoBoxVisual : MonoBehaviour
{
    private const string OPEN_COVER = "openCover";
    private const string CLOSE_COVER = "closeCover";

    [Header("Settings")]
    [SerializeField] private Vector2Int _generateAmmoGrid;
    [SerializeField] private Vector2 _generateAmmoOffset;

    [Header("Constraints")]
    [SerializeField] private Transform _roundPrefab;
    [SerializeField] private Transform _generateAmmoPoint;
    
    private GrabItemInteractable _grabItemInteractable;
    private Animator _animator;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _grabItemInteractable = GetComponent<GrabItemInteractable>();
    }

    private void Start()
    {
        _grabItemInteractable.OnInteract += OnInteractPerformed;
        _grabItemInteractable.OnQuantitySet += OnQuantitySetPerfmored;
    }

    private void OnDestroy()
    {
        _grabItemInteractable.OnInteract -= OnInteractPerformed;
        _grabItemInteractable.OnQuantitySet -= OnQuantitySetPerfmored;
    }

    private void OnQuantitySetPerfmored()
    {
        GenerateAmmo(_grabItemInteractable.quantity);
    }

    private void OnInteractPerformed()
    {
        _animator.SetTrigger(OPEN_COVER);
    }

    private void GenerateAmmo(int quantity)
    {
        
    }
}