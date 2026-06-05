using UnityEngine;

namespace Infrastructure
{
    public static class ProjectBootstrap
    {
        private const string ProjectContextResourcePath = "Infrastructure/ProjectContext";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateProjectContext()
        {
            if (ProjectContext.Instance != null)
                return;

            ProjectContext prefab = Resources.Load<ProjectContext>(ProjectContextResourcePath);
            if (prefab != null)
            {
                Object.Instantiate(prefab);
                return;
            }

            GameObject contextObject = new GameObject("[ProjectContext]");
            contextObject.AddComponent<ProjectContext>();
        }
    }
}
