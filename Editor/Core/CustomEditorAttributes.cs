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
    [InitializeOnLoad]
    public static class CustomEditorAttributes
    {
        /// <summary>
        /// �� field ���������Ƿ���������ı༭�����С�
        /// </summary>
        public readonly static bool availability;

        private readonly static Assembly coreModule;

        // CustomEditorAttributes ��ء�
        private readonly static Type customEditorAttributes;

        private readonly static FieldInfo kSCustomEditors;
        private readonly static FieldInfo kSCustomMultiEditors;

        private readonly static Action rebuild;

        // MonoEditorType ��ء�
        private readonly static Type monoEditorType;

        private readonly static FieldInfo monoEditorType_m_Inspected;
        private readonly static FieldInfo monoEditorType_m_Inspector;
        private readonly static FieldInfo monoEditorType_m_EditorForChildClasses;
        private readonly static FieldInfo monoEditorType_m_IsFallback;

        private readonly static Type listMonoEditorType;

        static CustomEditorAttributes()
        {
            availability = true;

            try
            {
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

                rebuild = (Action)customEditorAttributes
                          .GetMethod("Rebuild", BindingFlags.Static | BindingFlags.NonPublic)
                          .CreateDelegate(typeof(Action));

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
                availability = false;
                Debug.LogWarning($"Can't init {nameof(CustomEditorAttributes)} on this UnityEditor version.");
            }
        }

        private static bool IsAvailability()
        {
            if (!availability)
                Debug.LogWarning($"Invoke failed, {nameof(CustomEditorAttributes)} is not availability on this UnityEditor version.");

            return availability;
        }

        /// <summary>
        /// ��ͨ�� <see cref="CustomEditor"/> ��������� CustomEditor �󶨡�
        /// </summary>
        public static void CreateCustomEditor(Type objectType, Type editorType, bool editorForChildClasses = false, bool isFallback = false, bool canEditMultipleObject = false)
        {
            if (!IsAvailability()) return;

            if (!objectType.IsSubclassOf(typeof(UnityEngine.Object)))
                throw new ArgumentException($"{nameof(objectType)} must be subclass of UnityEngine.Object.");
            if (!editorType.IsSubclassOf(typeof(Editor)))
                throw new ArgumentException($"{nameof(editorType)} must be subclass of UnityEditor.Editor.");

            object monoEditorType = Activator.CreateInstance(CustomEditorAttributes.monoEditorType);
            monoEditorType_m_Inspected.SetValue(monoEditorType, objectType);
            monoEditorType_m_Inspector.SetValue(monoEditorType, editorType);
            monoEditorType_m_EditorForChildClasses.SetValue(monoEditorType, editorForChildClasses);
            monoEditorType_m_IsFallback.SetValue(monoEditorType, isFallback);

            {
                IList list;
                IDictionary kSCustomEditors = (IDictionary)CustomEditorAttributes.kSCustomEditors.GetValue(null);

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
                IDictionary kSCustomMultiEditors = (IDictionary)CustomEditorAttributes.kSCustomMultiEditors.GetValue(null);

                if (kSCustomMultiEditors.Contains(objectType)) list = (IList)kSCustomMultiEditors[objectType];
                else
                {
                    list = (IList)Activator.CreateInstance(listMonoEditorType);
                    kSCustomMultiEditors.Add(objectType, list);
                }

                list.Insert(0, monoEditorType);
            }
        }
        public static void CreateCustomEditor<TObject, TEditor>(bool editorForChildClasses = false, bool isFallback = false, bool canEditMultipleObject = false)
            where TObject : UnityEngine.Object where TEditor : Editor =>
            CreateCustomEditor(typeof(TObject), typeof(TEditor), editorForChildClasses, isFallback, canEditMultipleObject);

        /// <summary>
        /// �ؽ� CustomEditor ӳ���ϵ���������ؽ�ͨ�� <see cref="CreateCustomEditor"/> ������ӳ�䣩
        /// </summary>
        public static void Rebuild()
        {
            if (IsAvailability()) rebuild.Invoke();
        }

        /// <summary>
        /// ����־�������ӳ���ϵ��
        /// </summary>
        public static void DebugEntries()
        {
            if (IsAvailability())
                foreach (DictionaryEntry entry in (IDictionary)kSCustomEditors.GetValue(null))
                    Debug.Log($"key: {entry.Key} value: {entry.Value}");
        }
    }
}
#endif
