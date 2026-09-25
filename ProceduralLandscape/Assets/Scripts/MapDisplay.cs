using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Thin adapter between the generation pipeline and the scene's renderers.
/// Receives a texture or mesh produced by <see cref="Util"/> and forwards it
/// to the appropriate Unity renderer component.
/// </summary>
public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    /// <summary>Applies <paramref name="texture"/> to the flat preview quad and resizes it to match.</summary>
    public void DrawMap(Texture2D texture)
    {
        Util.RenderMap(textureRenderer, texture);
    }

    /// <summary>Uploads the finalised mesh and its colour texture to the scene's mesh renderers.</summary>
    public void DrawMeshes(MeshDatas mesh, Texture2D texture)
    {
        meshFilter.sharedMesh = mesh.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

}
