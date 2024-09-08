using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("Interact Settings")]
    [SerializeField] private float _clickScale = 0.9f;
    [SerializeField] private float _clickDuration = 0.1f;

    [Header("Constraints")]
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _promptUI;
    [SerializeField] private TextMeshProUGUI _promptText;
    [SerializeField] private InteractController _interactController;
    
    private Vector3 _originalScale;
    private Transform _interactableTransform;
    
    private void Start()
    {
        _originalScale = _promptUI.transform.localScale;
        
        _promptUI.gameObject.SetActive(false);
        
        _interactController.OnInteract += OnInteractPerformed;
        _interactController.OnInteractHover += OnInteractHover;
        _interactController.OnInteractUnhover += OnInteractUnhover;
    }

    private void OnDestroy()
    {
        _interactController.OnInteract -= OnInteractPerformed;
        _interactController.OnInteractHover -= OnInteractHover;
        _interactController.OnInteractUnhover -= OnInteractUnhover;
    }

    private void Update()
    {
        if (_interactableTransform)
        {
            _promptUI.position = _interactableTransform.position;
        }
    }

    private void LateUpdate()
    {
        Quaternion rotation = _camera.transform.rotation;
        transform.LookAt(transform.position + rotation * Vector3.forward, rotation * Vector3.up);
    }
    
    private void OnInteractPerformed(object sender, InteractController.InteractEventArgs e)
    {
        StartCoroutine(InteractEffectCoroutine());
    }
    
    private void OnInteractHover(object sender, InteractController.InteractEventArgs e)
    {
        _interactableTransform = e.interactable.transform;
        _promptText.text = e.interactable.GetInteractionPrompt();
        
        _promptUI.position = _interactableTransform.position;
        _promptUI.gameObject.SetActive(true);
    }
    
    private void OnInteractUnhover(object sender, InteractController.InteractEventArgs e)
    {
        _promptUI.gameObject.SetActive(false);
    }
    
    private IEnumerator InteractEffectCoroutine()
    {
        float elapsed = 0f;
        Vector3 targetScale = _originalScale * _clickScale;

        while (elapsed < _clickDuration)
        {
            _promptUI.transform.localScale = Vector3.Lerp(_originalScale, targetScale, elapsed / _clickDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _promptUI.transform.localScale = targetScale;

        elapsed = 0f;
        while (elapsed < _clickDuration)
        {
            _promptUI.transform.localScale = Vector3.Lerp(targetScale, _originalScale, elapsed / _clickDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _promptUI.transform.localScale = _originalScale;
    }
}
