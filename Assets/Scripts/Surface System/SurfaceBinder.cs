using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Surface_System
{
    [CreateAssetMenu(menuName = "Surface System/Surface Binder")]
    public class SurfaceBinder : ScriptableObject
    {
        [SerializeField] private List<SurfaceGroupSO> _surfaceGroups;

        [CanBeNull]
        public SurfaceData GetSurfaceData(Texture checkTexture)
        {
            foreach (var group in _surfaceGroups)
            {
                foreach (var textureSound in group.textureSounds)
                {
                    if (textureSound.texture == checkTexture)
                        return new SurfaceData(textureSound.texture, textureSound, group.surfaceImpactSound);
                }
            }

            return null;
        }
    }
}