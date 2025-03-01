#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace StudioFortithri.Editor43
{
    /// <summary>
    /// Ϊ�����������ʹ�� <see cref="CustomGUILayoutAttribute"/> ָ�� <see cref="GUILayoutAttribute"/> ���������԰󶨻�������
    /// </summary>
    public abstract class GUILayoutDrawer
    {
        protected internal sealed class GUILayoutDrawState
        {
            public bool isDrawed = false;

            internal GUILayoutDrawState() { }
        }

        protected internal object Target { get; internal set; }
        protected internal GUILayoutAttribute Attribute { get; internal set; }
        protected internal MemberInfo MemberInfo { get; internal set; }
        protected internal bool IsMethod { get; internal set; }

        private SerializedProperty serializedProperty;
        protected internal SerializedProperty SerializedProperty
        {
            get
            {
                if (IsMethod) throw new InvalidOperationException("Can't get serializedProperty with method.");
                return serializedProperty;
            }
            internal set => serializedProperty = value;
        }

        /// <summary>
        /// ���� <see cref="OnInspectorGUI"/> �����Ƿ��Ѿ��� Drawer ���ƹ� GUI��
        /// </summary>
        protected internal GUILayoutDrawState DrawState { get; internal set; }

        protected virtual void OnEnable() { }
        protected virtual void OnInspectorGUI() { }

        internal void InternalOnEnable() => OnEnable();
        internal void InternalOnInspectorGUI() => OnInspectorGUI();
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CustomGUILayoutAttribute : Attribute
    {
        internal Type guiLayout;

        public CustomGUILayoutAttribute(Type guiLayout)
        {
            if (guiLayout == null) throw new ArgumentNullException($"{nameof(guiLayout)} can't be null.");

            if (!guiLayout.IsSubclassOf(typeof(GUILayoutAttribute)))
                throw new ArgumentException($"{nameof(guiLayout)} must be subclass of {typeof(GUILayoutAttribute).FullName}.");

            this.guiLayout = guiLayout;
        }
    }

    internal static class CustomGUILayoutAttributes
    {
        public static readonly Dictionary<Type, Type> binds = new();

        public static void Rebuild()
        {
            foreach (Type type in TypeCache.GetTypesWithAttribute<CustomGUILayoutAttribute>())
                if (type.IsSubclassOf(typeof(GUILayoutDrawer)) &&
                    type.GetCustomAttribute(typeof(CustomGUILayoutAttribute), false) is CustomGUILayoutAttribute custom)

                    binds.Add(custom.guiLayout, type);
        }
    }
}
#endif
