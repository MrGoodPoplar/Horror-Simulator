using System;
using UnityEngine;

namespace Audio_System
{
    [RequireComponent(typeof(Weapon))]
    public class WeaponSoundHandler : MonoBehaviour
    {
        [Header("SFX")]
        [SerializeField] private ArraySoundData _fireArraySound;
        
        protected Weapon _weapon;
        protected ShooterController _shooterController;

        private void Awake()
        {
            _weapon = GetComponent<Weapon>();
            _shooterController = Player.Instance.shooterController;
        }

        private void Start()
        {
            _shooterController.OnFire += OnFirePerformed;
            _shooterController.OnReload += OnReloadPerformed;
        }

        private void OnDestroy()
        {
            _shooterController.OnFire -= OnFirePerformed;
            _shooterController.OnReload -= OnReloadPerformed;
        }

        private void OnFirePerformed()
        {
            SoundManager.Instance.CreateSound()
                .WithSoundData(_fireArraySound)
                .WithRandomPitch()
                .WithPosition(_weapon.transform.position)
                .Play();
        }

        private void OnReloadPerformed(int bullets)
        {
            Debug.Log($"RELOAD");
        }
    }
}