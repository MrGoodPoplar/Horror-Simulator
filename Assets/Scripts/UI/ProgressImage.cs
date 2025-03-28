using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ProgressImage : MonoBehaviour
{
    [SerializeField] private Image _progressImage;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TextMeshProUGUI _promptText;
    
    public bool active { get; private set; }

    private bool _isTemporaryMessage;
    private string _defaultText;
    private CancellationTokenSource _cts;
    private RectTransform _rect;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
    }

    public async UniTaskVoid ShowTemporaryMessageAsync(string text, float duration)
    {
        if (_isTemporaryMessage)
            return;
        
        _isTemporaryMessage = true;
        _promptText.text = text;

        await UniTask.WaitForSeconds(duration);

        _isTemporaryMessage = false;
        _promptText.text = _defaultText;
    }

    public ProgressImage Set(Sprite sprite, string text = null)
    {
        _progressImage.sprite = sprite;
        _backgroundImage.sprite = sprite;
        _promptText.text = text;
        _defaultText = text;
        
        _progressImage.type = Image.Type.Filled;
        return this;
    }
    
    public ProgressImage SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        _progressImage.fillAmount = progress;
        return this;
    }

    public ProgressImage Toggle(bool toggle)
    {
        active = toggle;

        if (!toggle)
        {
            _progressImage.sprite = null;
            _backgroundImage.sprite = null;
        }
        
        return this;
    }

    public ProgressImage SetPivot(Vector2 pivot)
    {
        _rect.pivot = pivot;
        return this;
    }
}