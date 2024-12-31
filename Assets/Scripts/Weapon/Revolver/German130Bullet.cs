using UnityEngine;

public class German130Bullet : MonoBehaviour
{
    [field: SerializeField] public Transform shell { get; private set; }

    public void Show()
    {
        shell.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        shell.gameObject.SetActive(false);
    }
}