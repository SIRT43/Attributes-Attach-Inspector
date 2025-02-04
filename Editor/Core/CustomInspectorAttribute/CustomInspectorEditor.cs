#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditorInternal;

namespace StudioFortithri.AttributesAttachInspector
{
    internal class CustomInspectorEditor : Editor
    {
        private class GUIHierarchy
        {
            public Action onInspectorGUICallback;

            public bool enableChild = true;
            public List<GUIHierarchy> child = new();

            public void OnInspectorGUI()
            {
                onInspectorGUICallback.Invoke();

                if (enableChild && child.Count != 0)
                {
                    EditorGUI.indentLevel++;
                    foreach (GUIHierarchy child in child) child.OnInspectorGUI();
                    EditorGUI.indentLevel--;
                }
            }
        }

        private static readonly Dictionary<Type, List<FieldInfo>> _fieldCache = new();
        private static readonly Dictionary<Type, List<MethodInfo>> _methodCache = new();

        [DidReloadScripts]
        private static void CleanCache()
        {
            _fieldCache.Clear();
            _methodCache.Clear();
        }

        private static void GetBeforeAfter(MemberInfo member, out GUILayoutAttribute[] before, out GUILayoutAttribute[] after)
        {
            GUILayoutAttribute[] attributes = (GUILayoutAttribute[])member.GetCustomAttributes(typeof(GUILayoutAttribute), true);

            List<GUILayoutAttribute> beforeAttributes = new() { Capacity = attributes.Length };
            List<GUILayoutAttribute> afterAttributes = new() { Capacity = attributes.Length };

            foreach (GUILayoutAttribute attribute in attributes)
            {
                if (attribute._order < 0) beforeAttributes.Add(attribute);
                else afterAttributes.Add(attribute);
            }

            static int Comparison(GUILayoutAttribute a, GUILayoutAttribute b) => a._order.CompareTo(b);

            beforeAttributes.Sort(Comparison);
            afterAttributes.Sort(Comparison);

            before = beforeAttributes.ToArray();
            after = afterAttributes.ToArray();
        }

        private static void InitSingle(GUILayoutAttribute[] attributes, MemberInfo member, SerializedProperty property, List<GUILayoutDrawer> drawers, List<GUIHierarchy> hierarchies)
        {
            for (int index = 0; index < attributes.Length; index++)
            {
                Type attributeType = attributes[index].GetType();

                if (!CustomGUILayoutAttributes.binds.TryGetValue(attributeType, out Type drawerType)) continue;

                GUILayoutDrawer guiLayoutDrawer = (GUILayoutDrawer)Activator.CreateInstance(drawerType);

                guiLayoutDrawer.MemberInfo = member;
                guiLayoutDrawer.IsMethod = member.MemberType == MemberTypes.Method;
                guiLayoutDrawer.SerializedProperty = property;

                drawers.Add(guiLayoutDrawer);
                hierarchies.Add(new() { onInspectorGUICallback = () => guiLayoutDrawer.InternalOnInspectorGUI() });
            }
        }

        private static void InitArray(IList array, SerializedObject serializedObject, SerializedProperty property, List<GUILayoutDrawer> drawers, List<List<GUIHierarchy>> arrayHierarchies)
        {
            for (int index = 0; index < array.Count; index++)
            {
                List<GUIHierarchy> singleHierarchies = new();
                arrayHierarchies.Add(singleHierarchies);

                Init(array[index], serializedObject, drawers, singleHierarchies, property.GetArrayElementAtIndex(index));
            }
        }

