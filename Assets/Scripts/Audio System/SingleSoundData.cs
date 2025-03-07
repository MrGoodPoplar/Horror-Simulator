using System;
using UnityEngine;

namespace Audio_System
{
    [Serializable]
    public class SingleSoundData : SoundData
    {
        [SerializeField] private AudioClip _clip;

        public override AudioClip GetClip() => _clip;
    }
}