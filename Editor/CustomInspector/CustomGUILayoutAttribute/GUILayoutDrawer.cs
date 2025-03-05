#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;

namespace StudioFortithri.Editor43
{
    public abstract class GUILayoutDrawer
    {
        protected internal sealed class GUILayoutDrawState
        {
            public bool isDrawed = false;
            internal GUILayoutDrawState() { }
        }

        protected internal object Target { get; internal set; }
        protected internal object[] Targets { get; internal set; }
        protected internal GUILayoutAttribute Attribute { get; internal set; }
        protected internal MemberInfo MemberInfo { get; internal set; }
        protected internal SerializedProperty SerializedProperty { get; internal set; }

        /// <summary>
        /// 本次 <see cref="OnInspectorGUI"/> 调用是否已经有 Drawer 绘制过 GUI。
        /// </summary>
        protected internal GUILayoutDrawState DrawState { get; internal set; }

        protected virtual void OnEnable() { }
        protected virtual void OnInspectorGUI() { }

        internal void InternalOnEnable() => OnEnable();
        internal void InternalOnInspectorGUI() => OnInspectorGUI();
    }
}
#endif