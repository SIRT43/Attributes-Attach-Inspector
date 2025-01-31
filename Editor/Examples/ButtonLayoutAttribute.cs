#if UNITY_EDITOR
using System;
using System.Reflection;
using StudioFortithri.AttributesAttachInspector;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true), GUILayoutDrawer(typeof(ButtonInspector))]
public sealed class ButtonLayoutAttribute : GUILayoutAttribute
{
    private readonly string _content;
    public string Content => _content;

    public ButtonLayoutAttribute(string content) =>
        _content = string.IsNullOrEmpty(content) ? "Missing Content" : content;
}

public class ButtonInspector : Inspector
{
    private ButtonLayoutAttribute _attribute;
    private MethodInfo _method;

    protected override void OnEnable()
    {
        _attribute = Attribute as ButtonLayoutAttribute;
        _method = MemberInfo as MethodInfo;
    }

    protected override void OnInspectorGUI()
    {
        if (GUILayout.Button(_attribute.Content))
            foreach (UnityEngine.Object target in Targets) _method.Invoke(target, null);
    }
}
#endif
