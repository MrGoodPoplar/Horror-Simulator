using Audio_System;
using UnityEngine;

namespace Surface_System
{
    public class SurfaceImpactHandler
    {
        private readonly SurfaceData _data;
        
        public SurfaceImpactHandler(SurfaceData data)
        {
            _data = data;
        }

        public SurfaceImpactHandler PlaySound(SoundData soundData)
        {
            if (soundData != null)
            {
                SoundManager.Instance.CreateSound()
                    .WithSoundData(soundData)
                    .WithRandomPitch()
                    .WithPosition(_data.position)
                    .Play();
            }
            
            return this;
        }

        public SurfaceImpactHandler PlayVfx()
        {
            return this;
        }
    }
}