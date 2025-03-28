using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("Prompt Settings")]
    [SerializeField] private float _tempMessageDuration = 1.0f;

    [Header("Constraints")]
    [SerializeField] private Camera _camera;
    [SerializeField] private ProgressImage _promptImage;
    [SerializeField] private PromptLabel _promptLabel;

    public Vector3 offset;
    private InteractController _interactController;
    private Vector3 _originalScale;
    private IInteractable _currentInteractable;
    private Transform _currentVisual;
    
    private void Start()
    {
        _interactController = Player.Instance.interactController;
        
        _originalScale = _promptLabel.transform.localScale;
        
        _promptLabel.gameObject.SetActive(false);
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
            _currentVisual.position = _currentInteractable.GetAnchorPosition() + offset;
                
            if (!_currentInteractable.instant && _promptImage.active)
            {
                _promptImage.SetProgress(_interactController.holdingProgress);
            }
        }
        else if (_currentVisual && _currentVisual.gameObject.activeSelf)
        {
            _currentVisual.gameObject.SetActive(false);
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
        if (!e.interactable.IsUnityNull() && !Player.Instance.HUDController.isHUDView)
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
        _currentVisual?.gameObject.SetActive(false);
        Vector2 pivot = PivotUtility.GetPivotFromAlignment(interactable.spriteAlignment);
        
        if (interactable.interactableVisualSO.visualType == InteractableVisualSO.VisualType.Icon)
        {
            _promptImage
                .Toggle(true)
                .Set(_currentInteractable.interactableVisualSO.sprite, interactable.GetInteractableName())
                .SetPivot(pivot)
                .SetProgress(interactable.instant ? 1 : 0);
            
            _currentVisual = _promptImage.transform;
        }
        else
        {
            _promptLabel
                .SetText(interactable.GetInteractableName())
                .SetPivot(pivot);
            
            _currentVisual = _promptLabel.transform;
        }

        _currentVisual.transform.localScale = _originalScale;
        _currentVisual.position = _currentInteractable.GetAnchorPosition();
        _currentVisual.gameObject.SetActive(true);
    }


    private void OnInteractUnhover(object sender, InteractController.InteractEventArgs e)
    {
        if (!_currentInteractable.IsUnityNull())
        {
            _currentInteractable = null;
            
            _promptImage.Toggle(false);
            _currentVisual.gameObject.SetActive(false);
            _currentVisual = null;
        }
    }
    
    private IEnumerator InteractPressedCoroutine()
    {
        float scale = _currentInteractable.interactableVisualSO.interactScaleEffect;
        float duration = _currentInteractable.interactableVisualSO.interactDurationEffect;
        Vector3 targetScale = _originalScale * scale;
        IInteractable triggeredInteractable = _currentInteractable;

        // Scale up
        yield return ScaleObject(_originalScale, targetScale, duration, triggeredInteractable);

        // Scale down
        yield return ScaleObject(targetScale, _originalScale, duration, triggeredInteractable);

        IEnumerator ScaleObject(Vector3 fromScale, Vector3 toScale, float duration, IInteractable triggeredInteractable)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (triggeredInteractable != _currentInteractable)
                {
                    if (_currentVisual)
                        _currentVisual.transform.localScale = _originalScale;
                    yield break;
                }

                _currentVisual.transform.localScale = Vector3.Lerp(fromScale, toScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _currentVisual.transform.localScale = toScale;
        }
    }
}
