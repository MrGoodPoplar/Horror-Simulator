using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("Prompt Settings")]
    [SerializeField] private float _tempMessageDuration = 1.0f;

    [Header("Constraints")]
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _promptUI;
    [SerializeField] private TextMeshProUGUI _promptText;
    [SerializeField] private ProgressImage _promptImage;
    
    private InteractController _interactController;
    private Vector3 _originalScale;
    private IInteractable _currentInteractable;
    private Transform _visualPrompt;
    
    private void Start()
    {
        _interactController = Player.instance.InteractController;
        
        _originalScale = _promptUI.transform.localScale;
        
        _promptUI.gameObject.SetActive(false);
        _promptImage.gameObject.SetActive(false);
        
        _interactController.OnInteract += OnInteractPerformed;
        _interactController.OnInteractCancelled += OnInteractCancelled;
        _interactController.OnInteractHover += OnInteractHover;
        _interactController.OnInteractUnhover += OnInteractUnhover;
    }

    private void OnDestroy()
    {
        _interactController.OnInteract -= OnInteractPerformed;
        _interactController.OnInteractCancelled -= OnInteractCancelled;
        _interactController.OnInteractHover -= OnInteractHover;
        _interactController.OnInteractUnhover -= OnInteractUnhover;
    }

    private void Update()
    {
        if (!_currentInteractable.IsUnityNull())
        {
            _visualPrompt.position = _currentInteractable.GetAnchorPosition();

            if (!_currentInteractable.instant && _promptImage.active)
            {
                _promptImage.SetProgress(_interactController.holdingProgress);
            }
        }
        else if (_visualPrompt && _visualPrompt.gameObject.activeSelf)
        {
            _visualPrompt.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (!_currentInteractable.IsUnityNull())
        {
            Quaternion rotation = _camera.transform.rotation;
            transform.LookAt(transform.position + rotation * Vector3.forward, rotation * Vector3.up);
        }
    }
    
    private void OnInteractPerformed(object sender, InteractController.InteractEventArgs e)
    {
        if (!e.interactable.IsUnityNull() && !Player.instance.isHUDView)
        {
            _currentInteractable = e.interactable;
            
            if (e.response.updateVisual)
                SetVisualPrompt(_currentInteractable);
            else if (e.response.hasMessage)
                _promptImage.ShowTemporaryMessageAsync(e.response.message, _tempMessageDuration).Forget();
            else if (_currentInteractable.interactableVisualSO.interactEffectEnabled)
                StartCoroutine(InteractPressedCoroutine());
        }
    }
    
    private void OnInteractCancelled(object sender, InteractController.InteractEventArgs e)
    {
        if (!e.interactable.IsUnityNull() && !e.interactable.instant)
            _promptImage.SetProgress(0);
    }
    
    private void OnInteractHover(object sender, InteractController.InteractEventArgs e)
    {
        _currentInteractable = e.interactable;
        SetVisualPrompt(_currentInteractable);
    }

    private void SetVisualPrompt(IInteractable interactable)
    {
        _visualPrompt?.gameObject.SetActive(false);

        if (interactable.interactableVisualSO.visualType == InteractableVisualSO.VisualType.Icon)
        {
            _visualPrompt = _promptImage.transform;
            _promptImage.Toggle(true);
            _promptImage.Set(_currentInteractable.interactableVisualSO.sprite, interactable.GetInteractableName());
            _promptImage.SetProgress(interactable.instant ? 1 : 0);
        }
        else
        {
            _promptImage.Toggle(false);
            
            _promptText.text = interactable.GetInteractableName();
            _visualPrompt = _promptUI;
        }
        
        _visualPrompt.position = _currentInteractable.GetAnchorPosition();
        _visualPrompt.gameObject.SetActive(true);
    }
    
    private void OnInteractUnhover(object sender, InteractController.InteractEventArgs e)
    {
        if (!_currentInteractable.IsUnityNull())
        {
            _currentInteractable = null;
            
            _promptImage.Toggle(false);
            _visualPrompt.gameObject.SetActive(false);
        }
    }
    
    private IEnumerator InteractPressedCoroutine()
    {
        float elapsed = 0f;
        float scale = _currentInteractable.interactableVisualSO.interactScaleEffect;
        float duration = _currentInteractable.interactableVisualSO.interactDurationEffect;
        
        Vector3 targetScale = _originalScale * scale;

        while (elapsed < duration)
        {
            _visualPrompt.transform.localScale = Vector3.Lerp(_originalScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _visualPrompt.transform.localScale = targetScale;

        elapsed = 0f;
        while (elapsed < duration)
        {
            _visualPrompt.transform.localScale = Vector3.Lerp(targetScale, _originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _visualPrompt.transform.localScale = _originalScale;
    }
}
