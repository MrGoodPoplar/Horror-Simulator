using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Hotkey_Prompts
{
    [RequireComponent(typeof(CanvasGroup), typeof(RectTransform))]
    public class HotkeyPrompt : MonoBehaviour
    {
        [Header("Constraints")]
        [SerializeField] private Image _hotkeyImage;
        [SerializeField] private TextMeshProUGUI _hotkeyLabel;
        
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void SetPreferences(Sprite icon, Vector2 spriteScale, string text)
        {
            _hotkeyImage.sprite = icon;
            _hotkeyImage.rectTransform.localScale = spriteScale;
            _hotkeyLabel.text = text;
        }
        
        public async UniTask ShowPromptAsync(float fadeDuration = 0.5f)
        {
            await FadeCanvasGroup(0f, 1f, fadeDuration);
        }

        public async UniTask HidePromptAsync(float fadeDuration = 0.5f)
        {
            await FadeCanvasGroup(1f, 0f, fadeDuration);
        }

        private async UniTask FadeCanvasGroup(float from, float to, float fadeDuration)
        {
            float elapsedTime = 0f;
            _canvasGroup.alpha = from;
            _canvasGroup.interactable = from > 0f;
            _canvasGroup.blocksRaycasts = from > 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, to, elapsedTime / fadeDuration);
                await UniTask.Yield();
            }

            _canvasGroup.alpha = to;
            _canvasGroup.interactable = to > 0f;
            _canvasGroup.blocksRaycasts = to > 0f;
        }
    }
}
