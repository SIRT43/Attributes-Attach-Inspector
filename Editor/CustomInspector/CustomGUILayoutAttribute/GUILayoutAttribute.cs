using System;

namespace StudioFortithri.Editor43
{
    public abstract class GUILayoutAttribute : Attribute
    {
        internal int order;

        /// <summary>
        /// ָ������˳�򣬴��� -1 ���ڻ���Ĭ������֮ǰ���ã�С�� 0 ���ڻ���Ĭ������֮����á�
        /// </summary>
        public GUILayoutAttribute(int order) => this.order = order;
        public GUILayoutAttribute() : this(0) { }
    }
}
