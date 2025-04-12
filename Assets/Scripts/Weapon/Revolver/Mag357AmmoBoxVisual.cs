using System;
using System.Collections.Generic;
using Audio_System;
using Interaction_System.Grab_Item.Open_Grab_Item;
using Prop;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(OpenGrabItemInteractable))]
public class Mag357AmmoBoxVisual : MonoBehaviour
{
    private const string OPEN_COVER = "openCover";
    private const string CLOSE_COVER = "closeCover";

    [Header("Settings")]
    [SerializeField] private Vector2Int _generateAmmoGrid;
    [SerializeField] private Vector2 _generateAmmoOffset;
    [SerializeField] private Vector3 _rotation;

    [Header("Constraints")]
    [SerializeField] private Transform _roundPrefab;
    [SerializeField] private Transform _generateAmmoPoint;
    
    private OpenGrabItemInteractable _openGrabItemInteractable;
    private Animator _animator;
    private List<Transform> _rounds = new ();
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _openGrabItemInteractable = GetComponent<OpenGrabItemInteractable>();

        _openGrabItemInteractable.OnOpen += OnOpenPerformed;
        _openGrabItemInteractable.OnInteract += OnQuantityUpdatePerformed;
        _openGrabItemInteractable.OnSet += OnSetPerformed;
        _openGrabItemInteractable.OnQuantityUpdate += OnQuantityUpdatePerformed;
    }

    private void OnDestroy()
    {
        _openGrabItemInteractable.OnOpen -= OnOpenPerformed;
        _openGrabItemInteractable.OnInteract -= OnQuantityUpdatePerformed;
        _openGrabItemInteractable.OnSet -= OnSetPerformed;
        _openGrabItemInteractable.OnQuantityUpdate -= OnQuantityUpdatePerformed;
    }

    private void OnSetPerformed()
    {
        GenerateAmmo(_openGrabItemInteractable.quantity);
    }
    
    private void OnQuantityUpdatePerformed()
    {
        if (_rounds.Count > 0)
        {
            ClearAmmo(_rounds.Count - _openGrabItemInteractable.quantity);
        }
    }

    private void OnOpenPerformed()
    {
        _animator.SetTrigger(OPEN_COVER);
    }

    private void GenerateAmmo(int quantity)
    {
        int rows = _generateAmmoGrid.y;
        int columns = _generateAmmoGrid.x;
        
        int maxAmmo = rows * columns;
        int ammoToGenerate = Mathf.Min(quantity, maxAmmo);
        Vector3 startPosition = _generateAmmoPoint.position;

        for (int i = 0; i < ammoToGenerate; i++)
        {
            int row = i / columns;
            int col = i % columns;

            Vector3 spawnPosition = startPosition + new Vector3(col * _generateAmmoOffset.x, 0, row * _generateAmmoOffset.y);
            Transform addedRound = Instantiate(_roundPrefab, spawnPosition, Quaternion.Euler(_rotation), _generateAmmoPoint);
            
            _rounds.Add(addedRound);
        }

        _generateAmmoPoint.localRotation = transform.rotation;
    }

    private void ClearAmmo(int quantity)
    {
        int ammoToClear = Mathf.Clamp(quantity, 0, _rounds.Count);

        for (int i = 0; i < ammoToClear; i++)
        {
            Destroy(_rounds[i].gameObject);
        }

        _rounds.RemoveRange(0, ammoToClear);
    }
}