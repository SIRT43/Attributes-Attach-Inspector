#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;

namespace StudioFortithri.Editor43
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class InitializeOnEditorStartupAttribute : Attribute
    {
        [Serializable]
        private class StartupMark
        {
            public bool isStartup;

            public StartupMark(bool isStartup) => this.isStartup = isStartup;
        }

        private static readonly string markFilePath = Path.Combine(ProjectPaths.projectSettingsPath, "Editor43_StartupMark.json");

        [InitializeOnLoadMethod]
        private static void OnLoadMethod()
        {
            if (!File.Exists(markFilePath))
            {
                File.Create(markFilePath).Close();
                File.WriteAllText(markFilePath, EditorJsonUtility.ToJson(new StartupMark(true)));

                InvokeMethods();
            }
            else
            {
                StartupMark mark = new(default);
                EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(markFilePath), mark);

                if (!mark.isStartup)
                {
                    File.WriteAllText(markFilePath, EditorJsonUtility.ToJson(new StartupMark(true)));
                    InvokeMethods();
                }
            }

            EditorApplication.quitting += OnEditorQuit;
        }

        private static void OnEditorQuit()
        {
            if (File.Exists(markFilePath)) File.WriteAllText(markFilePath, EditorJsonUtility.ToJson(new StartupMark(false)));
        }

        private static void InvokeMethods()
        {
            foreach (MethodInfo method in TypeCache.GetMethodsWithAttribute<InitializeOnEditorStartupAttribute>())
                method.Invoke(null, null);
        }
    }
}
#endif
