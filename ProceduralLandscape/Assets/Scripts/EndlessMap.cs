using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class EndlessMap : MonoBehaviour
{
    const float viewerMoveThreshOldForChunkUpdate = 25f;
    const float sqrViewerMoveThreshOldForChunkUpdate = 
        viewerMoveThreshOldForChunkUpdate* viewerMoveThreshOldForChunkUpdate;

    public static float maxViewDist;
    public Transform viewer;
    public Material mapMaterial;
    public Transform parent;
    static Landscape mapGenerator;
    public static Vector2 viewerPosition;
    private Vector2 OldViewerPosition;
    private int chunkSize;
    private int chunksVisibleInViewDist;

    private Dictionary<Vector2, Chunk> terrainChunkDictionary = new Dictionary<Vector2, Chunk>();
    List<Chunk> chunksVisiblesLastUpdate = new List<Chunk>();
    public LODInfos[] LODs;

    public void Start()
    {
        mapGenerator = FindObjectOfType<Landscape>();
        maxViewDist = LODs[LODs.Length - 1].visibleDistance;
        chunkSize = Landscape.mapChunkSize - 1;
        chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);
        UpdateVisibleChunk();
    }

    public void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        if ((OldViewerPosition - viewerPosition).sqrMagnitude > sqrViewerMoveThreshOldForChunkUpdate)
        {
            OldViewerPosition = viewerPosition;
            UpdateVisibleChunk();
        }
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
                    terrainChunkDictionary.Add(viewedChunkCoord, new Chunk(viewedChunkCoord,chunkSize,LODs,mapMaterial));
                }
                chunksVisiblesLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
            }
        }
    }
    public class Chunk
    {
        #region Chunk Attributes
        public GameObject mesh;
        public Vector2 position;
        public Bounds bounds;

        public MapData mapData;
        bool mapDataReceived;

        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;
        LODInfos[] infos;
        LODMesh[] LODMeshes;
        int previousLODIndex = -1;
        #endregion
        public Chunk(Vector2 coords, int size, LODInfos [] _infos, Material material,Transform parent = null)
        {
            infos = _infos;
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


            LODMeshes = new LODMesh[infos.Length];
            for (int i = 0; i < LODMeshes.Length; i++)
            {
                LODMeshes[i] = new LODMesh(infos[i].lod,UpdateChunk);
            }

            mapGenerator.RequestMapData(position,OnMapDataReceived);
        }

        public void OnMapDataReceived(MapData _mapData)
        {
            mapData = _mapData;
            mapDataReceived = true;

            Texture2D texture = Util.textureGenerator(mapData.mapColor, Landscape.mapChunkSize, Landscape.mapChunkSize, FilterMode.Point);
            meshRenderer.material.mainTexture = texture;

            UpdateChunk();
        }


        public void UpdateChunk()
        {
            
            if (!mapDataReceived)
            {
                return;
            }
            float distanceViewerFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = distanceViewerFromNearestEdge <= maxViewDist;

            if (visible)
            {
                int LODIndex = 0;
                for (int i = 0; i < infos.Length ; i++)
                {
                    if (distanceViewerFromNearestEdge > infos[i].visibleDistance)
                    {
                        LODIndex = i + 1;
                    } else
                    {
                        break;
                    }
                }
                if (previousLODIndex != LODIndex)
                {
                    LODMesh lodMesh = LODMeshes[LODIndex];
                    if (lodMesh.hasMesh)
                    {
                        previousLODIndex = LODIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(mapData);
                    }
                }
            }

            mesh.SetActive(visible);
        }
    }

    public class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updatecallBack;

        public LODMesh(int _lod, System.Action _updatecallBack)
        {
            lod = _lod;
            updatecallBack = _updatecallBack;
        }

        public void OnMeshDatasReceived(MeshDatas meshDatas)
        {
            mesh = meshDatas.CreateMesh();
            hasMesh = true;

            updatecallBack();
        }

        public void RequestMesh(MapData mapDatas)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshDatas(mapDatas,lod,OnMeshDatasReceived);
        }
    }

    [System.Serializable]
    public struct LODInfos
    {
        public int lod;
        public float visibleDistance;
    }
}
