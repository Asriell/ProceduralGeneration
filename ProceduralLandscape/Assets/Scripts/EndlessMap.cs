using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

/// <summary>
/// Streams an infinite terrain by dividing the world into square chunks.
/// Each frame, chunks within <see cref="maxViewDist"/> units of the viewer are activated;
/// chunks outside that radius are deactivated to save draw calls.
/// New chunks are created on demand and cached for subsequent visits.
/// </summary>
public class EndlessMap : MonoBehaviour
{
    const float ViewerMoveThresholdForChunkUpdate = 25f;
    const float SqrViewerMoveThresholdForChunkUpdate = ViewerMoveThresholdForChunkUpdate * ViewerMoveThresholdForChunkUpdate;
    //public const float maxViewDist = 500;
    public static float maxViewDist = 0;
    public LODInfo[] detailLevels;
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
    Vector2 viewerPositionOld;
    static Landscape mapGenerator;
    private int chunkSize;
    private int chunksVisibleInViewDist;

    private Dictionary<Vector2, Chunk> terrainChunkDictionary = new Dictionary<Vector2, Chunk>();
    private List<Chunk> chunksVisiblesLastUpdate = new List<Chunk>();

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDistThreshold;
    }
    public void Start()
    {
        mapGenerator = FindObjectOfType<Landscape>();
        maxViewDist = maxViewDist == 0 ? detailLevels[detailLevels.Length - 1].visibleDistThreshold : maxViewDist;
        chunkSize = Landscape.mapChunkSize - 1;
        chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);
        viewerPositionOld = viewerPosition;
        UpdateVisibleChunk();
    }

    public void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > SqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunk();
        }
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
                    terrainChunkDictionary.Add(viewedChunkCoord, new Chunk(viewedChunkCoord, chunkSize, detailLevels, this, transform));
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

        MapData mapData;
        bool mapDataReceived;

        int previousLODIndex = -1;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;

        public Chunk(Vector2 coords, int size, LODInfo[] detailLevels, EndlessMap map, Transform parent)
        {
            this.detailLevels = detailLevels;
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
            meshObject.transform.parent = parent;
            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateChunk);
            }

            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter.mesh = meshData.CreateMesh();
            meshRenderer.material = map.terrainMaterial != null
                ? new Material(map.terrainMaterial)
                : new Material(Shader.Find("Standard"));
            meshRenderer.material.mainTexture = texture;
            mapGenerator.RequestMapData(position,OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            //mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = Util.textureGenerator(mapData.colorMap, Landscape.mapChunkSize, Landscape.mapChunkSize, FilterMode.Point);
            meshRenderer.material.mainTexture = texture;
            UpdateChunk();
        }

        /*
        void OnMeshDataReceived(MeshDatas meshData)
        {
            meshFilter.mesh = meshData.CreateMesh();
        }*/

        /// <summary>Shows or hides the chunk based on the viewer's current distance to its bounds.</summary>
        public void UpdateChunk()
        {
            if (!mapDataReceived)
                return;
            float dist = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = dist <= maxViewDist;
            meshObject.SetActive(visible);

            if (visible)
            {
                int lodIndex = 0;
                for (int i = 0; i < detailLevels.Length-1; i++)
                {
                    if (dist > detailLevels[i].visibleDistThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }
                if (lodIndex != previousLODIndex)
                {
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    } else if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(mapData);
                    }
                }
            }
        }
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;

        int lod;

        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshDatas meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData,lod, OnMeshDataReceived);
        }
    }
}
