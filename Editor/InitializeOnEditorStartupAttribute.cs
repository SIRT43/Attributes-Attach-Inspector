#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;

namespace StudioFortithri.Editor43
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class InitializeOnEditorStartupAttribute : Attribute { }

    internal static class InitializeOnEditorStartupAttributes
    {
        [Serializable]
        private class StartupMark
        {
            public bool inited;
        }

        private static readonly string markFilePath =
            Path.Combine(ProjectPaths.projectSettingsPath, "Editor43_StartupMark.json");

        [InitializeOnLoadMethod]
        private static void OnLoadMethod()
        {
            StartupMark mark = new() { inited = true };

            if (!File.Exists(markFilePath))
            {
                File.Create(markFilePath).Close();
                // ֱ�� ToJson �����ǽ��� StartupMark.inited ��ת��������Ϊ������ inited �ѳ�ʼ��Ϊ true��
                File.WriteAllText(markFilePath, EditorJsonUtility.ToJson(mark));

                InvokeMethods();
            }
            else
            {
                // ��д mark��������������в���Ҫ���� mark ��ԭֵ��
                EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(markFilePath), mark);

                if (!mark.inited)
                {
                    mark.inited = true;
                    File.WriteAllText(markFilePath, EditorJsonUtility.ToJson(mark));

                    InvokeMethods();
                }
            }

            // Editor �˳������� StartupMark ��״̬Ϊ false��
            EditorApplication.quitting += OnEditorQuit;
        }

        private static void OnEditorQuit()
        {
            if (!File.Exists(markFilePath)) return;

            File.WriteAllText(markFilePath, EditorJsonUtility.ToJson(new StartupMark() { inited = false }));
        }

        private static void InvokeMethods()
        {
            foreach (MethodInfo method in TypeCache.GetMethodsWithAttribute<InitializeOnEditorStartupAttribute>())
                method.Invoke(null, null);
        }
    }
}
#endif
