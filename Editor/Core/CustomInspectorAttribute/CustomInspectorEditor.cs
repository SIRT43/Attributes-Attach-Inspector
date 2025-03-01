#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace StudioFortithri.Editor43
{
    internal class CustomInspectorEditor : Editor
    {
        // GUI 层级，用于实现嵌套的 Custom Inspector Serializable 数据结构。
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

        private static readonly Dictionary<Type, List<FieldInfo>> fieldCache = new();
        private static readonly Dictionary<Type, List<MethodInfo>> methodCache = new();

        private static void GetBeforeAfter(MemberInfo member, out GUILayoutAttribute[] before, out GUILayoutAttribute[] after)
        {
            GUILayoutAttribute[] attributes = (GUILayoutAttribute[])member.GetCustomAttributes(typeof(GUILayoutAttribute), true);

            List<GUILayoutAttribute> beforeAttributes = new() { Capacity = attributes.Length };
            List<GUILayoutAttribute> afterAttributes = new() { Capacity = attributes.Length };

            foreach (GUILayoutAttribute attribute in attributes)
            {
                if (attribute.order < 0) beforeAttributes.Add(attribute);
                else afterAttributes.Add(attribute);
            }

            static int Comparison(GUILayoutAttribute a, GUILayoutAttribute b) => a.order.CompareTo(b.order);

            beforeAttributes.Sort(Comparison);
            afterAttributes.Sort(Comparison);

            before = beforeAttributes.ToArray();
            after = afterAttributes.ToArray();
        }

        private static void InitSingle(object target, GUILayoutAttribute[] attributes, MemberInfo member, SerializedProperty property, List<GUILayoutDrawer> drawers, List<GUIHierarchy> hierarchies)
        {
            GUILayoutDrawer.GUILayoutDrawState drawState = new();

            for (int index = 0; index < attributes.Length; index++)
            {
                Type attributeType = attributes[index].GetType();

                // 如果没有创建关于此 GUI Layout Attribute 的 GUI Layout Drawer 绑定则跳过本 GUI Layout Attribute。
                if (!CustomGUILayoutAttributes.binds.TryGetValue(attributeType, out Type drawerType)) continue;

                GUILayoutDrawer guiLayoutDrawer = (GUILayoutDrawer)Activator.CreateInstance(drawerType);

                guiLayoutDrawer.Target = target;
                guiLayoutDrawer.Attribute = attributes[index];
                guiLayoutDrawer.MemberInfo = member;
                guiLayoutDrawer.IsMethod = member.MemberType == MemberTypes.Method;
                guiLayoutDrawer.SerializedProperty = property;
                guiLayoutDrawer.DrawState = drawState;

                drawers.Add(guiLayoutDrawer);
                hierarchies.Add(new() { onInspectorGUICallback = () => guiLayoutDrawer.InternalOnInspectorGUI() });
            }
        }

        private static void Init(object target, SerializedObject serializedObject, List<GUILayoutDrawer> drawers, List<GUIHierarchy> hierarchies, SerializedProperty fromProperty = null)
        {
            Type type = target.GetType();
            BindingFlags binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            if (!fieldCache.ContainsKey(type)) fieldCache.Add(type, new(type.GetFields(binding)));
            if (!methodCache.ContainsKey(type)) methodCache.Add(type, new(type.GetMethods(binding)));

            foreach (FieldInfo field in fieldCache[type])
            {
                SerializedProperty fieldProperty = fromProperty == null ?
                    serializedObject.FindProperty(field.Name) :
                    fromProperty.FindPropertyRelative(field.Name);

                // 此字段无法序列化则跳过本字段。
                if (fieldProperty == null) continue;

                Type fieldType = field.FieldType;
                // 判断此 SerializedProperty 是否是数组不仅要访问 isArray 还要确认它不是 string。
                // 因为 string 以数组序列化但是检查器不会以数组为 string 创建 Inspector GUI。
                bool isArray = fieldProperty.isArray && fieldType != typeof(string);

                Type arrayElementType = null;
                if (isArray) arrayElementType = fieldType.IsGenericType ?
                        fieldType.GetGenericArguments()[0] : fieldType.GetElementType();

                Type customInspectorFieldType = isArray ? arrayElementType : field.FieldType;
                // 要确认是否以嵌套的方式创建 Inspector GUI 需要确认此 Custom Inspector 类型不是 UnityEngine.Object 的派生类，
                // 因为 UnityEngine.Object 是以资产引用的方式创建 GUI 而不是层级结构。
                bool isCustomInspectorField =
                    !customInspectorFieldType.IsSubclassOf(typeof(UnityEngine.Object)) &&
                    customInspectorFieldType.IsDefined(typeof(CustomInspectorAttribute), false);

                GetBeforeAfter(field, out GUILayoutAttribute[] before, out GUILayoutAttribute[] after);
                InitSingle(target, before, field, fieldProperty, drawers, hierarchies);

                // 本作用域决定了绘制行为。
                // 如果是 Custom Inspector:
                //     是数组则绘制默认 (EditorGUILayout.PropertyField) GUI 并发出警告。
                //     不是数组则创建抽屉层级并基于那个层级创建嵌套 Custom Inspector 层级。
                // 如果不是 Custom Inspector 且没有任何自定义 GUI Layout Attribute 则绘制默认 GUI。
                {
                    GUIHierarchy hierarchy = null;

                    // 如果是自定义 Inspector 类则需要自定义 Inspector GUI 而不是使用 EditorGUILayout.PropertyField 绘制默认 GUI。
                    if (isCustomInspectorField)
                    {
                        // 如果是数组。
                        if (isArray) hierarchy = new()
                        {
                            onInspectorGUICallback = () =>
                            {
                                EditorGUILayout.PropertyField(fieldProperty);
                                EditorGUILayout.HelpBox("Unable to draw a custom inspector for an array.\nCustom inspector disabled, please use CustomPropertyDrawer.", MessageType.Warning);
                            }
                        };
                        // 如果是类型则绘制抽屉。(层级结构)
                        else
                        {
                            hierarchy = new();

                            hierarchy.onInspectorGUICallback =
                                () => hierarchy.enableChild = EditorGUILayout.Foldout(hierarchy.enableChild, field.Name);

                            Init(field.GetValue(target), serializedObject, drawers, hierarchy.child, fieldProperty);
                        }
                    }
                    // 如果不是 Custom Inspector 类且没有任何自定义 attribute 则绘制默认 GUI。
                    else if (before.Length == 0 && after.Length == 0)
                        hierarchy = new() { onInspectorGUICallback = () => EditorGUILayout.PropertyField(fieldProperty) };

                    if (hierarchy != null) hierarchies.Add(hierarchy);
                }

                InitSingle(target, after, field, fieldProperty, drawers, hierarchies);
            }

            foreach (MethodInfo method in methodCache[type])
                InitSingle(target, (GUILayoutAttribute[])method.GetCustomAttributes(typeof(GUILayoutAttribute), true), method, null, drawers, hierarchies);
        }

        private readonly List<GUILayoutDrawer> drawers = new();
        private readonly List<GUIHierarchy> hierarchies = new();

        private void OnEnable()
        {
            Init(target, serializedObject, drawers, hierarchies);

            foreach (GUILayoutDrawer drawer in drawers) drawer.InternalOnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            foreach (GUIHierarchy hierarchy in hierarchies) hierarchy.OnInspectorGUI();
            // 在每次绘制后重置 DrawState.isDrawed。
            // 因为下一次 GUI 会被重绘，如果不重置那么任何基于 isDrawed 绘制的 GUI 都不会被继续绘制。
            foreach (GUILayoutDrawer drawer in drawers) drawer.DrawState.isDrawed = false;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
