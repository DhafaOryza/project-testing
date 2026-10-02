using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.PoolingSystem;

namespace TrafficCrossing.CoreGame
{    
    /// <summary>
    /// Central entry point for the game. Holds references to core systems.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Core Systems")]
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private TerrainSpawner _terrainSpawner;

        private bool _hasInitialized;

        public bool HasInitialized => _hasInitialized;
        public PoolManager PoolManager => _poolManager;
        public PlayerController PlayerController => _playerController;
        public TerrainSpawner TerrainSpawner => _terrainSpawner;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
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

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _hasInitialized = false;
            InitializeSystems();
        }

        private void InitializeSystems()
        {
            if (_hasInitialized) return;

            _hasInitialized = true;
            FindMissingReferences();

            if (_poolManager != null && !_poolManager.IsInitialized)
            {
                _poolManager.Initialize();
            }

            if (_terrainSpawner != null)
            {
                Transform playerTransform = _playerController != null ? _playerController.transform : null;
                _terrainSpawner.Initialize(_poolManager, playerTransform);
            }
        }

        private void FindMissingReferences()
        {
            if (_poolManager == null)
            {
                _poolManager = FindFirstObjectByType<PoolManager>();
            }

            if (_playerController == null)
            {
                _playerController = FindFirstObjectByType<PlayerController>();
            }

            if (_terrainSpawner == null)
            {
                _terrainSpawner = FindFirstObjectByType<TerrainSpawner>();
            }
        }

        /// <summary>
        /// Global trigger untuk Game Over.
        /// </summary>
        public void TriggerGameOver()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void RunCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
}