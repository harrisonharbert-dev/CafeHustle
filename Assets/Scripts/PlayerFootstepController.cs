using System;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerFootstepController : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float rayStartHeight = 0.5f;
    [SerializeField] private float rayDistance = 1.5f;
    [SerializeField] private LayerMask groundMask = ~0;


    [Serializable]
    private class TerrainLayerSound
    {
        public int layerIndex;
        public string audioID;
        public VisualEffect visualEffect;
    }

    [SerializeField] private List<TerrainLayerSound> terrainLayerSound = new List<TerrainLayerSound>();


    
    private string defaultAudioID;

    void Awake()
    {
        if (rayOrigin == null)
        {
            rayOrigin = transform;
        }
    }


    //https://www.youtube.com/watch?v=41xfLGtJpwk tutorial
    string ResolveFootstep()
    {
        Vector3 origin = rayOrigin.position + Vector3.up * rayStartHeight;

        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance + rayStartHeight, groundMask, QueryTriggerInteraction.Ignore))
        {
            return defaultAudioID;
        }

        Terrain terrain = hit.collider.GetComponent<Terrain>();
        if (terrain != null)
        {
            TerrainData data = terrain.terrainData;
            //convert world position to terrain local position
            Vector3 terrainPosition = hit.point - terrain.transform.position;

            //convert to alphamap coordinate
            int mapx = Mathf.FloorToInt(terrainPosition.x / data.size.x * data.alphamapWidth);
            int mapz = Mathf.FloorToInt(terrainPosition.z / data.size.z * data.alphamapHeight);

            //Get the texture blending for this pos
            float[,,] splatmap = data.GetAlphamaps(mapx, mapz, 1, 1);

            int textureIndex = 0;
            float strongest = 0f;

            //Find strongest texture
            for (int i = 0; i < splatmap.GetLength(2); i++)
            {
                if (splatmap[0, 0, i] > strongest)
                {
                    strongest = splatmap[0, 0, i];
                    textureIndex = i;
                }
            }

            // Check terrain layer sounds and find the one with the same index.
            foreach (TerrainLayerSound layerSound in terrainLayerSound)
            {
                if (layerSound.layerIndex == textureIndex)
                {
                    if (layerSound.visualEffect != null)
                    {
                        layerSound.visualEffect.SendEvent("OnPlay");
                    }
                    return layerSound.audioID;
                }
            }

            return defaultAudioID;
        }

        string hitTag = hit.collider.tag;
        return hitTag;
    }

    public void PlayFootstep()
    {
        if (SoundManager.instance == null) return;
        SoundManager.instance.PlaySound2D(ResolveFootstep());
    }


}
