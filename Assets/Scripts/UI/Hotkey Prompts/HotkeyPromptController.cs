using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Library.UnityUIHelpers;
using UI.Inventory;
using UI.Inventory.Actions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace UI.Hotkey_Prompts
{
    [RequireComponent(typeof(AutoLayoutContents))]
    public class HotkeyPromptController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private HotkeyPrompt _hotkeyPromptPrefab;

        [Header("Settings")]
        [SerializeField, Range(0, 5)] private float _fadeDuration = 0.5f;
        
        [Header("Hotkey Prompt Pool Settings")]
        [SerializeField] private int _poolDefaultSize = 10;
        [SerializeField] private int _poolMaxSize = 20;
        [SerializeField] private bool _collectionCheck;
        
        private InventoryController _inventoryController;
        private InventoryItem _currentOnHoverItem;
        private AutoLayoutContents _autoLayout;
        private List<InventoryItemAction> _currentActions = new();
        private List<HotkeyPrompt> _currentHotkeyPrompts = new();
        private IObjectPool<HotkeyPrompt> _hotkeyPromptPool;
        private bool _refreshPromptLayoutInProcess;


        private void Awake()
        {
            _autoLayout = GetComponent<AutoLayoutContents>();
            
            InitPoolObjects();
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

        private void InitPoolObjects()
        {
            _hotkeyPromptPool = new ObjectPool<HotkeyPrompt>(CreateHotkeyPrompt, OnGetHotkeyPromptFromPool, OnReleaseHotkeyPromptToPool, OnDestroyPooledHotkeyPrompt,
                _collectionCheck, _poolDefaultSize, _poolMaxSize);
        }

        private void OnDestroyPooledHotkeyPrompt(HotkeyPrompt hotkeyPrompt) => hotkeyPrompt.gameObject.SetActive(false); // TODO: weak point
        
        private void OnReleaseHotkeyPromptToPool(HotkeyPrompt hotkeyPrompt) => hotkeyPrompt.gameObject.SetActive(false);

        private void OnGetHotkeyPromptFromPool(HotkeyPrompt hotkeyPrompt) => hotkeyPrompt.gameObject.SetActive(true);

        private HotkeyPrompt CreateHotkeyPrompt() => Instantiate(_hotkeyPromptPrefab);

        private async UniTask HideHotkeyPromptsAsync()
        {
            await UniTask.WhenAll(_currentHotkeyPrompts.Select(async item =>
            {
                await item.HidePromptAsync(_fadeDuration);
            }));

            foreach (var item in _currentHotkeyPrompts)
            {
                _hotkeyPromptPool.Release(item);
            }
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
            await HideHotkeyPromptsAsync();
            ForgetActions();
            
            if (!_inventoryController.onHoverInventoryItem)
                return;

            List<HotkeyPrompt> activeHotkeyPrompts = new();
            
            foreach (InventoryItemAction itemAction in _inventoryController.onHoverInventoryItem.inventoryItemSO.actions)
            {
                InputBindingSpriteBinder.BindingSpritePreference spritePreference = Player.instance.inputBindingSpriteBinder.GetSpritePreference(itemAction.GetInputBindingPath());

                if (!spritePreference.IsUnityNull())
                {
                    HotkeyPrompt hotkeyPrompt = _hotkeyPromptPool.Get();
                    hotkeyPrompt.SetPreferences(spritePreference.sprite, spritePreference.scale, itemAction.GetActionName());
                    hotkeyPrompt.transform.SetParent(transform);
                    itemAction.Activate();
                    
                    activeHotkeyPrompts.Add(hotkeyPrompt);
                    _currentActions.Add(itemAction);

                    if (!_currentHotkeyPrompts.Contains(hotkeyPrompt))
                        _currentHotkeyPrompts.Add(hotkeyPrompt);
                }
            }

            await UniTask.WaitForSeconds(0.1f); // TODO: weak point
            
            _autoLayout.RefreshLayout(); // TODO: weak point
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