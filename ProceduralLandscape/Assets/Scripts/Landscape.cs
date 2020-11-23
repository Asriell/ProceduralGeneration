using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landscape : MonoBehaviour
{
    public int width;
    public int height;
    public float scale;
    public int octaves;
    [Range(0,1)]
    public float persistence;
    public float lacunarity;


    public int seed;
    public Vector2 offset;


    public bool autoUpdate;

    public void Generate()
    {
        float[,] heightMap = Util.CreatePerlinNoiseMap(width, height, seed, scale, octaves, persistence, lacunarity, offset);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawMap(heightMap);
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
