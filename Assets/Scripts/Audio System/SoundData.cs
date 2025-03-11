using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio_System
{
    [Serializable]
    public abstract class SoundData
    {
        [Range(0, 1)] public float spatialBlend;
        public AudioMixerGroup mixerGroup;
        public bool loop;
        public bool playOnAwake;
        public bool isFrequent;
        public abstract AudioClip GetClip();
    }
}