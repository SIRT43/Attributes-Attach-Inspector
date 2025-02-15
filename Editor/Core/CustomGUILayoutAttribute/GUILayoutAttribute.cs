using System;

namespace StudioFortithri.Editor43
{
    public abstract class GUILayoutAttribute : Attribute
    {
        internal int order;

        /// <summary>
        /// 指定绘制顺序，大于 -1 则在绘制默认内容之前调用，小于 0 则在绘制默认内容之后调用。
        /// </summary>
        public GUILayoutAttribute(int order) => this.order = order;
        public GUILayoutAttribute() : this(0) { }
    }
}
