using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Prints the changes to an object
public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    //2D HeighMap rendering
    public void DrawMap(Texture2D texture)
    {
        Util.RenderMap(textureRenderer, texture);
    }

    //3D Scene Rendering
    public void DrawMeshes(MeshDatas mesh, Texture2D texture)
    {
        meshFilter.sharedMesh = mesh.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }

}
