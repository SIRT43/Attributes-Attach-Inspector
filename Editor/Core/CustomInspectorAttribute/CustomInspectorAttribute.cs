using System;

namespace StudioFortithri.AttributesAttachInspector
{
    /// <summary>
    /// �� CustomEditor ���ƣ���������Ӧ��ֱ����Ŀ���౾���ǡ�<br></br>
    /// ����ǵ��ཫ���� <see cref="GUILayoutAttribute"/>��
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CustomInspectorAttribute : Attribute { }
}
