using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScriptableObject), true)]
public class GuidedScriptableObjectEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (target is ScriptableObject && target is IGuided guidedObject)
        {
            EditorGUILayout.LabelField("GUID", guidedObject.guid);

            if (GUILayout.Button("Generate New GUID"))
            {
                guidedObject.GenerateGuid();
                EditorUtility.SetDirty(target);
            }
        }
    }
}