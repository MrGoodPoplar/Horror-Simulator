using System.Collections.Generic;
using Audio_System;
using UnityEngine;

namespace Surface_System
{
    [CreateAssetMenu(menuName = "Surface System/Surface Group")]
    public class SurfaceGroupSO : ScriptableObject
    {
        [field: SerializeField] public List<Texture> textures { get; private set; }
        [field: SerializeField] public SurfaceImpactSound surfaceImpactSound { get; private set; }
    }
}
