
using Cysharp.Threading.Tasks;

public interface IReloadHandler
{
    public bool isReloading { get; }

    public UniTask ReloadAsync(int toReload);
}
