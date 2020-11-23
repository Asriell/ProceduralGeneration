using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landscape : MonoBehaviour
{
    public int width;
    public int height;
    public float scale;
    public int octaves;
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
}
