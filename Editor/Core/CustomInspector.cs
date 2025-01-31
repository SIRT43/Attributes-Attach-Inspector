using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif
using UnityEngine;

namespace StudioFortithri.AttributesAttachInspector
{
    /// <summary>
    /// 与 <see cref="CustomEditor"/> 类似，但本特性应该直接向目标类本身标记。<br></br>
    /// 被标记的类将启用 <see cref="GUILayoutAttribute"/>。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CustomInspectorAttribute : Attribute { }

#if UNITY_EDITOR
    internal static class CustomInspectorAttributes
    {
        [InitializeOnLoadMethod, DidReloadScripts]
        private static void Rebuild()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.GetCustomAttribute(typeof(CustomInspectorAttribute), false) is CustomInspectorAttribute)
                        if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                            CustomEditorAttributes.CreateCustomEditor(type, typeof(CustomInspectorEditor), false, false, true);
                        else Debug.Log($"{type.Name} must be subclass of {typeof(UnityEngine.Object).FullName}.");
                }
            }
        }
    }
#endif
}
