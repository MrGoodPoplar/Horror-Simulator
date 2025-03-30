using Cysharp.Threading.Tasks;
using Surface_System;
using UnityEngine;
using UnityEngine.Pool;

public class BulletShell : MonoBehaviour
{
    [SerializeField] private Vector3 _targetScale = new (1, 1, 1);
    [SerializeField] private float _scalingSpeed = 0.5f;
    private IObjectPool<BulletShell> _bulletShellPool;

    private Vector3 _initialScale;

    private void Awake()
    {
        _initialScale = transform.localScale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out BulletShell _))
            return;
     
        var data = Player.Instance.surfaceManager.GetImpactDetails(collision);

        if (data == null)
            return;
        
        new SurfaceImpactHandler(data)
            .PlaySound(data.surfaceImpactSound.bulletDropImpactSounds)
            .PlayVfx();
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