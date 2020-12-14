//Endless terrain managing class
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class EndlessMap : MonoBehaviour //Main class of the endless terrain
{
    #region Main class parameters
    const float viewerMoveThreshOldForChunkUpdate = 25f; //minimum distance for an update
    const float sqrViewerMoveThreshOldForChunkUpdate = 
        viewerMoveThreshOldForChunkUpdate* viewerMoveThreshOldForChunkUpdate; //the square of the previous value

    public static float maxViewDist; // visible chunks value
    public Transform viewer; //the object in the scene
    public Material mapMaterial; //the colored height map
    public Transform parent; //optionnal, to put the chunks in an object.
    static Landscape mapGenerator; //compute the terrain
    public static Vector2 viewerPosition; //position of the object in the scene
    private Vector2 OldViewerPosition; //previous position of the object in the scene
    private int chunkSize; //size of each chunks. Chunks are squares.
    private int chunksVisibleInViewDist; //index of visible chunks.

    private Dictionary<Vector2, Chunk> terrainChunkDictionary = new Dictionary<Vector2, Chunk>(); //chunks already computed
    List<Chunk> chunksVisiblesLastUpdate = new List<Chunk>(); //list of visibke chunks, clear this before chunk updating, to refresh the map
    public LODInfos[] LODs; //informations for the number of polygons to compute for each chunk, in function of the distance with the viewer, sorted by distance and level of details.
    #endregion

    #region Update functions
    public void Start()
    {
        mapGenerator = FindObjectOfType<Landscape>(); //initialization of the object which computes the chunks
        maxViewDist = LODs[LODs.Length - 1].visibleDistance; //the last value of the array is the max distance to see chunks.
        chunkSize = Landscape.mapChunkSize - 1;
        chunksVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);//position of the farest visible chunk.
        UpdateVisibleChunk();//initialize the terrain
    }

    public void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        if ((OldViewerPosition - viewerPosition).sqrMagnitude > sqrViewerMoveThreshOldForChunkUpdate)//if the viewer has moved sufficiently
        {
            OldViewerPosition = viewerPosition;//current position becomes the old position
            UpdateVisibleChunk();//compute chunks
        }
    }


    public void UpdateVisibleChunk()
    {

        foreach(Chunk chunk in chunksVisiblesLastUpdate)//disable the chunks (refreshing)
        {
            chunk.mesh.SetActive(false);
        }
        chunksVisiblesLastUpdate.Clear();
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);//position of the current viewer's chunk. (x,y)
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int j = -chunksVisibleInViewDist; j <=chunksVisibleInViewDist; j++ ) //max chunk around the viewer
        {
            for (int i = -chunksVisibleInViewDist; i <= chunksVisibleInViewDist; i++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + i, currentChunkCoordY + j); //vector of each chunks around the viewer 
                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateChunk(); // display of the chunk.
                } else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new Chunk(viewedChunkCoord,chunkSize,LODs,mapMaterial)); //save the chunk to not compute it each time
                }
                chunksVisiblesLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]); //to refresh chunk
            }
        }
    }
    #endregion
    public class Chunk //class for each chunk
    {
        #region Chunk Attributes
        public GameObject mesh; //mesh of the chunk
        public Vector2 position; // its position in the world
        public Bounds bounds; //its edges

        public MapData mapData; //computed map
        bool mapDataReceived; 

        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;
        LODInfos[] infos; //same as main class's LODs 
        LODMesh[] LODMeshes; //set of meshes in function of the level of detail.
        int previousLODIndex = -1; //number of polygons at last update
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
                LODMeshes[i] = new LODMesh(infos[i].lod,UpdateChunk); //create an array with the same size as info
            }

            mapGenerator.RequestMapData(position,OnMapDataReceived); //compute the height map
        }

        public void OnMapDataReceived(MapData _mapData) //called when the Height map is received
        {
            mapData = _mapData;
            mapDataReceived = true;

            Texture2D texture = Util.textureGenerator(mapData.mapColor, Landscape.mapChunkSize, Landscape.mapChunkSize, FilterMode.Point);
            meshRenderer.material.mainTexture = texture; //apply the computed height map as a texture.

            UpdateChunk();
        }


        public void UpdateChunk()
        {
            
            if (!mapDataReceived)
            {
                return;
            }
            float distanceViewerFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = distanceViewerFromNearestEdge <= maxViewDist; // Can the viewer see the chunk ?

            if (visible)
            {
                int LODIndex = 0;
                for (int i = 0; i < infos.Length ; i++) //look for its level of details.
                {
                    if (distanceViewerFromNearestEdge > infos[i].visibleDistance)
                    {
                        LODIndex = i + 1;
                    } else
                    {
                        break;
                    }
                }
                if (previousLODIndex != LODIndex) //if the LOD has to be changed
                {
                    LODMesh lodMesh = LODMeshes[LODIndex];
                    if (lodMesh.hasMesh) //set the mesh if already computed 
                    {
                        previousLODIndex = LODIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    if (!lodMesh.hasRequestedMesh) //request the mesh if necessary
                    {
                        lodMesh.RequestMesh(mapData);
                    }
                }
            }

            mesh.SetActive(visible); //active visible meshes
        }
    }

    public class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updatecallBack; //Update chunks callback

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
