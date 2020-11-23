using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landscape : MonoBehaviour
{
    public int width;
    public int height;
    public float scale;

    public bool autoUpdate;

    public void Generate()
    {
        float[,] heightMap = Util.CreatePerlinNoiseMap(width, height, scale);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawMap(heightMap);
    }
}
