using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public void DrawMap(Texture2D texture)
    {
        Util.RenderMap(textureRenderer, texture);
    }

    public void DrawMeshes(MeshDatas mesh, Texture2D texture)
    {
        meshFilter.sharedMesh = mesh.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

}
