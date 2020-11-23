using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landscape : MonoBehaviour
{

    public enum DrawMode { NoiseMap, ColorMap, Mesh}
    public DrawMode drawMode;

    [Header("HeightMap Parameters")]
    public int width;
    public int height;
    public float scale;
    public int octaves;
    [Range(0,1)]
    public float persistence;
    public float lacunarity;
    public int seed;
    public Vector2 offset;

    [Header("Landscape Parameter")]
    public LandscapeType[] landscapeType;


    public bool autoUpdate;

    public void Generate()
    {
        float[,] heightMap = Util.CreatePerlinNoiseMap(width, height, seed, scale, octaves, persistence, lacunarity, offset);

        Color[] mapColor = new Color[width*height];

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

        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawMap(Util.textureGenerator(heightMap,FilterMode.Point));
        } else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawMap(Util.textureGenerator(mapColor,width,height,FilterMode.Point));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMeshes(Util.GenerateMesh(heightMap), Util.textureGenerator(mapColor, width, height, FilterMode.Point));
        }
    }

    public void OnValidate()
    {
        if (width < 1)
        {
            width = 1;
        }
        if (height < 1)
        {
            height = 1;
        }
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


[System.Serializable]
public class LandscapeType
{
    public string name;
    public float height;
    public Color color;
}
