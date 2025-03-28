using TMPro;
using UnityEngine;

namespace UI
{
    public class PromptLabel : MonoBehaviour
    {
        [SerializeField] private RectTransform _promptLabel;
        [SerializeField] private TextMeshProUGUI _promptText;

        public PromptLabel SetPivot(Vector2 pivot)
        {
            _promptLabel.pivot = pivot;
            return this;
        }

        public PromptLabel SetText(string text)
        {
            _promptText.text = text;
            return this;
        }
    }
}