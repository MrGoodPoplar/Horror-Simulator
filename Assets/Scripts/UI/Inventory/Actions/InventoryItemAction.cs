using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

namespace UI.Inventory.Actions
{
    public abstract class InventoryItemAction : ScriptableObject
    {
        [SerializeField] protected LocalizedString LabelText;
        [SerializeField] protected InputActionReference InputActionReference;
        [field: SerializeField] public bool onlyInGridMain { get; private set; }
        
        public string actionName => LabelText.GetLocalizedString();
        
        public string GetInputBindingPath()
        {
            if (InputActionReference&& !InputActionReference.action.IsUnityNull())
            {
                return InputActionReference.action.bindings.FirstOrDefault().effectivePath;
            }
            
            return null;
        }
        
        public void Activate()
        {
            InputActionReference.action.Enable();
            InputActionReference.action.performed += OnActionPerformed;
        }

        public void Deactivate()
        {
            InputActionReference.action.Disable();
            InputActionReference.action.performed -= OnActionPerformed;
        }

        protected abstract void OnActionPerformed(InputAction.CallbackContext obj);
    }
}
