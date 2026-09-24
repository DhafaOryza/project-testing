using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrafficCrossing.CoreGame
{
    /// <summary>
    /// Smoothly advances the camera forward and tracks player progress, triggering scene reload when the player leaves the viewport.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 12f, -8f);

        [Header("Movement Settings")]
        [SerializeField] private float _autoScrollSpeed = 0.8f;
        [SerializeField] private float _smoothSpeed = 5f;

        [Header("Game Over Settings")]
        [SerializeField] private float _bottomViewportThreshold = -0.05f;

        private Camera _camera;
        private float _targetZPosition;
        private bool _isGameOver;

        /// <summary>
        /// Gets or sets the target player transform to track.
        /// </summary>
        public Transform PlayerTransform
        {
            get { return _playerTransform; }
            set { _playerTransform = value; }
        }

        /// <summary>
        /// Gets whether the game over condition has been triggered.
        /// </summary>
        public bool IsGameOver
        {
            get { return _isGameOver; }
            private set { _isGameOver = value; }
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Start()
        {
            FindPlayerIfMissing();

            if (_playerTransform != null)
            {
                _targetZPosition = _playerTransform.position.z + _offset.z;
            }
        }

        private void LateUpdate()
        {
            if (_isGameOver || _playerTransform == null)
            {
                return;
            }

            MoveCamera();
            CheckPlayerOutOfBounds();
        }

        /// <summary>
        /// Automatically finds the player in the scene if omitted in Inspector.
        /// </summary>
        private void FindPlayerIfMissing()
        {
            if (_playerTransform == null)
            {
                GameObject playerObject = GameObject.FindWithTag("Player");
                if (playerObject != null)
                {
                    _playerTransform = playerObject.transform;
                }
            }
        }

        /// <summary>
        /// Advances the camera position along the Z-axis automatically and catches up when the player hops forward.
        /// </summary>
        private void MoveCamera()
        {
            _targetZPosition = Mathf.Max(_targetZPosition + _autoScrollSpeed * Time.deltaTime, _playerTransform.position.z + _offset.z);

            Vector3 desiredPosition = new Vector3(_playerTransform.position.x + _offset.x, _offset.y, _targetZPosition);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Evaluates the player's viewport position and triggers game over if the player drops behind the bottom edge.
        /// </summary>
        private void CheckPlayerOutOfBounds()
        {
            Vector3 viewportPoint = _camera.WorldToViewportPoint(_playerTransform.position);

            if (viewportPoint.y < _bottomViewportThreshold || viewportPoint.z < 0f)
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// Handles game over state and reloads the active scene.
        /// </summary>
        private void TriggerGameOver()
        {
            _isGameOver = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}