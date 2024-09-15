using System.Linq;
using UnityEngine;

public class German130BulletVisual : MonoBehaviour
{
    [field: Header("Constraints")]
    [field: SerializeField] public German130Bullet[] bullets { get; private set; }
    
    public void ShowBullets(int count)
    {
        foreach (German130Bullet bullet in bullets.Take(count).ToArray())
        {
            bullet.Show();
        }
    }

    public void HideBullets(int count)
    {
        foreach (German130Bullet bullet in bullets.Take(count).ToArray())
        {
            bullet.Hide();
        }
    }
}