﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Landscape Generation
public class Landscape : MonoBehaviour
{
    #region Parameters
    public enum DrawMode { NoiseMap, ColorMap, Mesh}
    public DrawMode drawMode;

    [Header("HeightMap & Mesh Parameters")]
    public const int mapChunkSize = 241;//map size
    [Range(0,6)]
    public int levelOfDetail;//rate of meshes which are drawn
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
    #endregion
    //Map Generation and display
    public void Generate()
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
        MapDisplay display = FindObjectOfType<MapDisplay>();
        //display, depends of the drawmode.
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawMap(Util.textureGenerator(heightMap,FilterMode.Point));
        } else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawMap(Util.textureGenerator(mapColor,width,height,FilterMode.Point));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMeshes(Util.GenerateMesh(heightMap,heightRateMesh,heightCurve, levelOfDetail), Util.textureGenerator(mapColor, width, height, FilterMode.Point));
        }
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
}

//Landscape type, growing order
[System.Serializable]
public class LandscapeType
{
    public string name;
    public float height;//beetween 0 and 1 ! Growing order
    public Color color;
}
