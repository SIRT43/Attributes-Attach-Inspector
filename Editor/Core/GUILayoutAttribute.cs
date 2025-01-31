using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StudioFortithri.AttributesAttachInspector
{
    /// <summary>
    /// 在继承本类时确保将引用本类成员的语句使用 #if UNITY_EDITOR 包裹，本类在 Release 编译时将变为空类。
    /// </summary>
    public abstract class Inspector
    {
#if UNITY_EDITOR
        /// <summary>
        /// <see cref="Inspector"/> 所关联的 <see cref="GUILayoutAttribute"/>。
        /// </summary>
        protected internal GUILayoutAttribute Attribute { get; internal set; }
        /// <summary>
        /// <see cref="GUILayoutAttribute"/> 所关联的 <see cref="System.Reflection.MemberInfo"/>。
        /// </summary>
        protected internal MemberInfo MemberInfo { get; internal set; }
        /// <summary>
        /// <see cref="System.Reflection.MemberInfo"/> 所关联的实例的 <see cref="UnityEditor.SerializedObject"/>。
        /// </summary>
        protected internal SerializedObject SerializedObject { get; internal set; }

        /// <summary>
        /// <see cref="System.Reflection.MemberInfo"/> 所关联的实例，它是 <see cref="Targets"/> 的元素。
        /// </summary>
        protected internal UnityEngine.Object Target { get; internal set; }
        /// <summary>
        /// 在编辑多个对象时将启用。
        /// </summary>
        protected internal UnityEngine.Object[] Targets { get; internal set; }

        protected virtual void OnEnable() { }
        protected virtual void OnInspectorGUI() { }
        protected virtual void OnValidate() { }

        // 这些方法用于本程序集中外部调用。
        // 否则继承者在 override 时将使用 protected internal 而不是 protected，这不是期望的。
        internal void InvokeOnEnable() => OnEnable();
        internal void InvokeOnInspectorGUI() => OnInspectorGUI();
        internal void InvokeOnValidate() => OnValidate();
#endif
    }

    /// <summary>
    /// 为 <see cref="GUILayoutAttribute"/> 指定 <see cref="Inspector"/>。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class GUILayoutDrawerAttribute : Attribute
    {
        internal Type _inspector;

        public GUILayoutDrawerAttribute(Type inspector)
        {
            if (inspector == null) throw new ArgumentNullException($"{nameof(inspector)} can't be null.");

            if (!inspector.IsSubclassOf(typeof(Inspector)))
                throw new ArgumentException($"{nameof(inspector)} must be subclass of {typeof(Inspector).FullName}.");

            _inspector = inspector;
        }
    }

    /// <summary>
    /// 继承本类以创建 GUI Layout Attribute 标记。<br></br>
    /// 使用 <see cref="GUILayoutDrawerAttribute"/> 指定 <see cref="Inspector"/>。
    /// </summary>
    public abstract class GUILayoutAttribute : Attribute { }
}
