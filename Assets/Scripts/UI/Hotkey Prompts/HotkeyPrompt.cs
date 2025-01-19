using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Hotkey_Prompts
{
    public class HotkeyPrompt : MonoBehaviour
    {
        [Header("Constraints")]
        [SerializeField] private Image _hotkeyImage;
        [SerializeField] private TextMeshProUGUI _hotkeyLabel;

        public void SetPreferences(Sprite icon, Vector2 spriteScale, string text)
        {
            _hotkeyImage.sprite = icon;
            _hotkeyImage.rectTransform.sizeDelta *= spriteScale;
            _hotkeyLabel.text = text;
        }
    }
}
