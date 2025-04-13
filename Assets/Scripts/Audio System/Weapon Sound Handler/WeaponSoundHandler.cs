using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Audio_System
{
    public class WeaponSoundHandler<T1, T2> : MonoBehaviour
        where T1 : WeaponSoundConstraints where T2 : WeaponSoundsData
    {
        [SerializeField] protected T1 _constraints;
        [SerializeField] protected T2 _soundsData;

        protected ShooterController _shooterController;

        protected virtual void Awake()
        {
            _shooterController = Player.Instance.shooterController;
        }

        private void Start()
        {
            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        protected virtual void Subscribe()
        {
            _shooterController.OnFire += OnFirePerformed;
            _shooterController.OnDryFire += OnDryFirePerformed;
            _shooterController.OnReloadStart += OnReloadStartStartPerformed;
            _shooterController.OnReloadEnd += OnReloadEndPerformed;
        }

        protected virtual void Unsubscribe()
        {
            _shooterController.OnFire -= OnFirePerformed;
            _shooterController.OnDryFire -= OnDryFirePerformed;
            _shooterController.OnReloadStart -= OnReloadStartStartPerformed;
            _shooterController.OnReloadEnd -= OnReloadEndPerformed;
        }

        protected virtual void OnFirePerformed() => PlaySoundOnWeapon(_soundsData.fireSounds);
        
        protected virtual void OnDryFirePerformed() => PlaySoundOnWeapon(_soundsData.dryFireSounds);

        protected virtual void OnReloadStartStartPerformed(int bullets) => PlaySoundOnWeapon(_soundsData.reloadStartSounds);

        protected virtual void OnReloadEndPerformed() => PlaySoundOnWeapon(_soundsData.reloadEndSounds);
        
        protected void PlaySoundOnWeapon(SoundData soundData)
        {
            if (!soundData.GetClip())
                return;
            
            SoundManager.Instance.CreateSound()
                .WithSoundData(soundData)
                .WithRandomPitch()
                .WithPosition(_constraints.weapon.transform.position)
                .Play();
        }

        protected async UniTask PlaySoundOnWeaponAsync(SoundData soundData, float delay = 0f)
        {
            await UniTask.WaitForSeconds(delay);
            PlaySoundOnWeapon(soundData);
        }
    }
}