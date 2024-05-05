namespace VirtualTexture.Sample
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(VTUnlitTest))]
    public class VirtualTextureConsoleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("TODO: Add some editor tools here.");
        }
    }
}
