using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.PoolingSystem;

namespace EndlessPrecisionRunner.CoreGame
{    
    /// <summary>
    /// Central entry point for the game. Holds references to the other core systems
    /// and initializes them in the right order, so those systems don't need their own singletons.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// Gets the active GameManager instance for this scene.
        /// </summary>
        public static GameManager Instance { get; private set; }

        [Header("Core Systems")]
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private ObstacleSpawner _obstacleSpawner;
        [SerializeField] private PlatformLooper _platformLooper;
        [SerializeField] private PlayerController _playerController;

        private bool _hasInitialized;

        /// <summary>
        /// Gets whether all core systems have finished initializing.
        /// </summary>
        public bool HasInitialized => _hasInitialized;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            InitializeSystems();
        }

        /// <summary>
        /// Re-runs initialization whenever a new scene finishes loading.
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _hasInitialized = false;
            InitializeSystems();
        }

        /// <summary>
        /// Fills in any missing system references, then initializes the systems that need it.
        /// </summary>
        private void InitializeSystems()
        {
            if (_hasInitialized)
            {
                return;
            }

            _hasInitialized = true;

            FindMissingReferences();

            if (_poolManager != null)
            {
                _poolManager.Initialize();
            }
        }

        /// <summary>
        /// Fills in any system reference that wasn't assigned in the Inspector.
        /// </summary>
        private void FindMissingReferences()
        {
            if (_poolManager == null)
            {
                _poolManager = FindFirstObjectByType<PoolManager>();
            }

            if (_obstacleSpawner == null)
            {
                _obstacleSpawner = FindFirstObjectByType<ObstacleSpawner>();
            }

            if (_platformLooper == null)
            {
                _platformLooper = FindFirstObjectByType<PlatformLooper>();
            }

            if (_playerController == null)
            {
                _playerController = FindFirstObjectByType<PlayerController>();
            }
        }

        /// <summary>
        /// Starts a coroutine on behalf of a non-MonoBehaviour class.
        /// </summary>
        public void RunCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
}