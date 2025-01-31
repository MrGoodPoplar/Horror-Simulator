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

        [Header("Settings")]
        [SerializeField, Range(0, 5)] private float _fadeDuration = 0.5f;
        
        private InventoryController _inventoryController;
        private InventoryItem _currentOnHoverItem;
        private AutoLayoutContents _autoLayout;
        private List<InventoryItemAction> _currentActions = new();
        private List<HotkeyPrompt> _currentHotkeyPrompts = new();
        private bool _refreshPromptLayoutInProcess;


        private void Awake()
        {
            _autoLayout = GetComponent<AutoLayoutContents>();
        }

        private void Start()
        {
            _inventoryController = Player.instance.inventoryController;

            if (!_inventoryController)
                Destroy(this);
        }

        private async void Update()
        {
            if (!_refreshPromptLayoutInProcess && _inventoryController.onHoverInventoryItem != _currentOnHoverItem)
            {
                _currentOnHoverItem = _inventoryController.onHoverInventoryItem;
                _refreshPromptLayoutInProcess = true;
         
                await RefreshPromptLayout();
                
                _refreshPromptLayoutInProcess = false;
            }
        }
        
        private async UniTask HideHotkeyPromptsAsync()
        {
            await UniTask.WhenAll(_currentHotkeyPrompts.Select(async item =>
            {
                await item.HidePromptAsync(_fadeDuration);
                Destroy(item.gameObject);
            }));
            
            _currentHotkeyPrompts.Clear();
        }
        
        private async UniTask ShowHotkeyPromptsAsync(List<HotkeyPrompt> hotkeyPrompts)
        {
            await UniTask.WhenAll(hotkeyPrompts.Select(async item =>
            {
                await item.ShowPromptAsync(_fadeDuration);
            }));
        }
        
        private async UniTask RefreshPromptLayout()
        {
            bool toVoid = !_inventoryController.onHoverInventoryItem;
            await HideHotkeyPromptsAsync();
            ForgetActions();

            if (toVoid || !_inventoryController.onHoverInventoryItem)
                return;
            
            List<HotkeyPrompt> activeHotkeyPrompts = new();
            
            foreach (InventoryItemAction itemAction in _inventoryController.onHoverInventoryItem.inventoryItemSO.actions)
            {
                InputBindingSpriteBinder.BindingSpritePreference spritePreference = Player.instance.inputBindingSpriteBinder.GetSpritePreference(itemAction.GetInputBindingPath());

                if (!spritePreference.IsUnityNull())
                {
                    HotkeyPrompt hotkeyPrompt = Instantiate(_hotkeyPromptPrefab, transform, true);
                    hotkeyPrompt.SetPreferences(spritePreference.sprite, spritePreference.scale, itemAction.GetActionName());
                    itemAction.Activate();
                    
                    activeHotkeyPrompts.Add(hotkeyPrompt);
                    _currentActions.Add(itemAction);

                    if (!_currentHotkeyPrompts.Contains(hotkeyPrompt))
                        _currentHotkeyPrompts.Add(hotkeyPrompt);
                }
            }

            await UniTask.DelayFrame(2); // TODO: weak point
            
            _autoLayout.RefreshLayout();
            await ShowHotkeyPromptsAsync(activeHotkeyPrompts);
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