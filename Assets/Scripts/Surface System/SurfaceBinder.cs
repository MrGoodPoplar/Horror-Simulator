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
                foreach (var texture in group.textures)
                {
                    if (texture == checkTexture)
                        return new SurfaceData(texture, group.surfaceImpactSound);
                }
            }

            return null;
        }
    }
}