using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Surface_System
{
    public class SurfaceManager : MonoBehaviour
    {
        [Header("Constraints")]
        [SerializeField] private SurfaceBinder _surfaceBinder;

        [CanBeNull]
        public SurfaceData GetImpactDetails(Collision collision)
        {
            if (collision.transform.TryGetComponent(out Terrain terrain))
            {
                Vector3 hitPosition = GetHitPosition(collision);
                return HandleTerrainImpact(terrain, hitPosition)
                    ?.SetPosition(hitPosition);
            }

            if (collision.transform.TryGetComponent(out Renderer renderer))
            {
                return HandleRenderImpact(renderer)
                    ?.SetPosition(GetHitPosition(collision));
            }
            
            return null;
        }
        
        [CanBeNull]
        public SurfaceData GetImpactDetails(RaycastHit hit)
        {
            if (hit.transform.TryGetComponent(out Terrain terrain))
                return HandleTerrainImpact(terrain, hit.point)
                    ?.SetPosition(hit.point);

            if (hit.transform.TryGetComponent(out Renderer renderer))
                return HandleRenderImpact(renderer)
                    ?.SetPosition(hit.point);

            return null;
        }
        
        [CanBeNull]
        private SurfaceData HandleRenderImpact(Renderer renderer)
        {
            return _surfaceBinder.GetSurfaceData(renderer.material.mainTexture);
        }
        
        private SurfaceData HandleTerrainImpact(Terrain terrain, Vector3 hitPosition)
        {
            Texture texture = GetTerrainTextureAtPosition(terrain, hitPosition);
            return _surfaceBinder.GetSurfaceData(texture)?.SetPosition(hitPosition);
        }

        private Vector3 GetHitPosition(Collision collision)
        {
            return collision.GetContact(0).point;
        }
        
        private Texture GetTerrainTextureAtPosition(Terrain terrain, Vector3 worldPosition)
        {
            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainPosition = terrain.transform.position;

            int mapX = Mathf.FloorToInt((worldPosition.x - terrainPosition.x) / terrainData.size.x * terrainData.alphamapWidth);
            int mapZ = Mathf.FloorToInt((worldPosition.z - terrainPosition.z) / terrainData.size.z * terrainData.alphamapHeight);

            float[,,] alphaMap = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
            int dominantTextureIndex = GetDominantTextureIndex(terrainData, alphaMap);

            return terrainData.terrainLayers[dominantTextureIndex].diffuseTexture;
        }

        private int GetDominantTextureIndex(TerrainData terrainData, float[,,] alphaMap)
        {
            int dominantTextureIndex = 0;
            float maxAlpha = 0f;
            
            for (int i = 0; i < terrainData.alphamapLayers; i++)
            {
                if (alphaMap[0, 0, i] > maxAlpha)
                {
                    maxAlpha = alphaMap[0, 0, i];
                    dominantTextureIndex = i;
                }
            }

            return dominantTextureIndex;
        }
    }
}
