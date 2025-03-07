using System;
using Audio_System;

[Serializable]
public record RevolverSoundsData : WeaponSoundsData
{
    public ArraySoundData bulletInsertSounds;
    public ArraySoundData cylinderSpinSounds;
}