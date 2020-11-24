using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(Landscape))]
public class ScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Landscape map = (Landscape)target;
        if (DrawDefaultInspector() && map.autoUpdate)
        {
            map.DrawMapInEditor();
        }

        if (GUILayout.Button("Generate !"))
        {
            map.DrawMapInEditor();
        }
    }
}
