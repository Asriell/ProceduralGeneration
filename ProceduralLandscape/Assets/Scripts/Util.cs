using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// Static toolkit that centralises all procedural-generation primitives:
/// noise sampling, texture baking, mesh construction and rendering helpers.
/// All methods are stateless – pass data in, get results out.
/// </summary>
public static class Util
{
    public enum NormalizationMode
    {
        Local,
        Global
    }
    /// <summary>
    /// Generates a 2D Perlin noise heightmap by stacking <paramref name="octaves"/> noise layers
    /// (FBM), then normalises the result to [0, 1].
    /// </summary>
    /// <param name="width">Number of columns in the output grid.</param>
    /// <param name="height">Number of rows in the output grid.</param>
    /// <param name="seed">Determines the random per-octave offsets; identical seeds produce identical maps.</param>
    /// <param name="scale">Zoom level of the noise field – larger values = broader, smoother features.</param>
    /// <param name="octaves">How many noise layers to stack; more layers add finer surface detail.</param>
    /// <param name="persistence">Amplitude factor applied each octave (0-1): lower = softer, flatter terrain.</param>
    /// <param name="lacunarity">Frequency factor applied each octave (≥1): higher = more high-frequency detail.</param>
    /// <param name="offset">World-space shift used to sample a different region of the noise field.</param>
    /// <returns>A [width, height] array with values remapped to [0, 1] (0 = lowest point, 1 = highest).</returns>
    public static float[,] CreatePerlinNoiseMap(int width, int height, int seed, float scale, int octaves = 0, float persistence = 1, float lacunarity = 1, Vector2? offset = null, NormalizationMode normalizationMode = NormalizationMode.Local)
    {
        float amplitude = 1;
        float frequency = 1;

        float noiseHeight = 0;
        Vector2 offsetVal = offset ?? Vector2.zero;//optionnal element, (0,0) if no offset submitted
        float[,] heightMap = new float[width, height];

        System.Random rng = new System.Random(seed);//begins the random number generator to an integer fixed, to have the same number sequence each time.
        //offset submitted at each octaves
        Vector2[] octavesOffsets = new Vector2[octaves];
        float maxPossibleHeight = 0;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = rng.Next(-100000,100000) + offsetVal.x;
            float offsetY = rng.Next(-100000, 100000) - offsetVal.y;
            octavesOffsets[i] = new Vector2(offsetX, offsetY);
            maxPossibleHeight += amplitude;
            amplitude *= persistence;
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
                amplitude = 1;
                frequency = 1;

                noiseHeight = 0;

                for (int k = 0; k < octaves; k++)
                {
                    float Scalei = (i - halfWidth + octavesOffsets[k].x) / scale * frequency ;
                    float Scalej = (j - halfHeight + octavesOffsets[k].y) / scale * frequency ;
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
                if (normalizationMode == NormalizationMode.Local)
                {
                    heightMap[i, j] = Mathf.InverseLerp(minHeight, maxHeight, heightMap[i, j]);//to rescale [-1,1] perlin values, into [0,1] height values (0 -> seas, 1-> mountains summits)
                }
                else if (normalizationMode == NormalizationMode.Global)
                {
                    heightMap[i, j] = (heightMap[i, j] + 1) / (2f * maxPossibleHeight);//Global normalization assuming perlin noise is in [-1,1]
                }
            }
        }
        return heightMap;//a grid with [0,1] values.
    }

    /// <summary>
    /// Bakes a flat colour array into a <see cref="Texture2D"/> with clamped wrapping.
    /// </summary>
    /// <param name="colorMap">Row-major colour array of length width × height.</param>
    /// <param name="width">Texture width in pixels.</param>
    /// <param name="height">Texture height in pixels.</param>
    /// <param name="filtermode">GPU filter applied when the texture is scaled on screen.</param>
    public static Texture2D textureGenerator(Color[] colorMap, int width, int height,FilterMode filtermode = FilterMode.Bilinear)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = filtermode;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    /// <summary>
    /// Converts a 2D float heightmap to a greyscale <see cref="Texture2D"/> (0 = black, 1 = white).
    /// Delegates to <see cref="textureGenerator(Color[],int,int,FilterMode)"/> after building the colour array.
    /// </summary>
    /// <param name="map">Heightmap with values in [0, 1].</param>
    /// <param name="mode">GPU filter applied when the texture is scaled on screen.</param>
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

    /// <summary>
    /// Assigns <paramref name="texture"/> to a renderer's shared material and resizes the
    /// GameObject's local scale so one unit = one texel (useful for flat preview quads).
    /// </summary>
    public static void RenderMap(Renderer textureRenderer, Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(width, 1, height);
    }

    /// <summary>
    /// Builds a quad-grid mesh from a heightmap with optional LOD decimation and curve remapping.
    /// </summary>
    /// <param name="heightMap">Source elevation data; values expected in [0, 1].</param>
    /// <param name="heightRate">Vertical scale multiplier applied to every vertex.</param>
    /// <param name="heightCurve">
    /// Optional animation curve that remaps [0,1] height values before scaling,
    /// allowing non-linear elevation profiles (e.g. flat water, steep cliffs).
    /// Pass <c>null</c> to use a linear mapping.
    /// </param>
    /// <param name="levelOfDetail">
    /// 0 = full resolution; 1-6 = increasingly coarse (every 2, 4, 6, 8, 10, 12 vertices sampled).
    /// </param>
    /// <returns>A <see cref="MeshDatas"/> ready to be finalised with <see cref="MeshDatas.CreateMesh"/>.</returns>
    public static MeshDatas GenerateMesh(float[,] heightMap,float heightRate = 1, AnimationCurve _heightCurve = null, int levelOfDetail = 0)
    {
        AnimationCurve heightCurve = _heightCurve != null ? new AnimationCurve(_heightCurve.keys) : null;
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

/// <summary>
/// Intermediate container that accumulates raw mesh data (vertices, triangles, UVs)
/// before it is committed to a Unity <see cref="Mesh"/> via <see cref="CreateMesh"/>.
/// Pre-allocates arrays to avoid per-vertex allocations during mesh generation.
/// </summary>
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

    /// <summary>Appends one triangle by its three vertex indices (counter-clockwise winding).</summary>
    public void AddTriangle(int a, int b, int c)
    {
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);
    }

    /// <summary>Finalises the accumulated data into a Unity <see cref="Mesh"/> with recalculated normals.</summary>
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
