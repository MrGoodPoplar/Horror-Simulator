using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressImage : MonoBehaviour
{
    [SerializeField] private Image _progressImage;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TextMeshProUGUI _promptText;
    
    public bool active { get; private set; }

    private bool _isTemporaryMessage;
    private string _defaultText;
    private CancellationTokenSource _cts;

    private void OnDestroy()
    {
        _cts?.Cancel();
    }

    public void Set(Sprite sprite, string text = null)
    {
        _progressImage.sprite = sprite;
        _backgroundImage.sprite = sprite;
        _promptText.text = text;
        _defaultText = text;
        
        _progressImage.type = Image.Type.Filled;
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

    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        _progressImage.fillAmount = progress;
    }

    public void Toggle(bool toggle)
    {
        active = toggle;

        if (!toggle)
        {
            _progressImage.sprite = null;
            _backgroundImage.sprite = null;
        }
    }
}