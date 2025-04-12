using Audio_System;
using UnityEngine;

namespace Prop
{
    public class PropImpactHandler : MonoBehaviour
    {
        [SerializeField] private SoundSO _impactSoundSO;
        
        private void OnCollisionEnter(Collision collision)
        {
            SoundManager.Instance.CreateSound()
                .WithSoundData(_impactSoundSO.soundData)
                .WithRandomPitch()
                .WithPosition(collision.GetContact(0).point)
                .Play();
        }
    }
}