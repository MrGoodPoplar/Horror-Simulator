using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Library.UnityUIHelpers;
using UI.Inventory;
using UI.Inventory.Actions;
using Unity.VisualScripting;
using UnityEngine;

namespace UI.Hotkey_Prompts
{
    [RequireComponent(typeof(AutoLayoutContents))]
    public class HotkeyPromptController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private HotkeyPrompt _hotkeyPromptPrefab;
        
        private InventoryController _inventoryController;
        private InventoryItem _currentOnHoverItem;
        private AutoLayoutContents _autoLayout;
        private List<InventoryItemAction> _currentActions = new();

        private void Awake()
        {
            _autoLayout = GetComponent<AutoLayoutContents>();
        }

        private void Start()
        {
            _inventoryController = Player.instance.inventoryController;

            if (!_inventoryController)
                Destroy(this); // TODO: cache, instead of destroy
        }

        private void Update()
        {
            if (_inventoryController.onHoverInventoryItem != _currentOnHoverItem)
            {
                _currentOnHoverItem = _inventoryController.onHoverInventoryItem;
                RefreshPromptLayout().Forget();
            }
        }

        private async UniTaskVoid RefreshPromptLayout()
        {
            _autoLayout.ClearChildren();
            ForgetActions();
            
            if (!_inventoryController.onHoverInventoryItem)
                return;

            foreach (InventoryItemAction itemAction in _inventoryController.onHoverInventoryItem.inventoryItemSO.actions)
            {
                HotkeyPrompt hotkeyPrompt = Instantiate(_hotkeyPromptPrefab);
                InputBindingSpriteBinder.BindingSpritePreference spritePreference = Player.instance.inputBindingSpriteBinder.GetSpritePreference(itemAction.GetInputBindingPath());

                if (!spritePreference.IsUnityNull())
                {
                    hotkeyPrompt.SetPreferences(spritePreference.sprite, spritePreference.scale, itemAction.GetActionName());
                    hotkeyPrompt.transform.SetParent(transform);
                    itemAction.Activate();
                    _currentActions.Add(itemAction);
                }
            }

            await UniTask.WaitForSeconds(0.01f);
            _autoLayout.RefreshLayout();
        }

        private void ForgetActions()
        {
            foreach (InventoryItemAction itemAction in _currentActions)
            {
                itemAction.Deactivate();
            }
            
            _currentActions.Clear();
        }
    }
}