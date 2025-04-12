using Audio_System;
using UnityEngine;

namespace Prop
{
    [CreateAssetMenu(menuName = "Audio System/Sound")]
    public class SoundSO : ScriptableObject
    {
        [field: SerializeField] public ArraySoundData soundData { get; private set; }
    }
}