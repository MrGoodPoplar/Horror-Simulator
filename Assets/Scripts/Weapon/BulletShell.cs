using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

public class BulletShell : MonoBehaviour
{
    private IObjectPool<BulletShell> _bulletShellPool;
    
    public void SetBulletShellPool(IObjectPool<BulletShell> bulletShellPool)
    {
        _bulletShellPool = bulletShellPool;
    }
    
    public async void Drop(float lifeSpan)
    {
        await Task.Delay((int)(lifeSpan * 1000));
        _bulletShellPool.Release(this);
    }
}