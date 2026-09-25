using UnityEngine;
using UnityEditor;

public static class SceneSetup
{
    [MenuItem("Tools/Setup Endless Landscape Scene")]
    public static void Setup()
    {
        // Camera
        GameObject camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        Camera cam = camGO.AddComponent<Camera>();
        cam.farClipPlane = 500f;
        camGO.AddComponent<AudioListener>();
        var flyCamType = System.Type.GetType("FlyCamera, Assembly-CSharp");
        if (flyCamType != null) camGO.AddComponent(flyCamType);
        camGO.transform.position = new Vector3(0, 80, 0);
        camGO.transform.rotation = Quaternion.Euler(30, 0, 0);

        // EndlessMap
        GameObject mapGO = new GameObject("EndlessMap");
        EndlessMap endlessMap = mapGO.AddComponent<EndlessMap>();
        endlessMap.viewer = camGO.transform;

        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Material/MeshMaterial.mat");
        endlessMap.terrainMaterial = mat;

        endlessMap.scale = 150f;
        endlessMap.octaves = 4;
        endlessMap.persistence = 0.5f;
        endlessMap.lacunarity = 2f;
        endlessMap.seed = 42;
        endlessMap.heightRateMesh = 40f;

        AnimationCurve curve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.4f, 0.05f),
            new Keyframe(0.6f, 0.3f),
            new Keyframe(1f, 1f));
        endlessMap.heightCurve = curve;

        endlessMap.landscapeType = new LandscapeType[]
        {
            new LandscapeType { name = "Deep Water",   height = 0.3f,  color = new Color(0.06f, 0.18f, 0.55f) },
            new LandscapeType { name = "Water",        height = 0.4f,  color = new Color(0.13f, 0.35f, 0.78f) },
            new LandscapeType { name = "Sand",         height = 0.45f, color = new Color(0.87f, 0.82f, 0.55f) },
            new LandscapeType { name = "Grass",        height = 0.6f,  color = new Color(0.22f, 0.55f, 0.14f) },
            new LandscapeType { name = "Forest",       height = 0.7f,  color = new Color(0.12f, 0.35f, 0.08f) },
            new LandscapeType { name = "Rock",         height = 0.85f, color = new Color(0.45f, 0.40f, 0.35f) },
            new LandscapeType { name = "Snow",         height = 1f,    color = Color.white },
        };

        // Directional light
        GameObject lightGO = new GameObject("Directional Light");
        Light light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        Selection.activeGameObject = mapGO;
        Debug.Log("[SceneSetup] Scene ready. Press Play and fly with WASD + mouse.");
    }
}
