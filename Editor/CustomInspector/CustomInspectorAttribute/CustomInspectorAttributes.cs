#if UNITY_EDITOR
using System;
using UnityEditor;

namespace StudioFortithri.Editor43
{
    public static class CustomInspectorAttributes
    {
        public static void Rebuild()
        {
            foreach (Type type in TypeCache.GetTypesWithAttribute<CustomInspectorAttribute>())
                if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                    CustomEditorAttributes.CreateCustomEditor(type, typeof(CustomInspectorEditor), false, false, true);
        }
    }
}
#endif
