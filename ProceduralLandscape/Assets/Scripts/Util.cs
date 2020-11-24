using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


//Fonctions to create/render mesh and texture
public static class Util
{
    //Noise creation, for the disposition of elements (Vertice, pixels)
    public static float[,] CreatePerlinNoiseMap(int width, int height, int seed, float scale, int octaves = 0, float persistence = 1, float lacunarity = 1, Vector2? offset = null)
    {
        Vector2 offsetVal = offset ?? Vector2.zero;//optionnal element, (0,0) if no offset submitted
        float[,] heightMap = new float[width, height];

        System.Random rng = new System.Random(seed);//begins the random number generator to an integer fixed, to have the same number sequence each time.
        //offset submitted at each octaves
        Vector2[] octavesOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = rng.Next(-100000,100000) + offsetVal.x;
            float offsetY = rng.Next(-100000, 100000) + offsetVal.y;
            octavesOffsets[i] = new Vector2(offsetX, offsetY);
        }
        //size of elements
        if (scale <= 0)
        {
            scale = 0.0001f;
        }
        //interval of values for the whole heightmap
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        float halfWidth = width / 2f;//to scale on center
        float halfHeight = height / 2f;//to scale on center



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
                heightMap[i, j] = Mathf.InverseLerp(minHeight, maxHeight, heightMap[i, j]);//to rescale [-1,1] perlin values, into [0,1] height values (0 -> seas, 1-> mountains summits)
            }
        }
        return heightMap;//a grid with [0,1] values.
    }

    //Set a grid color to a texture
    public static Texture2D textureGenerator(Color[] colorMap, int width, int height,FilterMode filtermode = FilterMode.Bilinear)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = filtermode;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    //Creates a greyScale color array with a 2D Grid and generate the texture
    public static Texture2D textureGenerator(float [,] map, FilterMode mode = FilterMode.Bilinear)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        Color[] colorMap = new Color[width * height];

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                colorMap[j * width + i] = Color.Lerp(Color.black, Color.white, map[i, j]);//Linear Interpolation, to have a Color value beetween black and white, with a map element
            }
        }
        return textureGenerator(colorMap,width,height,mode);
    }

    //Render a texture into a Renderer
    public static void RenderMap(Renderer textureRenderer, Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(width, 1, height);
    }

    //Mesh generation, with a heightMap, heightRate, a curve, and a level of details (number of edges)
    public static MeshDatas GenerateMesh(float[,] heightMap,float heightRate = 1, AnimationCurve _heightCurve = null, int levelOfDetail = 0)
    {
        AnimationCurve heightCurve = _heightCurve!= null ? new AnimationCurve(_heightCurve.keys) : null;
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1f) / (-2f);
        float topLeftZ = (height - 1f) / 2f;


        int meshDetails = levelOfDetail * 2;//to have 1 edge of 1,2,4,6,8,10,12 drawn 
        if (meshDetails == 0)
        {
            meshDetails = 1;
        }

        int verticePerLine = (width - 1) / meshDetails + 1;
        int verticePerColumns = (height - 1) / meshDetails + 1;


        MeshDatas mesh = new MeshDatas(verticePerLine, verticePerColumns);
        int vertexIndex = 0;
        for (int j = 0; j < height; j+= meshDetails)// draw 1 edge of meshDetails on the heightMap
        {
            for (int i = 0; i < width; i+=meshDetails)
            {
                if (heightCurve == null)//vertice
                {
                    mesh.vertices[vertexIndex] = new Vector3(topLeftX + i, heightMap[i, j] * heightRate, topLeftZ - j);
                } else
                {
                    mesh.vertices[vertexIndex] = new Vector3(topLeftX + i, heightCurve.Evaluate(heightMap[i, j]) * heightRate, topLeftZ - j);
                }
                mesh.uvs[vertexIndex] = new Vector2(i / (float)width, j / (float)height);//textures

                if (i < width - 1 && j < height - 1)//edges
                {
                    mesh.AddTriangle(vertexIndex, vertexIndex + verticePerLine + 1, vertexIndex + verticePerLine);
                    mesh.AddTriangle(vertexIndex + verticePerLine + 1, vertexIndex, vertexIndex + 1);
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

    //Generate a Mesh with a meshDatas Oject
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
