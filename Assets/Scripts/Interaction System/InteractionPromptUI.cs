using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("Interact Settings")]
    [SerializeField] private float _clickScale = 0.9f;
    [SerializeField] private float _clickDuration = 0.1f;

    [Header("Constraints")]
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _promptUI;
    [SerializeField] private TextMeshProUGUI _promptText;
    [SerializeField] private Image _promptImage;
    [SerializeField] private InteractController _interactController;
    
    private Vector3 _originalScale;
    private Transform _interactableTransform;
    private Transform _visualPrompt;
    
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
            _visualPrompt.position = _interactableTransform.position;
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
        _promptText.text = e.interactable.InteractableVisualSO.text;

        _visualPrompt?.gameObject.SetActive(false);
        _visualPrompt = e.interactable.InteractableVisualSO.visualType == InteractableVisualSO.VisualType.Icon ? _promptImage.transform : _promptUI;
        
        _visualPrompt.position = _interactableTransform.position;
        _visualPrompt.gameObject.SetActive(true);
    }
    
    private void OnInteractUnhover(object sender, InteractController.InteractEventArgs e)
    {
        _visualPrompt.gameObject.SetActive(false);
    }
    
    private IEnumerator InteractEffectCoroutine()
    {
        float elapsed = 0f;
        Vector3 targetScale = _originalScale * _clickScale;

        while (elapsed < _clickDuration)
        {
            _visualPrompt.transform.localScale = Vector3.Lerp(_originalScale, targetScale, elapsed / _clickDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _visualPrompt.transform.localScale = targetScale;

        elapsed = 0f;
        while (elapsed < _clickDuration)
        {
            _visualPrompt.transform.localScale = Vector3.Lerp(targetScale, _originalScale, elapsed / _clickDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _visualPrompt.transform.localScale = _originalScale;
    }
}