        private static void Init(object target, SerializedObject serializedObject, List<GUILayoutDrawer> drawers, List<GUIHierarchy> hierarchies, SerializedProperty fromProperty = null)
        {
            Type type = target.GetType();
            BindingFlags binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            if (!_fieldCache.ContainsKey(type)) _fieldCache.Add(type, new(type.GetFields(binding)));
            if (!_methodCache.ContainsKey(type)) _methodCache.Add(type, new(type.GetMethods(binding)));

            foreach (FieldInfo field in _fieldCache[type])
            {
                SerializedProperty fieldProperty = fromProperty == null ?
                    serializedObject.FindProperty(field.Name) :
                    fromProperty.FindPropertyRelative(field.Name);

                // 此字段无法序列化则跳过本字段。
                if (fieldProperty == null) continue;

                GetBeforeAfter(field, out GUILayoutAttribute[] before, out GUILayoutAttribute[] after);

                bool isArray = fieldProperty.isArray && field.FieldType != typeof(string);
                Type genericType = isArray ? field.FieldType.GetGenericArguments()[0] : null;

                bool isCustomInspectorField = (isArray ? genericType : field.FieldType)
                    .IsDefined(typeof(CustomInspectorAttribute), false);

                InitSingle(before, field, fieldProperty, drawers, hierarchies);

                {
                    GUIHierarchy hierarchy = null;

                    // 如果是自定义 Inspector 类则需要自定义 Inspector GUI 而不是使用 EditorGUILayout.PropertyField 绘制默认 GUI。
                    if (isCustomInspectorField)
                    {
                        // 如果是数组则使用 ReorderableList 绘制。
                        if (isArray)
                        {
                            IList array = field.GetValue(target) as IList;

                            List<GUILayoutDrawer> arrayDrawers = new();
                            List<List<GUIHierarchy>> arrayHierarchies = new() { Capacity = fieldProperty.arraySize };

                            InitArray(array, serializedObject, fieldProperty, arrayDrawers, arrayHierarchies);

                            foreach (GUILayoutDrawer drawer in arrayDrawers)
                                drawer.InternalOnEnable();

                            ReorderableList guiList = new(array, genericType, true, false, true, true)
                            {
                                drawElementCallback = (rect, index, isActive, isFocused) =>
                                {
                                    foreach (GUIHierarchy hierarchy in arrayHierarchies[index])
                                        hierarchy.OnInspectorGUI();
                                },
                                onChangedCallback = (list) =>
                                {
                                    arrayDrawers.Clear();
                                    arrayHierarchies.Clear();

                                    InitArray(array, serializedObject, fieldProperty, arrayDrawers, arrayHierarchies);
                                }
                            };

                            hierarchy = new() { onInspectorGUICallback = () => guiList.DoLayoutList() };
                        }
                        // 如果是类型则绘制抽屉。
                        else
                        {
                            hierarchy = new();

                            hierarchy.onInspectorGUICallback =
                                () => hierarchy.enableChild = EditorGUILayout.Foldout(hierarchy.enableChild, field.Name);

                            Init(field.GetValue(target), serializedObject, drawers, hierarchy.child, fieldProperty);
                        }
                    }
                    // 如果不是自定义 Inspector 类且没有任何自定义 attribute 则绘制默认 GUI。
                    else if (before.Length == 0 && after.Length == 0)
                        hierarchy = new() { onInspectorGUICallback = () => EditorGUILayout.PropertyField(fieldProperty) };

                    if (hierarchy != null) hierarchies.Add(hierarchy);
                }

                InitSingle(after, field, fieldProperty, drawers, hierarchies);
            }

            foreach (MethodInfo method in _methodCache[type])
                InitSingle((GUILayoutAttribute[])method.GetCustomAttributes(typeof(GUILayoutAttribute), true), method, null, drawers, hierarchies);

        }

        private readonly List<GUILayoutDrawer> _drawers = new();
        private readonly List<GUIHierarchy> _hierarchies = new();

        private void OnEnable()
        {
            Init(target, serializedObject, _drawers, _hierarchies);

            foreach (GUILayoutDrawer drawer in _drawers) drawer.InternalOnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            foreach (GUIHierarchy hierarchy in _hierarchies) hierarchy.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
