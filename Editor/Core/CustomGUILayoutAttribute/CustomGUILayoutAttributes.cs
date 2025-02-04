#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;

namespace StudioFortithri.AttributesAttachInspector
{
    /// <summary>
    /// 使用 <see cref="CustomGUILayoutAttribute"/> 指定 <see cref="GUILayoutAttribute"/> 的派生类以绑定绘制器。
    /// </summary>
    public abstract class GUILayoutDrawer
    {
        protected internal MemberInfo MemberInfo { get; internal set; }
        protected internal bool IsMethod { get; internal set; }

        private SerializedProperty _serializedProperty;
        protected internal SerializedProperty SerializedProperty
        {
            get
            {
                if (IsMethod) throw new InvalidOperationException("Can't get serializedProperty with method.");
                return _serializedProperty;
            }
            internal set => _serializedProperty = value;
        }

        protected virtual void OnEnable() { }
        protected virtual void OnInspectorGUI() { }

        internal void InternalOnEnable() => OnEnable();
        internal void InternalOnInspectorGUI() => OnInspectorGUI();
    }

    /// <summary>
    /// 为 <see cref="GUILayoutDrawer"/> 绑定 <see cref="GUILayoutAttribute"/>。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CustomGUILayoutAttribute : Attribute
    {
        internal Type _guiLayout;

        public CustomGUILayoutAttribute(Type guiLayout)
        {
            if (guiLayout == null) throw new ArgumentNullException($"{nameof(guiLayout)} can't be null.");

            if (!guiLayout.IsSubclassOf(typeof(GUILayoutAttribute)))
                throw new ArgumentException($"{nameof(guiLayout)} must be subclass of {typeof(GUILayoutAttribute).FullName}.");

            _guiLayout = guiLayout;
        }
    }

    internal static class CustomGUILayoutAttributes
    {
        public static readonly Dictionary<Type, Type> binds = new();

        [InitializeOnLoadMethod, DidReloadScripts]
        public static void Rebuild()
        {
            binds.Clear();

            foreach (Type type in TypeCache.GetTypesWithAttribute<CustomGUILayoutAttribute>())
                if (type.IsSubclassOf(typeof(GUILayoutDrawer)) &&
                    type.GetCustomAttribute(typeof(CustomGUILayoutAttribute), false) is CustomGUILayoutAttribute custom)
                    binds.Add(custom._guiLayout, type);
        }
    }
}
#endif
