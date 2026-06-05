using UnityEngine;

namespace Infrastructure
{
    [DefaultExecutionOrder(-10000)]
    public sealed class ProjectContext : MonoBehaviour
    {
        [Header("Global Static Data")]
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private ItemCrafts itemCrafts;
        [SerializeField] private WindowStaticData windowStaticData;

        private ServiceContainer _services;
        private WindowService _windowService;

        public static ProjectContext Instance { get; private set; }
        public static bool IsReady => Instance != null && Instance._services != null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            InstallBindings();
        }

        public static TService Get<TService>()
        {
            EnsureReady();
            return Instance._services.Resolve<TService>();
        }

        public static bool TryGet<TService>(out TService service)
        {
            if (!IsReady)
            {
                service = default;
                return false;
            }

            return Instance._services.TryResolve(out service);
        }

        public void ConfigureStaticData(
            ItemDatabase configuredItemDatabase,
            ItemCrafts configuredItemCrafts,
            WindowStaticData configuredWindowStaticData = null)
        {
            itemDatabase = configuredItemDatabase != null ? configuredItemDatabase : itemDatabase;
            itemCrafts = configuredItemCrafts != null ? configuredItemCrafts : itemCrafts;
            windowStaticData = configuredWindowStaticData != null ? configuredWindowStaticData : windowStaticData;

            RegisterStaticData();
            _windowService.Configure(windowStaticData);
        }

        private void InstallBindings()
        {
            _services = new ServiceContainer();

            LoadResourceFallbacks();
            RegisterStaticData();

            GameplayFeedbackService feedbackService = new GameplayFeedbackService();
            _services.Register<IGameplayFeedback>(feedbackService);
            _services.Register<IGameplayFeedbackSource>(feedbackService);

            LocalPlayerProvider localPlayerProvider = new LocalPlayerProvider();
            _services.Register<ILocalPlayerProvider>(localPlayerProvider);

            _windowService = new WindowService(windowStaticData);
            _services.Register<IWindowService>(_windowService);
        }

        private void RegisterStaticData()
        {
            if (itemDatabase != null)
                _services.Replace(itemDatabase);

            if (itemCrafts != null)
                _services.Replace(itemCrafts);

            if (windowStaticData != null)
                _services.Replace(windowStaticData);
        }

        private void LoadResourceFallbacks()
        {
            if (itemDatabase == null)
                itemDatabase = LoadResourceFallback<ItemDatabase>("ItemDatabase");

            if (itemCrafts == null)
                itemCrafts = LoadResourceFallback<ItemCrafts>("ItemCrafts");

            if (windowStaticData == null)
                windowStaticData = LoadResourceFallback<WindowStaticData>("WindowStaticData");
        }

        private static TResource LoadResourceFallback<TResource>(string resourcePath)
            where TResource : Object
        {
            TResource resource = Resources.Load<TResource>(resourcePath);
            if (resource != null)
                return resource;

            TResource[] resources = Resources.LoadAll<TResource>(string.Empty);
            if (resources.Length == 0)
                return null;

            foreach (TResource candidate in resources)
            {
                if (candidate.name == resourcePath)
                    return candidate;
            }

            return resources[0];
        }

        private static void EnsureReady()
        {
            if (!IsReady)
            {
                throw new System.InvalidOperationException(
                    "ProjectContext is not ready. Ensure ProjectBootstrap can create it before scene load.");
            }
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            _services?.Dispose();
            _services = null;
            Instance = null;
        }
    }
}
