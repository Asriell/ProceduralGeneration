using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

//Landscape Generation
public class Landscape : MonoBehaviour
{
    #region Parameters
    public enum DrawMode { NoiseMap, ColorMap, Mesh}
    public DrawMode drawMode;

    [Header("HeightMap & Mesh Parameters")]
    public const int mapChunkSize = 241;//map size
    [Range(0,6)]
    public int levelOfDetailOnEditor;//rate of meshes which are drawn
    private int width;
    private int height;
    public float scale;//height of the mesh
    public int octaves;//number of iterations on generation step
    [Range(0,1)]
    public float persistence;//density of elements, difference between high points and lower points (amplitude)
    public float lacunarity;//size of each elements
    public int seed;//for the noise generation
    public Vector2 offset;//map position
    public float heightRateMesh;//how much the elements will grow in the mesh
    public AnimationCurve heightCurve;//how much each element will be influenced in the mesh

    [Header("Landscape Parameter")]
    public LandscapeType[] landscapeType;//type of each element


    public bool autoUpdate;//editor auto update

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshDatas>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshDatas>>();
    #endregion

    public void DrawMapInEditor()
    {
        MapData mapData = DatasGeneration();
        MapDisplay display = FindObjectOfType<MapDisplay>();
        //display, depends of the drawmode.
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawMap(Util.textureGenerator(mapData.heightMap, FilterMode.Point));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawMap(Util.textureGenerator(mapData.mapColor, width, height, FilterMode.Point));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMeshes(Util.GenerateMesh(mapData.heightMap, heightRateMesh, heightCurve, levelOfDetailOnEditor), Util.textureGenerator(mapData.mapColor, width, height, FilterMode.Point));
        }
    }

    public void RequestMapData(Vector2 center,Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center,callback);
        };
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center,Action<MapData> callback)
    {
        MapData mapData = DatasGeneration(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshDatas(MapData mapData, int lod, Action<MeshDatas> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDatasThread(mapData,lod,callback);
        };
        new Thread(threadStart).Start();
    }

    public void MeshDatasThread(MapData mapData,int lod, Action<MeshDatas> callback)
    {
        MeshDatas meshDatas = Util.GenerateMesh(mapData.heightMap, heightRateMesh, heightCurve,lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshDatas>(callback, meshDatas));
        }
    }

    private void Update()
    {
        if(mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count;i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshDatas> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }
    //Map Generation and display
    public MapData DatasGeneration()
    {
        width = mapChunkSize;
        height = mapChunkSize;
        float[,] heightMap = Util.CreatePerlinNoiseMap(width, height, seed, scale, octaves, persistence, lacunarity, offset);

        Color[] mapColor = new Color[width*height];
        //color setup, depends of the landscape color
        for (int j = 0; j < height; j++) {
            for (int i = 0; i < width; i++)
            {
                float currentHeight = heightMap[i, j];
                for (int k = 0; k < landscapeType.Length; k++)
                {
                    if (currentHeight <= landscapeType[k].height)
                    {
                        mapColor[j * width + i] = landscapeType[k].color;
                        break;
                    }
                }
            }
        }
        return new MapData(heightMap, mapColor);
    }

    public MapData DatasGeneration(Vector2 center)
    {
        width = mapChunkSize;
        height = mapChunkSize;
        float[,] heightMap = Util.CreatePerlinNoiseMap(width, height, seed, scale, octaves, persistence, lacunarity, center + offset);

        Color[] mapColor = new Color[width * height];
        //color setup, depends of the landscape color
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                float currentHeight = heightMap[i, j];
                for (int k = 0; k < landscapeType.Length; k++)
                {
                    if (currentHeight <= landscapeType[k].height)
                    {
                        mapColor[j * width + i] = landscapeType[k].color;
                        break;
                    }
                }
            }
        }
        return new MapData(heightMap, mapColor);
    }

    //To only have authorized values
    public void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
        //persistence = Mathf.Clamp(persistence, 0f, 1f);
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action <T> _callback, T _parameter)
        {
            callback = _callback;
            parameter = _parameter;
        }
    }
}

//Landscape type, growing order
[System.Serializable]
public class LandscapeType
{
    public string name;
    public float height;//beetween 0 and 1 ! Growing order
    public Color color;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] mapColor;
    public MapData(float [,] _heightMap, Color[] _mapColor)
    {
        heightMap = _heightMap;
        mapColor = _mapColor;
    }
}