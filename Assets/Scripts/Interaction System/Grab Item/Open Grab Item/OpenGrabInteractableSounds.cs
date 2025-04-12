using System;
using Prop;
using UnityEngine;

namespace Interaction_System.Grab_Item.Open_Grab_Item
{
    [Serializable]
    public record OpenGrabInteractableSounds : GrabInteractableSounds
    {
        [field: SerializeField] public SoundSO changeStateSoundSO { get; protected set; }
    }
}