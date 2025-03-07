using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Audio_System
{
    [Serializable]
    public class ArraySoundData : SoundData
    {
        [SerializeField] private AudioClip[] _clips;

        public override AudioClip GetClip()
        {
            return _clips is { Length: > 0 }
                ? _clips[Random.Range(0, _clips.Length)]
                : null;
        }
    }
}