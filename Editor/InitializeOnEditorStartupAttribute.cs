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
            public bool inited = true;
        }

        private static readonly string markFilePath =
            Path.Combine(ProjectPaths.projectSettingsPath, "Editor43_StartupMark.json");

        [InitializeOnLoadMethod]
        private static void OnLoadMethod()
        {
            StartupMark mark = new();

            if (File.Exists(markFilePath))
            {
                EditorJsonUtility.FromJsonOverwrite(File.ReadAllText(markFilePath), mark);

                if (mark.inited) return;

                mark.inited = true;
                File.WriteAllText(markFilePath, EditorJsonUtility.ToJson(mark));

                InvokeMethods();
            }
            else
            {
                File.Create(markFilePath).Close();
                File.WriteAllText(markFilePath, EditorJsonUtility.ToJson(mark));

                InvokeMethods();
            }
        }

        private static void InvokeMethods()
        {
            foreach (MethodInfo method in TypeCache.GetMethodsWithAttribute<InitializeOnEditorStartupAttribute>())
                method.Invoke(null, null);
        }
    }
}
#endif
