using System;

namespace StudioFortithri.AttributesAttachInspector
{
    /// <summary>
    /// ͨ���̳б��������� GUI Layout Attribute ��ǡ�
    /// </summary>
    public abstract class GUILayoutAttribute : Attribute
    {
        internal int _order;

        public GUILayoutAttribute(int order) => _order = order;
        public GUILayoutAttribute() : this(0) { }
    }
}
