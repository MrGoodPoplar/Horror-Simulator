using Unity.VisualScripting;
using UnityEngine;

public class HoldingItemController : MonoBehaviour
{
    public IHoldable currentHoldable { get; private set; }

    public void Hold(IHoldable holdable)
    {
        if (!currentHoldable.IsUnityNull())
            currentHoldable.OnHide();

        holdable.OnHold();
        holdable.transform.gameObject.SetActive(true);
        holdable.transform.SetParent(transform);

        currentHoldable = holdable;
    }

    public void Hide()
    {
        currentHoldable.OnHide();
        currentHoldable.transform.gameObject.SetActive(false);
        currentHoldable = null;
    }
}