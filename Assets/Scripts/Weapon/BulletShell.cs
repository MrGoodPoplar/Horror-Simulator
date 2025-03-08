using System;
using Audio_System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class BulletShell : MonoBehaviour
{
    [SerializeField] private ArraySoundData _fallSounds;
    private IObjectPool<BulletShell> _bulletShellPool;
    
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
        transform.position = startPos;
        transform.rotation = Quaternion.identity;

        await UniTask.WaitForSeconds(lifeSpan);
        _bulletShellPool.Release(this);
    }
}