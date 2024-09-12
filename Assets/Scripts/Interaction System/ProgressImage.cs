using UnityEngine;
using UnityEngine.UI;

public class ProgressImage : MonoBehaviour
{
    [SerializeField] private Image _progressImage;
    [SerializeField] private Image _backgroundImage;

    public bool active { get; private set; }
    
    public void SetSprite(Sprite sprite)
    {
        _progressImage.sprite = sprite;
        _backgroundImage.sprite = sprite;

        _progressImage.type = Image.Type.Filled;
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