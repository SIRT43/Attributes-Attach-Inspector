#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace StudioFortithri.AttributesAttachInspector
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

        private static Assembly _coreModule;

        // CustomEditorAttributes 相关。
        private static Type _customEditorAttributes;

        private static FieldInfo _kSCustomEditors;
        private static FieldInfo _kSCustomMultiEditors;

        private static MethodInfo _rebuild;

        // MonoEditorType 相关。
        private static Type _monoEditorType;

        private static FieldInfo _monoEditorType_m_Inspected;
        private static FieldInfo _monoEditorType_m_Inspector;
        private static FieldInfo _monoEditorType_m_EditorForChildClasses;
        private static FieldInfo _monoEditorType_m_IsFallback;

        private static Type _listMonoEditorType;

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
                        _coreModule = assembly;
                        break;
                    }
                }

                // 从核心模块获取 CustomEditorAttributes 与它的嵌套类 MonoEditorType。
                // 见 https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Editor/Mono/CustomEditorAttributes.cs。

                _customEditorAttributes = _coreModule.GetType("UnityEditor.CustomEditorAttributes");

                // 本字段用于存储所有 CustomEditor 实现映射。
                _kSCustomEditors = _customEditorAttributes.GetField("kSCustomEditors", BindingFlags.Static | BindingFlags.NonPublic);
                _kSCustomMultiEditors = _customEditorAttributes.GetField("kSCustomMultiEditors", BindingFlags.Static | BindingFlags.NonPublic);

                _rebuild = _customEditorAttributes.GetMethod("Rebuild", BindingFlags.Static | BindingFlags.NonPublic);

                _monoEditorType = _customEditorAttributes.GetNestedType("MonoEditorType", BindingFlags.NonPublic);

                // 获取构造 MonoEditorType 的字段信息。
                _monoEditorType_m_Inspected = _monoEditorType.GetField("m_InspectedType", BindingFlags.Instance | BindingFlags.Public);
                _monoEditorType_m_Inspector = _monoEditorType.GetField("m_InspectorType", BindingFlags.Instance | BindingFlags.Public);
                _monoEditorType_m_EditorForChildClasses = _monoEditorType.GetField("m_EditorForChildClasses", BindingFlags.Instance | BindingFlags.Public);
                _monoEditorType_m_IsFallback = _monoEditorType.GetField("m_IsFallback", BindingFlags.Instance | BindingFlags.Public);

                // 实现 List<MonoEditorType> 泛型类型。
                _listMonoEditorType = typeof(List<>).MakeGenericType(_monoEditorType);

                if (_kSCustomEditors == null ||
                    _kSCustomMultiEditors == null ||
                    _rebuild == null ||
                    _monoEditorType_m_Inspected == null ||
                    _monoEditorType_m_Inspector == null ||
                    _monoEditorType_m_EditorForChildClasses == null ||
                    _monoEditorType_m_IsFallback == null) throw new NullReferenceException();
            }
            catch
            {
                // 初始化发生异常，本类在此编辑器不可用。
                Availability = false;
                Debug.LogWarning($"Can't init {nameof(CustomEditorAttributes)} on this UnityEditor.");
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

            IDictionary _kSCustomEditors = (IDictionary)CustomEditorAttributes._kSCustomEditors.GetValue(null);
            IDictionary _kSCustomMultiEditors = (IDictionary)CustomEditorAttributes._kSCustomMultiEditors.GetValue(null);

            object monoEditorType = Activator.CreateInstance(_monoEditorType);
            _monoEditorType_m_Inspected.SetValue(monoEditorType, objectType);
            _monoEditorType_m_Inspector.SetValue(monoEditorType, editorType);
            _monoEditorType_m_EditorForChildClasses.SetValue(monoEditorType, editorForChildClasses);
            _monoEditorType_m_IsFallback.SetValue(monoEditorType, isFallback);

            {
                IList list;

                if (_kSCustomEditors.Contains(objectType)) list = (IList)_kSCustomEditors[objectType];
                else
                {
                    list = (IList)Activator.CreateInstance(_listMonoEditorType);
                    _kSCustomEditors.Add(objectType, list);
                }

                list.Insert(0, monoEditorType);
            }

            if (canEditMultipleObject)
            {
                IList list;

                if (_kSCustomMultiEditors.Contains(objectType)) list = (IList)_kSCustomMultiEditors[objectType];
                else
                {
                    list = (IList)Activator.CreateInstance(_listMonoEditorType);
                    _kSCustomMultiEditors.Add(objectType, list);
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

            _rebuild.Invoke(null, null);
        }

        /// <summary>
        /// 向日志输出所有映射关系。
        /// </summary>
        public static void DebugEntries()
        {
            if (!IsAvailability()) return;

            foreach (DictionaryEntry entry in (IDictionary)_kSCustomEditors.GetValue(null))
                Debug.Log($"key: {entry.Key} value: {entry.Value}");
        }
    }
}
#endif
