using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    [CreateAssetMenu(menuName = "UI/Input Binding Sprite Binder")]
    public class InputBindingSpriteBinder : ScriptableObject
    {
        [System.Serializable]
        public class BindingSpritePreference
        {
            [field: SerializeField, BindingPathDropdown] public string bindingPath { get; private set; } // TODO: proper way to assign binding path
            [field: SerializeField] public Sprite sprite { get; private set; }
            [field: SerializeField] public Vector2 scale { get; private set; } = new(1, 1);
        }

        [SerializeField] private List<BindingSpritePreference> _bindingSpritePairs = new();

        public BindingSpritePreference GetSpritePreference(string bindingPath)
        {
            return _bindingSpritePairs.FirstOrDefault(pair => pair.bindingPath == bindingPath);
        }
    }
}