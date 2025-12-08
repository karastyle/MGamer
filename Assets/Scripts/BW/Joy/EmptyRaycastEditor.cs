// EmptyRaycastEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(EmptyRaycast), false)]
public class EmptyRaycastEditor : GraphicEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.Space();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif