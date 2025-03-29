using Audio_System;
using UnityEngine;

namespace Prop
{
    public class PropImpactHandler : MonoBehaviour
    {
        [SerializeField] private PropSoundSO _impactSoundSO;
        
        private void OnCollisionEnter(Collision collision)
        {
            SoundManager.Instance.CreateSound()
                .WithSoundData(_impactSoundSO.impactSounds)
                .WithRandomPitch()
                .WithPosition(collision.GetContact(0).point)
                .Play();
        }
    }
}