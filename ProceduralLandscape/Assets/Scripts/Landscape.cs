using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single-chunk terrain generator. Attach to a GameObject in the Editor scene.
/// Exposes all noise and mesh parameters in the Inspector and regenerates the preview
/// whenever <see cref="Generate"/> is called (or automatically when <see cref="autoUpdate"/> is on).
/// </summary>
public class Landscape : MonoBehaviour
{
    #region Parameters
    public enum DrawMode { NoiseMap, ColorMap, Mesh}
    public DrawMode drawMode;

    [Header("HeightMap & Mesh Parameters")]
    public const int mapChunkSize = 241;//map size
    [Range(0,6)]
    public int levelOfDetail;//rate of meshes which are drawn
    private int width;
    private int height;
    public float scale;//height of the mesh
    public int octaves;//number of iterations on generation step
    [Range(0,1)]
    public float persistence;//density of elements, difference between high points and lower points (amplitude)
    public float lacunarity;//size of each elements
    public int seed;//for the noise generation
    public Vector2 offset;//map position
    public float heightRateMesh;//how much the elements will grow in the mesh
    public AnimationCurve heightCurve;//how much each element will be influenced in the mesh

    [Header("Landscape Parameter")]
    public LandscapeType[] landscapeType;//type of each element


    public bool autoUpdate;//editor auto update
    #endregion
    /// <summary>
    /// Samples a Perlin noise heightmap, assigns biome colours per pixel, then renders
    /// either a flat noise preview, a colour map, or a full 3-D mesh depending on <see cref="drawMode"/>.
    /// </summary>
    public void Generate()
    {
        width = mapChunkSize;
        height = mapChunkSize;
        float[,] heightMap = Util.CreatePerlinNoiseMap(width, height, seed, scale, octaves, persistence, lacunarity, offset);

        Color[] mapColor = new Color[width*height];
        //color setup, depends of the landscape color
        for (int j = 0; j < height; j++) {
            for (int i = 0; i < width; i++)
            {
                float currentHeight = heightMap[i, j];
                for (int k = 0; k < landscapeType.Length; k++)
                {
                    if (currentHeight <= landscapeType[k].height)
                    {
                        mapColor[j * width + i] = landscapeType[k].color;
                        break;
                    }
                }
            }
        }
        MapDisplay display = FindObjectOfType<MapDisplay>();
        //display, depends of the drawmode.
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawMap(Util.textureGenerator(heightMap,FilterMode.Point));
        } else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawMap(Util.textureGenerator(mapColor,width,height,FilterMode.Point));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMeshes(Util.GenerateMesh(heightMap,heightRateMesh,heightCurve, levelOfDetail), Util.textureGenerator(mapColor, width, height, FilterMode.Point));
        }
    }

    /// <summary>Clamps Inspector parameters to valid ranges so the noise algorithm never receives illegal inputs.</summary>
    public void OnValidate()
    {
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

/// <summary>
/// Defines one biome layer: an upper height threshold and the colour applied to all
/// pixels at or below it. Entries must be ordered from lowest to highest <see cref="height"/>.
/// </summary>
[System.Serializable]
public class LandscapeType
{
    public string name;
    public float height;//beetween 0 and 1 ! Growing order
    public Color color;
}
