using System;
using UnityEngine;

namespace Interaction_System.Grab_Item.Open_Grab_Item
{
    [Serializable]
    public record OpenGrabInteractableConstraints : GrabInteractableConstraints
    {
        [field: SerializeField] public InteractableVisualSO openInteractableVisualSO { get; protected set; }
    }
}