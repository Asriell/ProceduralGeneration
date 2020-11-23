using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static float[,] CreatePerlinNoiseMap(int width, int height,float scale)
    {
        float[,] heightMap = new float[width, height];

        if (scale <= 0)
        {
            scale = 0.0001f;
        }
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float Scalei = i / scale;
                float Scalej = j / scale;
                heightMap[i, j] = Mathf.PerlinNoise(Scalei, Scalej);
            }
        }
        return heightMap;
    }
}
