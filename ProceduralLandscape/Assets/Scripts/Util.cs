using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static float[,] CreatePerlinNoiseMap(int width, int height,float scale, int octaves = 0, float persistence = 1, float lacunarity = 1)
    {
        float[,] heightMap = new float[width, height];

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float amplitude = 1;
                float frequency = 1;

                float noiseHeight = 0;

                for (int k = 0; k < octaves; k++)
                {
                    float Scalei = i / scale * frequency;
                    float Scalej = j / scale * frequency;
                    float perlinValue = Mathf.PerlinNoise(Scalei, Scalej) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxHeight)
                {
                    maxHeight = noiseHeight;
                } else if (noiseHeight < minHeight)
                {
                    minHeight = noiseHeight;
                }
                heightMap[i, j] = noiseHeight;
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                heightMap[i, j] = Mathf.InverseLerp(minHeight, maxHeight, heightMap[i, j]);
            }
        }
        return heightMap;
    }
}
