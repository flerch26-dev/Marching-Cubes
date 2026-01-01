using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public bool flatShaded;
    public bool smoothTerrain;
    public float scale;
    public int seed;

    int chunksVisibleInViewDst = 3;
    int chunkSize = 10;

    public Material terrainMat;
    public Transform viewer;

    Vector3 viewerPosition;
    Dictionary<Vector3, Chunk> terrainChunkDictionary = new Dictionary<Vector3, Chunk>();
    public List<Chunk> terrainChunksVisibleLastUpdate = new List<Chunk>();

    public Texture2D[] terrainTextures;
    [HideInInspector]
    public Texture2DArray terrainTexArray;

    private void Awake()
    {
        ProjectSetup();
    }

    void ProjectSetup()
    {
        if (scale <= 0)
            scale = 0.0001f;

        PopulateTextureArray();
        seed = Random.Range(0, 1000);
    }

    private void Update()
    {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        viewerPosition = viewer.transform.position;
        int viewerChunkX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int viewerChunkY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
        int viewerChunkZ = Mathf.RoundToInt(viewerPosition.z / chunkSize);

        for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
        {
            for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
            {
                for (int zOffset = -chunksVisibleInViewDst; zOffset <= chunksVisibleInViewDst; zOffset++)
                {
                    Vector3 currentChunkCoord = new Vector3(viewerChunkX + xOffset, viewerChunkY + yOffset, viewerChunkZ + zOffset);

                    if (terrainChunkDictionary.ContainsKey(currentChunkCoord))
                    {
                        terrainChunkDictionary[currentChunkCoord].UpdateChunkVisibility(viewerPosition);
                        if (terrainChunkDictionary[currentChunkCoord].IsVisible())
                        {
                            terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[currentChunkCoord]);
                        }
                    }
                    else
                    {
                        terrainChunkDictionary.Add(currentChunkCoord, new Chunk(this, currentChunkCoord, chunkSize, scale, seed, terrainMat));
                    }
                }
            }
        }
    }

    void PopulateTextureArray()
    {
        terrainTexArray = new Texture2DArray(1024, 1024, terrainTextures.Length, TextureFormat.ARGB32, false);
        for (int i = 0; i < terrainTextures.Length; i++)
        {
            terrainTexArray.SetPixels(terrainTextures[i].GetPixels(0), i, 0);

        }
        terrainTexArray.Apply();
    }
}