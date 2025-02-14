#if UNITY_EDITOR
using System.IO;
using UnityEngine;

namespace StudioFortithri.Editor43
{
    public static class ProjectPaths
    {
        public static readonly string projectRootPath = Directory.GetParent(Application.dataPath).FullName;
        public static readonly string projectSettingsPath = Path.Combine(projectRootPath, "ProjectSettings");
    }
}
#endif
