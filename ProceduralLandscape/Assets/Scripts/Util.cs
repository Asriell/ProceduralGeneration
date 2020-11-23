using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static float[,] CreatePerlinNoiseMap(int width, int height, int seed, float scale, int octaves = 0, float persistence = 1, float lacunarity = 1, Vector2? offset = null)
    {
        Vector2 offsetVal = offset ?? Vector2.zero;
        float[,] heightMap = new float[width, height];

        System.Random rng = new System.Random(seed);

        Vector2[] octavesOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = rng.Next(-100000,100000) + offsetVal.x;
            float offsetY = rng.Next(-100000, 100000) + offsetVal.y;
            octavesOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;



        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float amplitude = 1;
                float frequency = 1;

                float noiseHeight = 0;

                for (int k = 0; k < octaves; k++)
                {
                    float Scalei = (i - halfWidth) / scale * frequency + octavesOffsets[k].x;
                    float Scalej = (j - halfHeight) / scale * frequency + octavesOffsets[k].y;
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

    public static Texture2D textureGenerator(Color[] colorMap, int width, int height,FilterMode filtermode = FilterMode.Bilinear)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = filtermode;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D textureGenerator(float [,] map, FilterMode mode = FilterMode.Bilinear)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        Color[] colorMap = new Color[width * height];

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                colorMap[j * width + i] = Color.Lerp(Color.black, Color.white, map[i, j]);
            }
        }
        return textureGenerator(colorMap,width,height,mode);
    }

}
