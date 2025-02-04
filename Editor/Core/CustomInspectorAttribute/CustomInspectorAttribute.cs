using System;

namespace StudioFortithri.AttributesAttachInspector
{
    /// <summary>
    /// 与 CustomEditor 类似，但本特性应该直接向目标类本身标记。<br></br>
    /// 被标记的类将启用 <see cref="GUILayoutAttribute"/>。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CustomInspectorAttribute : Attribute { }
}
