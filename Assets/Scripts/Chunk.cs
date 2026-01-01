using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Chunk
{
    bool smoothTerrain;
    bool flatShaded;
    Vector3 chunkPosition;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    MeshFilter meshFilter;
    //MeshCollider meshCollider;
    MeshRenderer meshRenderer;

    float terrainSurface = 0.5f;
    int size = 10;
    float scale = 30;
    int seed;
    Bounds bounds;

    TerrainPoint[,,] terrainMap;
    WorldGenerator worldGenerator;
    GameObject chunk;
    Material terrainMat;

    Vector3Int[] CornerTable = MarchingCubesTables.CornerTable;
    int[,] EdgeIndexes = MarchingCubesTables.EdgeIndexes;
    int[,] TriangleTable = MarchingCubesTables.TriangleTable;

    public Chunk(WorldGenerator worldGen, Vector3 chunkPos, int chunkSize, float scale, int seed, Material terrainMat)
    {
        ClearMeshData();
        this.worldGenerator = worldGen;
        this.chunkPosition = chunkPos;
        this.smoothTerrain = worldGen.smoothTerrain;
        this.flatShaded = worldGen.flatShaded;
        this.size = chunkSize;
        this.terrainMat = terrainMat;
        this.scale = scale;
        this.seed = seed;

        bounds = new Bounds(chunkPos * chunkSize, Vector3.one * chunkSize);

        GenerateGameObject(worldGen.transform);
    }

    public void GenerateGameObject(Transform parent)
    {
        chunk = new GameObject("Chunk");
        chunk.transform.position = chunkPosition * size;

        meshFilter = chunk.AddComponent<MeshFilter>();
        //meshCollider = chunk.AddComponent<MeshCollider>();
        meshRenderer = chunk.AddComponent<MeshRenderer>();

        meshRenderer.material = terrainMat;
        meshRenderer.material.SetTexture("_TexArr", worldGenerator.terrainTexArray);

        chunk.transform.tag = "Terrain";
        terrainMap = new TerrainPoint[size + 1, size + 1, size + 1];
        chunk.transform.parent = parent;

        Thread populateThread = new Thread(PopulateTerrainMap);
        populateThread.Start();
        populateThread.Join();
        CreateMeshData();
    }

    public void UpdateChunkVisibility(Vector3 viewerPos)
    {
        float dstToViewer = Mathf.Sqrt(bounds.SqrDistance(viewerPos));
        bool visible = dstToViewer <= 200;
        SetVisible(visible);
    }

    public void SetVisible(bool visible)
    {
        chunk.SetActive(visible);
    }

    public bool IsVisible()
    {
        return chunk.activeSelf;
    }

    void PopulateTerrainMap()
    {
        for (int x = 0; x < size + 1; x++)
        {
            for (int y = 0; y < size + 1; y++)
            {
                for (int z = 0; z < size + 1; z++)
                {
                    Vector3 internalCoords = new Vector3(x, y, z);

                    //float Noise2D = Noise.Noise2D(chunkPosition * size + internalCoords, scale, seed);
                    float Noise3D = Noise.Noise3D(chunkPosition * size + internalCoords, scale, seed);

                    float value = chunkPosition.y >= 0 ? 0 : Noise3D;
                    terrainMap[x, y, z] = new TerrainPoint(value, 0);
                }
            }
        }
    }

    void CreateMeshData()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    MarchCube(new Vector3Int(x, y, z));
                }
            }
        }

        BuildMesh();
    }

    int GetCubeConfiguration(float[] cube)
    {
        int configurationIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cube[i] > terrainSurface)
                configurationIndex |= 1 << i;
        }
        return configurationIndex; 
    }

    void MarchCube(Vector3Int position)
    {
        float[] cube = new float[8];
        for (int i = 0; i < 8; i++)
        {
            cube[i] = SampleTerrain(position + CornerTable[i]);
        }

        int configIndex = GetCubeConfiguration(cube);

        if (configIndex == 0 || configIndex == 255)  { return; }

        int edgeIndex = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int p = 0; p < 3; p++)
            {
                int indice = TriangleTable[configIndex, edgeIndex];

                if (indice == -1) { return; }

                Vector3 vert1 = position + CornerTable[EdgeIndexes[indice, 0]];
                Vector3 vert2 = position + CornerTable[EdgeIndexes[indice, 1]];

                Vector3 vertPosition;

                if (smoothTerrain)
                {
                    float vert1Sample = cube[EdgeIndexes[indice, 0]];
                    float vert2Sample = cube[EdgeIndexes[indice, 1]];

                    float difference = vert2Sample - vert1Sample;

                    if (difference == 0)
                        difference = terrainSurface;
                    else
                        difference = (terrainSurface - vert1Sample) / difference;

                    vertPosition = vert1 + ((vert2 - vert1) * difference);
                }
                else
                {
                    vertPosition = (vert1 + vert2) / 2f;
                }

                if (flatShaded)
                {
                    vertices.Add(vertPosition);
                    triangles.Add(vertices.Count - 1);
                }
                else
                    triangles.Add(VertForIndice(vertPosition, position));

                edgeIndex++;
            }
        }
    }

    float SampleTerrain (Vector3Int point)
    {
        return terrainMap[point.x, point.y, point.z].dstToSurface;
    }

    int VertForIndice (Vector3 vert, Vector3Int point)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if (vertices[i] == vert)
                return i;
        }

        vertices.Add(vert);
        uvs.Add(new Vector2(terrainMap[point.x, point.y, point.z].textureID, 0));
        return vertices.Count - 1; 
    }

    void ClearMeshData()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    void BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        //meshCollider.sharedMesh = mesh;
    }
}