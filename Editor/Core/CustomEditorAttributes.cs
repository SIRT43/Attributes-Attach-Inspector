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
    /// ͨ�� System.Reflection �� UnityEditor.CustomEditorAttributes ��ȡ��ԭ��ɼ���Ϊ internal��<br></br>
    /// ����ּ��ʵ�ָ������ɵı༭���������չ��
    /// </summary>
    public static class CustomEditorAttributes
    {
        /// <summary>
        /// �� property ���������Ƿ���������ı༭�����С�
        /// </summary>
        public static bool Availability { get; private set; }

        private static Assembly coreModule;

        // CustomEditorAttributes ��ء�
        private static Type customEditorAttributes;

        private static FieldInfo kSCustomEditors;
        private static FieldInfo kSCustomMultiEditors;

        private static MethodInfo rebuild;

        // MonoEditorType ��ء�
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

                // �ӳ����л�ȡ����ģ�顣
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == "UnityEditor.CoreModule")
                    {
                        coreModule = assembly;
                        break;
                    }
                }

                // �Ӻ���ģ���ȡ CustomEditorAttributes ������Ƕ���� MonoEditorType ��ص� Reflection ��Ϣ��
                // �� https://github.com/Unity-Technologies/UnityCsReference/blob/2022.3/Editor/Mono/CustomEditorAttributes.cs��

                customEditorAttributes = coreModule.GetType("UnityEditor.CustomEditorAttributes");

                // ���������ֶ����ڴ洢���� CustomEditor ʵ��ӳ�䡣
                kSCustomEditors = customEditorAttributes.GetField("kSCustomEditors", BindingFlags.Static | BindingFlags.NonPublic);
                kSCustomMultiEditors = customEditorAttributes.GetField("kSCustomMultiEditors", BindingFlags.Static | BindingFlags.NonPublic);

                rebuild = customEditorAttributes.GetMethod("Rebuild", BindingFlags.Static | BindingFlags.NonPublic);

                monoEditorType = customEditorAttributes.GetNestedType("MonoEditorType", BindingFlags.NonPublic);

                // ��ȡ���� MonoEditorType ���ֶ���Ϣ��
                monoEditorType_m_Inspected = monoEditorType.GetField("m_InspectedType", BindingFlags.Instance | BindingFlags.Public);
                monoEditorType_m_Inspector = monoEditorType.GetField("m_InspectorType", BindingFlags.Instance | BindingFlags.Public);
                monoEditorType_m_EditorForChildClasses = monoEditorType.GetField("m_EditorForChildClasses", BindingFlags.Instance | BindingFlags.Public);
                monoEditorType_m_IsFallback = monoEditorType.GetField("m_IsFallback", BindingFlags.Instance | BindingFlags.Public);

                // ʵ�� List<MonoEditorType> �������͡�
                listMonoEditorType = typeof(List<>).MakeGenericType(monoEditorType);

                // �����޷���ȡ�������ô�������޷��������׳��쳣������ Catch ������
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
                // ��ʼ�������쳣�������ڴ˱༭�������á�
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
        /// ��ͨ�� <see cref="CustomEditor"/> ��������� CustomEditor �󶨡�
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
        /// �ؽ� CustomEditor ӳ���ϵ���������ؽ�ͨ�� <see cref="CreateCustomEditor"/> ������ӳ�䣩
        /// </summary>
        public static void Rebuild()
        {
            if (!IsAvailability()) return;

            rebuild.Invoke(null, null);
        }

        /// <summary>
        /// ����־�������ӳ���ϵ��
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
