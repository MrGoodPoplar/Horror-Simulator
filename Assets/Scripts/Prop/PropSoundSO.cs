using Audio_System;
using UnityEngine;

namespace Prop
{
    [CreateAssetMenu(menuName = "Props/Sound")]
    public class PropSoundSO : ScriptableObject
    {
        [field: SerializeField] public ArraySoundData impactSounds { get; private set; }
    }
}