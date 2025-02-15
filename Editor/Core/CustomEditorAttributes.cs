#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace StudioFortithri.Editor43
{
    /// <summary>
    /// 通过 System.Reflection 从 UnityEditor.CustomEditorAttributes 提取，原类可见性为 internal。<br></br>
    /// 本类旨在实现更加自由的编辑器检查器扩展。
    /// </summary>
    public static class CustomEditorAttributes
    {
        /// <summary>
        /// 本 property 决定本类是否可以在您的编辑器运行。
        /// </summary>
        public static bool Availability { get; private set; }

        private static Assembly coreModule;

        // CustomEditorAttributes 相关。
        private static Type customEditorAttributes;

        private static FieldInfo kSCustomEditors;
        private static FieldInfo kSCustomMultiEditors;

        private static MethodInfo rebuild;

        // MonoEditorType 相关。
        private static Type monoEditorType;

        private static FieldInfo monoEditorType_m_Inspected;
        private static FieldInfo monoEditorType_m_Inspector;
        private static FieldInfo monoEditorType_m_EditorForChildClasses;
        private static FieldInfo monoEditorType_m_IsFallback;

        private static Type listMonoEditorType;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            try
            {
                Availability = true;

                // 从程序集中获取核心模块。
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == "UnityEditor.CoreModule")
                    {
                        coreModule = assembly;
                        break;
                    }
                }

                // 从核心模块获取 CustomEditorAttributes 与它的嵌套类 MonoEditorType 相关的 Reflection 信息。
                // 见 https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Editor/Mono/CustomEditorAttributes.cs。

                customEditorAttributes = coreModule.GetType("UnityEditor.CustomEditorAttributes");

                // 以下两个字段用于存储所有 CustomEditor 实现映射。
                kSCustomEditors = customEditorAttributes.GetField("kSCustomEditors", BindingFlags.Static | BindingFlags.NonPublic);
                kSCustomMultiEditors = customEditorAttributes.GetField("kSCustomMultiEditors", BindingFlags.Static | BindingFlags.NonPublic);

                rebuild = customEditorAttributes.GetMethod("Rebuild", BindingFlags.Static | BindingFlags.NonPublic);

                monoEditorType = customEditorAttributes.GetNestedType("MonoEditorType", BindingFlags.NonPublic);

                // 获取构造 MonoEditorType 的字段信息。
                monoEditorType_m_Inspected = monoEditorType.GetField("m_InspectedType", BindingFlags.Instance | BindingFlags.Public);
                monoEditorType_m_Inspector = monoEditorType.GetField("m_InspectorType", BindingFlags.Instance | BindingFlags.Public);
                monoEditorType_m_EditorForChildClasses = monoEditorType.GetField("m_EditorForChildClasses", BindingFlags.Instance | BindingFlags.Public);
                monoEditorType_m_IsFallback = monoEditorType.GetField("m_IsFallback", BindingFlags.Instance | BindingFlags.Public);

                // 实现 List<MonoEditorType> 泛型类型。
                listMonoEditorType = typeof(List<>).MakeGenericType(monoEditorType);

                // 存在无法获取的情况那么本类则无法工作，抛出异常并进入 Catch 作用域。
                if (kSCustomEditors == null ||
                    kSCustomMultiEditors == null ||
                    rebuild == null ||
                    monoEditorType_m_Inspected == null ||
                    monoEditorType_m_Inspector == null ||
                    monoEditorType_m_EditorForChildClasses == null ||
                    monoEditorType_m_IsFallback == null) throw new NullReferenceException();
            }
            catch
            {
                // 初始化发生异常，本类在此编辑器不可用。
                Availability = false;
                Debug.LogWarning($"Can't init {nameof(CustomEditorAttributes)} on this UnityEditor version.");
            }
        }

        private static bool IsAvailability()
        {
            if (!Availability)
                Debug.LogWarning($"Invoke failed, {nameof(CustomEditorAttributes)} is not availability on this UnityEditor version.");
            return Availability;
        }

        /// <summary>
        /// 不通过 <see cref="CustomEditor"/> 创建检查器 CustomEditor 绑定。
        /// </summary>
        public static void CreateCustomEditor(Type objectType, Type editorType, bool editorForChildClasses = false, bool isFallback = false, bool canEditMultipleObject = false)
        {
            if (!IsAvailability()) return;

            if (!editorType.IsSubclassOf(typeof(Editor)))
                throw new ArgumentException($"{nameof(editorType)} must be subclass of UnityEditor.Editor.");
            if (!objectType.IsSubclassOf(typeof(UnityEngine.Object)))
                throw new ArgumentException($"{nameof(objectType)} must be subclass of UnityEngine.Object.");

            IDictionary kSCustomEditors = (IDictionary)CustomEditorAttributes.kSCustomEditors.GetValue(null);
            IDictionary kSCustomMultiEditors = (IDictionary)CustomEditorAttributes.kSCustomMultiEditors.GetValue(null);

            object monoEditorType = Activator.CreateInstance(CustomEditorAttributes.monoEditorType);
            monoEditorType_m_Inspected.SetValue(monoEditorType, objectType);
            monoEditorType_m_Inspector.SetValue(monoEditorType, editorType);
            monoEditorType_m_EditorForChildClasses.SetValue(monoEditorType, editorForChildClasses);
            monoEditorType_m_IsFallback.SetValue(monoEditorType, isFallback);

            {
                IList list;

                if (kSCustomEditors.Contains(objectType)) list = (IList)kSCustomEditors[objectType];
                else
                {
                    list = (IList)Activator.CreateInstance(listMonoEditorType);
                    kSCustomEditors.Add(objectType, list);
                }

                list.Insert(0, monoEditorType);
            }

            if (canEditMultipleObject)
            {
                IList list;

                if (kSCustomMultiEditors.Contains(objectType)) list = (IList)kSCustomMultiEditors[objectType];
                else
                {
                    list = (IList)Activator.CreateInstance(listMonoEditorType);
                    kSCustomMultiEditors.Add(objectType, list);
                }

                list.Insert(0, monoEditorType);
            }
        }

        /// <summary>
        /// 重建 CustomEditor 映射关系。（不会重建通过 <see cref="CreateCustomEditor"/> 创建的映射）
        /// </summary>
        public static void Rebuild()
        {
            if (!IsAvailability()) return;

            rebuild.Invoke(null, null);
        }

        /// <summary>
        /// 向日志输出所有映射关系。
        /// </summary>
        public static void DebugEntries()
        {
            if (!IsAvailability()) return;

            foreach (DictionaryEntry entry in (IDictionary)kSCustomEditors.GetValue(null))
                Debug.Log($"key: {entry.Key} value: {entry.Value}");
        }
    }
}
#endif
