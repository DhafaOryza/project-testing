using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrafficCrossing.CoreGame
{
    /// <summary>
    /// Tracks player progress smoothly with automatic offset calculation matching initial Transform setup.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform _playerTransform;
        
        [Tooltip("Jika dicentang, offset dihitung otomatis dari jarak Kamera dan Player di Editor saat Play.")]
        [SerializeField] private bool _autoCalculateOffset = true;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 3f, -10f);

        [Header("Movement Settings")]
        [SerializeField] private float _autoScrollSpeed = 0.8f;
        [SerializeField] private float _smoothTime = 0.05f;

        [Header("Game Over Settings")]
        [SerializeField] private float _bottomViewportThreshold = -0.05f;

        private Camera _camera;
        private float _highestYPosition;
        private bool _isGameOver;
        private Vector3 _velocity = Vector3.zero;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Start()
        {
            FindPlayerIfMissing();

            if (_playerTransform != null)
            {
                // Menghitung offset otomatis berdasarkan posisi Kamera dan Player di Editor
                if (_autoCalculateOffset)
                {
                    _offset = transform.position - _playerTransform.position;
                }

                _highestYPosition = transform.position.y;
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

        private void MoveCamera()
        {
            float targetY;

            _highestYPosition = Mathf.Max(_highestYPosition + _autoScrollSpeed * Time.deltaTime, _playerTransform.position.y + _offset.y);
            targetY = _highestYPosition;

            // Target posisi kamera mengikuti offset awal
            Vector3 desiredPosition = new Vector3(_playerTransform.position.x + _offset.x, targetY, _playerTransform.position.z + _offset.z);

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, _smoothTime);
        }

        private void CheckPlayerOutOfBounds()
        {
            Vector3 viewportPoint = _camera.WorldToViewportPoint(_playerTransform.position);

            if (viewportPoint.y < _bottomViewportThreshold)
            {
                TriggerGameOver();
            }
        }

        private void TriggerGameOver()
        {
            _isGameOver = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}