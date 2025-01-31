#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace StudioFortithri.AttributesAttachInspector
{
    internal class CustomInspectorEditor : Editor
    {
        private readonly List<Inspector> _inspectors = new();
        private readonly List<SerializedProperty> _defaultSerializes = new();

        private void InitInspectorsWithMemberInfo(MemberInfo member, GUILayoutAttribute[] attributes = null)
        {
            foreach (GUILayoutAttribute attribute in attributes ?? (GUILayoutAttribute[])member.GetCustomAttributes(typeof(GUILayoutAttribute), true))
            {
                Type attributeType = attribute.GetType();

                object drawer = attributeType.GetCustomAttribute(typeof(GUILayoutDrawerAttribute), false) ??
                    throw new NullReferenceException($"{attributeType.Name} must attach to a {typeof(Inspector).FullName} use {typeof(GUILayoutDrawerAttribute).FullName}.");

                Inspector inspector = (Inspector)Activator.CreateInstance((drawer as GUILayoutDrawerAttribute)._inspector);

                inspector.Attribute = attribute;
                inspector.MemberInfo = member;
                inspector.SerializedObject = serializedObject;

                inspector.Target = target;
                inspector.Targets = targets;

                _inspectors.Add(inspector);
            }
        }

        private void InitInspectors()
        {
            Type type = target.GetType();
            BindingFlags binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (FieldInfo field in type.GetFields(binding))
            {
                SerializedProperty serializedProperty = serializedObject.FindProperty(field.Name);
                GUILayoutAttribute[] attributes = (GUILayoutAttribute[])field.GetCustomAttributes(typeof(GUILayoutAttribute), true);

                if (attributes.Length == 0 && serializedProperty != null) _defaultSerializes.Add(serializedProperty);
                else InitInspectorsWithMemberInfo(field, attributes);
            }

            foreach (PropertyInfo property in type.GetProperties(binding))
                InitInspectorsWithMemberInfo(property);

            foreach (MethodInfo method in type.GetMethods(binding))
                InitInspectorsWithMemberInfo(method);
        }

        private void OnEnable()
        {
            InitInspectors();

            foreach (Inspector inspector in _inspectors) inspector.InvokeOnEnable();
        }
        public override void OnInspectorGUI()
        {
            foreach (SerializedProperty property in _defaultSerializes) EditorGUILayout.PropertyField(property);
            serializedObject.ApplyModifiedProperties();

            foreach (Inspector inspector in _inspectors) inspector.InvokeOnInspectorGUI();
        }
    }
}
#endif
