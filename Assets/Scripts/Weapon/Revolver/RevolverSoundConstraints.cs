using System;
using Audio_System;
using UnityEngine;

[Serializable]
public record RevolverSoundConstraints : WeaponSoundConstraints
{
    [field: SerializeField] public German130Visual revolverVisual { get; protected set; }
}