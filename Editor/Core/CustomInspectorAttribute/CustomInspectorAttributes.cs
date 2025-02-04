#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Callbacks;

namespace StudioFortithri.AttributesAttachInspector
{
    internal static class CustomInspectorAttributes
    {
        [InitializeOnLoadMethod, DidReloadScripts]
        public static void Rebuild()
        {
            foreach (Type type in TypeCache.GetTypesWithAttribute<CustomInspectorAttribute>())
                if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                    CustomEditorAttributes.CreateCustomEditor(type, typeof(CustomInspectorEditor), false, false, true);
        }
    }
}
#endif
