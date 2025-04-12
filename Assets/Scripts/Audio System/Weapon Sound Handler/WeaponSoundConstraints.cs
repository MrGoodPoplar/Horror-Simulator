using System;
using UnityEngine;

namespace Audio_System
{
    [Serializable]
    public record WeaponSoundConstraints
    {
        [field: SerializeField] public Weapon weapon { get; protected set; }
    }
}