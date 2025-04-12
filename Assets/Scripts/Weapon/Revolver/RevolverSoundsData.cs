using System;
using Audio_System;
using UnityEngine;

[Serializable]
public record RevolverSoundsData : WeaponSoundsData
{
    [field: SerializeField] public ArraySoundData bulletInsertSounds { get; protected set; }
    [field: SerializeField] public ArraySoundData cylinderSpinSounds { get; protected set; } 
    [field: SerializeField] public ArraySoundData dropShellSounds { get; protected set; }
}