#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace StudioFortithri.Editor43
{
    internal class Editor43AttributesWindow : EditorWindow
    {
        [MenuItem("Window/Editor 43 Attributes", priority = 9999)]
        private static void DisplayWindow()
        {
            Editor43AttributesWindow window = GetWindow<Editor43AttributesWindow>();
            window.Show();
        }

        private void OnEnable() => position = new(Screen.width / 2, Screen.height / 2, minSize.x, minSize.y);

        private void OnGUI()
        {
            if (GUILayout.Button("Rebuild Custom Editor Attributes")) CustomEditorAttributes.Rebuild();
            if (GUILayout.Button("Rebuild Custom Inspector Attributes")) CustomInspectorAttributes.Rebuild();
            if (GUILayout.Button("Rebuild Custom GUI Layout Attributes")) CustomGUILayoutAttributes.Rebuild();
        }
    }
}
#endif
