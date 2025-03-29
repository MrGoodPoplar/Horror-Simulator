using Audio_System;
using UnityEngine;

namespace Surface_System
{
    public record SurfaceData
    {
        public Texture texture;
        public Vector3 position;
        public readonly SurfaceImpactSound surfaceImpactSound;

        public SurfaceData(Texture texture, SurfaceImpactSound surfaceImpactSound)
        {
            this.texture = texture;
            this.surfaceImpactSound = surfaceImpactSound;
            
            position = Vector3.zero;
        }

        public SurfaceData SetPosition(Vector3 position)
        {
            this.position = position;
            return this;
        }
    }
}