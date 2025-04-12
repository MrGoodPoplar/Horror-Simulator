using System;
using UnityEngine;

namespace Audio_System
{
    [Serializable]
    public record WeaponSoundsData
    {
        [field: SerializeField] public ArraySoundData fireSounds { get; protected set; }
        [field: SerializeField] public ArraySoundData dryFireSounds { get; protected set; }
        [field: SerializeField] public ArraySoundData reloadStartSounds { get; protected set; }
        [field: SerializeField] public ArraySoundData reloadEndSounds { get; protected set; }
    }
}