using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    public static void RenderMap(Renderer textureRenderer, Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(width, 1, height);
    }

    public static MeshDatas GenerateMesh(float[,] heightMap,float heightRate = 1, AnimationCurve heightCurve = null)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1f) / (-2f);
        float topLeftZ = (height - 1f) / 2f;
        MeshDatas mesh = new MeshDatas(width, height);

        int vertexIndex = 0;
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                if (heightCurve == null)
                {
                    mesh.vertices[vertexIndex] = new Vector3(topLeftX + i, heightMap[i, j] * heightRate, topLeftZ - j);
                } else
                {
                    mesh.vertices[vertexIndex] = new Vector3(topLeftX + i, heightCurve.Evaluate(heightMap[i, j]) * heightRate, topLeftZ - j);
                }
                mesh.uvs[vertexIndex] = new Vector2(i / (float)width, j / (float)height);

                if (i < width - 1 && j < height - 1)
                {
                    mesh.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                    mesh.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }
        return mesh;
    }
}

public class MeshDatas
{
    public Vector3[] vertices;
    public List<int> triangles;
    public Vector2[] uvs;

    public MeshDatas(int width, int height)
    {
        vertices = new Vector3[width * height];
        uvs = new Vector2[width * height];
        triangles = new List<int>();
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
} 
