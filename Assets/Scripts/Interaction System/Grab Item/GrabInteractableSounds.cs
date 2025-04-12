using System;
using Prop;
using UnityEngine;

namespace Interaction_System.Grab_Item
{
    [Serializable]
    public record GrabInteractableSounds
    {
        [field: SerializeField] public SoundSO pickUpSoundSO { get; protected set; }
    }
}