using System;
using Audio_System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class BulletShell : MonoBehaviour
{
    [SerializeField] private ArraySoundData _fallSounds;
    [SerializeField] private Vector3 _targetScale = new (1, 1, 1);
    [SerializeField] private float _scalingSpeed = 0.5f;
    private IObjectPool<BulletShell> _bulletShellPool;

    private Vector3 _initialScale;

    private void Awake()
    {
        _initialScale = transform.localScale;
    }

    // TODO: proper surface type system
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent(out BulletShell _))
            return;
        
        SoundManager.Instance.CreateSound()
            .WithSoundData(_fallSounds)
            .WithPosition(transform.position)
            .WithRandomPitch()
            .Play();
    }

    public void SetBulletShellPool(IObjectPool<BulletShell> bulletShellPool)
    {
        _bulletShellPool = bulletShellPool;
    }
    
    public async UniTaskVoid DropAsync(Vector3 startPos, float lifeSpan)
    {
        transform.localScale = _initialScale;
        transform.position = startPos;
        transform.rotation = Quaternion.identity;

        SmoothScaleAsync(_targetScale).Forget();;
        
        await UniTask.WaitForSeconds(lifeSpan);
        _bulletShellPool.Release(this);
    }

    private async UniTaskVoid SmoothScaleAsync(Vector3 targetScale)
    {
        float elapsedTime = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsedTime < 1f)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime);
            elapsedTime += Time.deltaTime * _scalingSpeed;
            await UniTask.Yield(PlayerLoopTiming.Update); // Wait until the next frame
        }

        transform.localScale = targetScale;
    }

}