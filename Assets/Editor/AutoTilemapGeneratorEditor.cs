using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AutoTilemapGenerator))]
public class AutoTilemapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AutoTilemapGenerator generator = (AutoTilemapGenerator)target;

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Layers"))
        {
            generator.GenerateAll();
            EditorUtility.SetDirty(generator); // Označí objekt ako modifikovaný
        }
    }
}
