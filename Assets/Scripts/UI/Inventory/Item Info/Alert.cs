using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory.Item_Info
{
    [RequireComponent(typeof(Image))]
    public class Alert : MonoBehaviour
    {
        [SerializeField] private float _fadeSpeed;
        [SerializeField] private float _duration;

        private Image _image;
        private CancellationTokenSource _alertCts;

        private void Start()
        {
            _image = transform.GetComponent<Image>();
            
            Hide();
        }

        public void Hide()
        {
            _image.enabled = false;
            transform.SetParent(null, false);
        }
        
        public void Perform(Transform parent, Quaternion rotation)
        {
            transform.SetParent(parent, false);
            
            _alertCts?.Cancel();
            _alertCts?.Dispose();
    
            _alertCts = new CancellationTokenSource();
            
            _image.enabled = true;
            
            Rotate(rotation);
            AlertFlashAsync(_fadeSpeed, _duration, _alertCts.Token).Forget();
        }
        
        private async UniTaskVoid AlertFlashAsync(float fadeSpeed, float duration, CancellationToken ct)
        {
            float halfFadeSpeed = fadeSpeed * 0.5f;

            await FadeAlpha(0f, 1f, halfFadeSpeed, ct);
            await UniTask.WaitForSeconds(duration, cancellationToken: ct);
            await FadeAlpha(1f, 0f, halfFadeSpeed, ct);
        }
        
        private async UniTask FadeAlpha(float from, float to, float time, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;

            float t = 0f;
            var baseColor = _image.color;

            while (t < time)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(from, to, t / time);
                _image.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                await UniTask.Yield(ct);
            }

            _image.color = new Color(baseColor.r, baseColor.g, baseColor.b, to);
        }

        public void Rotate(Quaternion rotation)
        {
            _image.rectTransform.localRotation = rotation;
        }
    }
}