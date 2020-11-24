using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class EndlessMap : MonoBehaviour
{
    public const float maxViewDist = 300;
    public Transform viewer;
    public Material mapMaterial;
    public Transform parent;
    static Landscape mapGenerator;
    public static Vector2 viewerPosition;
    private int chunkSize;
    private int chunksVisibleInViewDist;

    private Dictionary<Vector2, Chunk> terrainChunkDictionary = new Dictionary<Vector2, Chunk>();
    List<Chunk> chunksVisiblesLastUpdate = new List<Chunk>();

    public void Start()
    {
        mapGenerator = FindObjectOfType<Landscape>();
        chunkSize = Landscape.mapChunkSize - 1;
        chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);
    }

    public void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunk();
    }


    public void UpdateVisibleChunk()
    {

        foreach(Chunk chunk in chunksVisiblesLastUpdate)
        {
            chunk.mesh.SetActive(false);
        }
        chunksVisiblesLastUpdate.Clear();
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int j = -chunksVisibleInViewDist; j <=chunksVisibleInViewDist; j++ )
        {
            for (int i = -chunksVisibleInViewDist; i <= chunksVisibleInViewDist; i++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + i, currentChunkCoordY + j);
                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateChunk();
                } else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new Chunk(viewedChunkCoord,chunkSize,mapMaterial));
                }
                chunksVisiblesLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
            }
        }
    }
    public class Chunk
    {
        public GameObject mesh;
        public Vector2 position;
        public Bounds bounds;

        public MapData mapData;

        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;

        public Chunk(Vector2 coords, int size, Material material,Transform parent = null)
        {
            position = coords * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionVector3 = new Vector3(position.x, 0, position.y);
            mesh = new GameObject("Chunk");
            meshRenderer = mesh.AddComponent<MeshRenderer>();
            meshFilter = mesh.AddComponent<MeshFilter>();
            meshRenderer.material = material;
            mesh.transform.position = positionVector3;
            
            if (parent != null)
            {
                mesh.transform.parent = parent;
            }

            mapGenerator.RequestMapData(OnMapDataReceived);
        }

        public void OnMapDataReceived(MapData mapData)
        {
            mapGenerator.RequestMeshDatas(mapData, OnMeshDataReceived);
        }

        public void OnMeshDataReceived(MeshDatas meshDatas)
        {
            meshFilter.mesh = meshDatas.CreateMesh();
        }

        public void UpdateChunk()
        {
            float distanceViewerFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = distanceViewerFromNearestEdge <= maxViewDist;
            mesh.SetActive(visible);
        }
    }
}
