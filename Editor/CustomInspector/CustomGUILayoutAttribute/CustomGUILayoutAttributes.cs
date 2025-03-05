#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace StudioFortithri.Editor43
{
    public static class CustomGUILayoutAttributes
    {
        public static readonly Dictionary<Type, Type> pairs = new();

        public static void Rebuild()
        {
            foreach (Type type in TypeCache.GetTypesWithAttribute<CustomGUILayoutAttribute>())
                if (type.IsSubclassOf(typeof(GUILayoutDrawer)) &&
                    type.GetCustomAttribute(typeof(CustomGUILayoutAttribute), false) is CustomGUILayoutAttribute custom)
                    pairs.Add(custom.guiLayout, type);
        }
    }
}
#endif
