using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class BulletShell : MonoBehaviour
{
    private IObjectPool<BulletShell> _bulletShellPool;
    
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