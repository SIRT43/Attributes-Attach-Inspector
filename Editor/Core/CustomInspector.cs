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
    /// �� <see cref="CustomEditor"/> ���ƣ���������Ӧ��ֱ����Ŀ���౾���ǡ�<br></br>
    /// ����ǵ��ཫ���� <see cref="GUILayoutAttribute"/>��
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
