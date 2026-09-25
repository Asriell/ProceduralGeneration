using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Streams an infinite terrain by dividing the world into square chunks.
/// Each frame, chunks within <see cref="maxViewDist"/> units of the viewer are activated;
/// chunks outside that radius are deactivated to save draw calls.
/// New chunks are created on demand and cached for subsequent visits.
/// </summary>
public class EndlessMap : MonoBehaviour
{
    public const float maxViewDist = 300;
    public Transform viewer;
    public Material terrainMaterial;

    [Header("Terrain Parameters")]
    public float scale = 150f;
    public int octaves = 4;
    [Range(0, 1)] public float persistence = 0.5f;
    public float lacunarity = 2f;
    public int seed;
    public float heightRateMesh = 30f;
    public AnimationCurve heightCurve;
    public LandscapeType[] landscapeType;

    public static Vector2 viewerPosition;
    private int chunkSize;
    private int chunksVisibleInViewDist;

    private Dictionary<Vector2, Chunk> terrainChunkDictionary = new Dictionary<Vector2, Chunk>();
    private List<Chunk> chunksVisiblesLastUpdate = new List<Chunk>();

    public void Start()
    {
        chunkSize = Landscape.mapChunkSize - 1;
        chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);
    }

    public void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunk();
    }

    /// <summary>
    /// Hides all previously visible chunks, then activates or creates every chunk
    /// whose grid coordinates fall within the viewer's visible range.
    /// </summary>
    public void UpdateVisibleChunk()
    {
        foreach (Chunk chunk in chunksVisiblesLastUpdate)
            chunk.meshObject.SetActive(false);
        chunksVisiblesLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int j = -chunksVisibleInViewDist; j <= chunksVisibleInViewDist; j++)
        {
            for (int i = -chunksVisibleInViewDist; i <= chunksVisibleInViewDist; i++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + i, currentChunkCoordY + j);
                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateChunk();
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new Chunk(viewedChunkCoord, chunkSize, this));
                }
                chunksVisiblesLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
            }
        }
    }

    /// <summary>
    /// Represents one terrain tile. On construction it generates its heightmap, colour
    /// map, mesh and texture, then spawns a GameObject at the correct world position.
    /// </summary>
    public class Chunk
    {
        public GameObject meshObject;
        public Vector2 position;
        public Bounds bounds;

        public Chunk(Vector2 coords, int size, EndlessMap map)
        {
            position = coords * size;
            bounds = new Bounds(position, Vector2.one * size);

            // offset so each chunk samples a unique but seamlessly adjacent region
            Vector2 noiseOffset = new Vector2(position.x / map.scale, position.y / map.scale);
            float[,] heightMap = Util.CreatePerlinNoiseMap(
                Landscape.mapChunkSize, Landscape.mapChunkSize,
                map.seed, map.scale, map.octaves, map.persistence, map.lacunarity, noiseOffset);

            Color[] colorMap = new Color[Landscape.mapChunkSize * Landscape.mapChunkSize];
            for (int j = 0; j < Landscape.mapChunkSize; j++)
                for (int i = 0; i < Landscape.mapChunkSize; i++)
                    for (int k = 0; k < map.landscapeType.Length; k++)
                        if (heightMap[i, j] <= map.landscapeType[k].height)
                        {
                            colorMap[j * Landscape.mapChunkSize + i] = map.landscapeType[k].color;
                            break;
                        }

            MeshDatas meshData = Util.GenerateMesh(heightMap, map.heightRateMesh, map.heightCurve, 0);
            Texture2D texture = Util.textureGenerator(colorMap, Landscape.mapChunkSize, Landscape.mapChunkSize, FilterMode.Point);

            meshObject = new GameObject("Chunk " + coords);
            meshObject.transform.position = new Vector3(position.x, 0, position.y);
            MeshFilter mf = meshObject.AddComponent<MeshFilter>();
            MeshRenderer mr = meshObject.AddComponent<MeshRenderer>();
            mf.mesh = meshData.CreateMesh();
            mr.material = map.terrainMaterial != null
                ? new Material(map.terrainMaterial)
                : new Material(Shader.Find("Standard"));
            mr.material.mainTexture = texture;
        }

        /// <summary>Shows or hides the chunk based on the viewer's current distance to its bounds.</summary>
        public void UpdateChunk()
        {
            float dist = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            meshObject.SetActive(dist <= maxViewDist);
        }
    }
}
