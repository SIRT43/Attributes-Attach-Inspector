using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StudioFortithri.AttributesAttachInspector
{
    /// <summary>
    /// �ڼ̳б���ʱȷ�������ñ����Ա�����ʹ�� #if UNITY_EDITOR ������������ Release ����ʱ����Ϊ���ࡣ
    /// </summary>
    public abstract class Inspector
    {
#if UNITY_EDITOR
        /// <summary>
        /// <see cref="Inspector"/> �������� <see cref="GUILayoutAttribute"/>��
        /// </summary>
        protected internal GUILayoutAttribute Attribute { get; internal set; }
        /// <summary>
        /// <see cref="GUILayoutAttribute"/> �������� <see cref="System.Reflection.MemberInfo"/>��
        /// </summary>
        protected internal MemberInfo MemberInfo { get; internal set; }
        /// <summary>
        /// <see cref="System.Reflection.MemberInfo"/> ��������ʵ���� <see cref="UnityEditor.SerializedObject"/>��
        /// </summary>
        protected internal SerializedObject SerializedObject { get; internal set; }

        /// <summary>
        /// <see cref="System.Reflection.MemberInfo"/> ��������ʵ�������� <see cref="Targets"/> ��Ԫ�ء�
        /// </summary>
        protected internal UnityEngine.Object Target { get; internal set; }
        /// <summary>
        /// �ڱ༭�������ʱ�����á�
        /// </summary>
        protected internal UnityEngine.Object[] Targets { get; internal set; }

        protected virtual void OnEnable() { }
        protected virtual void OnInspectorGUI() { }
        protected virtual void OnValidate() { }

        // ��Щ�������ڱ��������ⲿ���á�
        // ����̳����� override ʱ��ʹ�� protected internal ������ protected���ⲻ�������ġ�
        internal void InvokeOnEnable() => OnEnable();
        internal void InvokeOnInspectorGUI() => OnInspectorGUI();
        internal void InvokeOnValidate() => OnValidate();
#endif
    }

    /// <summary>
    /// Ϊ <see cref="GUILayoutAttribute"/> ָ�� <see cref="Inspector"/>��
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
    /// �̳б����Դ��� GUI Layout Attribute ��ǡ�<br></br>
    /// ʹ�� <see cref="GUILayoutDrawerAttribute"/> ָ�� <see cref="Inspector"/>��
    /// </summary>
    public abstract class GUILayoutAttribute : Attribute { }
}
