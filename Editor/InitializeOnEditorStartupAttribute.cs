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
            public bool isStartup = true;
        }

        private static readonly string markFilePath = Path.Combine(ProjectPaths.projectSettingsPath, "Editor43_StartupMark.json");
        private static readonly StartupMark single = new();

        [InitializeOnLoadMethod]
        private static void OnLoadMethod()
        {
            if (!File.Exists(markFilePath))
            {
                File.Create(markFilePath).Close();
                File.WriteAllText(markFilePath, EditorJsonUtility.ToJson(single));

                InvokeMethods();
            }
            else
            {
                EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(markFilePath), single);

                if (!single.isStartup)
                {
                    single.isStartup = true;
                    File.WriteAllText(markFilePath, EditorJsonUtility.ToJson(single));

                    InvokeMethods();
                }
            }

            EditorApplication.quitting += OnEditorQuit;
        }

        private static void OnEditorQuit()
        {
            if (!File.Exists(markFilePath)) return;

            single.isStartup = false;
            File.WriteAllText(markFilePath, EditorJsonUtility.ToJson(single));
        }

        private static void InvokeMethods()
        {
            foreach (MethodInfo method in TypeCache.GetMethodsWithAttribute<InitializeOnEditorStartupAttribute>())
                method.Invoke(null, null);
        }
    }
}
#endif
