using System;

namespace StudioFortithri.AttributesAttachInspector
{
    /// <summary>
    /// 通过继承本类来创建 GUI Layout Attribute 标记。
    /// </summary>
    public abstract class GUILayoutAttribute : Attribute
    {
        internal int _order;

        public GUILayoutAttribute(int order) => _order = order;
        public GUILayoutAttribute() : this(0) { }
    }
}
