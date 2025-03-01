#if UNITY_EDITOR
using UnityEditor.Callbacks;

namespace StudioFortithri.Editor43
{
    internal static class Editor43Register
    {
        [InitializeOnEditorStartup, DidReloadScripts]
        private static void RebuildCustomInspector()
        {
            CustomGUILayoutAttributes.Rebuild();
            CustomInspectorAttributes.Rebuild();
        }
    }
}
#endif
