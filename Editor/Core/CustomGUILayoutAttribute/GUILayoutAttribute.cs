using System;

namespace StudioFortithri.Editor43
{
    /// <summary>
    /// 通过继承本类来创建 GUI Layout Attribute 标记。
    /// </summary>
    public abstract class GUILayoutAttribute : Attribute
    {
        internal int order;

        public GUILayoutAttribute(int order) => this.order = order;
        public GUILayoutAttribute() : this(0) { }
    }
}
