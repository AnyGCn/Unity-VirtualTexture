namespace VirtualTexture.Editor
{
    using UnityEditor;
    using UnityEngine;
    using TestRunning;

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
