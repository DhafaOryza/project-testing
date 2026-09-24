using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.PoolingSystem;

namespace TopDownArenaSurvival.CoreGame
{    
    /// <summary>
    /// Central entry point for the game. Holds references to core systems
    /// and initializes them in the correct order.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Core Systems")]
        [SerializeField] private PoolManager _poolManager;
        [SerializeField] private EnemySpawner _enemySpawner;

        public PoolManager PoolManager => _poolManager;
        public EnemySpawner EnemySpawner => _enemySpawner;

        private bool _hasInitialized;
        public bool HasInitialized => _hasInitialized;

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

        /// <summary>
        /// Inisialisasi sistem utama sesuai urutan ketergantungan (Dependency).
        /// </summary>
        private void InitializeSystems()
        {
            if (_hasInitialized) return;

            _hasInitialized = true;

            FindMissingReferences();

            // 1. Inisialisasi PoolManager terlebih dahulu agar sistem lain bisa memakai pool
            if (_poolManager != null)
            {
                _poolManager.Initialize();
            }

            if (_enemySpawner != null)
            {
                _enemySpawner.Initialize(_poolManager);
            }
        }

        private void FindMissingReferences()
        {
            if (_poolManager == null)
            {
                _poolManager = FindFirstObjectByType<PoolManager>();
            }

            if (_enemySpawner == null)
            {
                _enemySpawner = FindFirstObjectByType<EnemySpawner>();
            }
        }

        public void RunCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }
}