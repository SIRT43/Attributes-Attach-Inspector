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
    /// ͨ�� System.Reflection �� UnityEditor.CustomEditorAttributes ��ȡ��ԭ��ɼ���Ϊ internal��<br></br>
    /// ����ּ��ʵ�ָ������ɵı༭���������չ��
    /// </summary>
    public static class CustomEditorAttributes
    {
        /// <summary>
        /// �� property ���������Ƿ���������ı༭�����С�
        /// </summary>
        public static bool Availability { get; private set; }

        private static Assembly _coreModule;

        // CustomEditorAttributes ��ء�
        private static Type _customEditorAttributes;

        private static FieldInfo _kSCustomEditors;
        private static FieldInfo _kSCustomMultiEditors;

        private static MethodInfo _rebuild;

        // MonoEditorType ��ء�
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

                // �ӳ����л�ȡ����ģ�顣
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == "UnityEditor.CoreModule")
                    {
                        _coreModule = assembly;
                        break;
                    }
                }

                // �Ӻ���ģ���ȡ CustomEditorAttributes ������Ƕ���� MonoEditorType��
                // �� https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Editor/Mono/CustomEditorAttributes.cs��

                _customEditorAttributes = _coreModule.GetType("UnityEditor.CustomEditorAttributes");

                // ���ֶ����ڴ洢���� CustomEditor ʵ��ӳ�䡣
                _kSCustomEditors = _customEditorAttributes.GetField("kSCustomEditors", BindingFlags.Static | BindingFlags.NonPublic);
                _kSCustomMultiEditors = _customEditorAttributes.GetField("kSCustomMultiEditors", BindingFlags.Static | BindingFlags.NonPublic);

                _rebuild = _customEditorAttributes.GetMethod("Rebuild", BindingFlags.Static | BindingFlags.NonPublic);

                _monoEditorType = _customEditorAttributes.GetNestedType("MonoEditorType", BindingFlags.NonPublic);

                // ��ȡ���� MonoEditorType ���ֶ���Ϣ��
                _monoEditorType_m_Inspected = _monoEditorType.GetField("m_InspectedType", BindingFlags.Instance | BindingFlags.Public);
                _monoEditorType_m_Inspector = _monoEditorType.GetField("m_InspectorType", BindingFlags.Instance | BindingFlags.Public);
                _monoEditorType_m_EditorForChildClasses = _monoEditorType.GetField("m_EditorForChildClasses", BindingFlags.Instance | BindingFlags.Public);
                _monoEditorType_m_IsFallback = _monoEditorType.GetField("m_IsFallback", BindingFlags.Instance | BindingFlags.Public);

                // ʵ�� List<MonoEditorType> �������͡�
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
                // ��ʼ�������쳣�������ڴ˱༭�������á�
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
        /// ��ͨ�� <see cref="CustomEditor"/> ��������� CustomEditor �󶨡�
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
        /// �ؽ� CustomEditor ӳ���ϵ���������ؽ�ͨ�� <see cref="CreateCustomEditor"/> ������ӳ�䣩
        /// </summary>
        public static void Rebuild()
        {
            if (!IsAvailability()) return;

            _rebuild.Invoke(null, null);
        }

        /// <summary>
        /// ����־�������ӳ���ϵ��
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
