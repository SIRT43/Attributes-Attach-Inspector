using System;

namespace StudioFortithri.Editor43
{
    /// <summary>
    /// ͨ���̳б��������� GUI Layout Attribute ��ǡ�
    /// </summary>
    public abstract class GUILayoutAttribute : Attribute
    {
        internal int order;

        public GUILayoutAttribute(int order) => this.order = order;
        public GUILayoutAttribute() : this(0) { }
    }
}
