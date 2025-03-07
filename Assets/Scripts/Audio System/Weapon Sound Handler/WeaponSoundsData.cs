using System;

namespace Audio_System
{
    [Serializable]
    public record WeaponSoundsData
    {
        public ArraySoundData fireSounds;
        public ArraySoundData dryFireSounds;
        public ArraySoundData reloadStartSounds;
        public ArraySoundData reloadEndSounds;
    }
}