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

    public bool autoUpdate;

    public void Generate()
    {
        float[,] heightMap = Util.CreatePerlinNoiseMap(width, height, scale, octaves, persistence, lacunarity);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawMap(heightMap);
    }
}
