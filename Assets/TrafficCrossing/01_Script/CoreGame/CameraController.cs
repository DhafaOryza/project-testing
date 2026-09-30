using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrafficCrossing.CoreGame
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 2f, -10f);

        [Header("Movement Settings")]
        [SerializeField] private float _autoScrollSpeed = 0.8f;
        [SerializeField] private float _smoothTime = 0.05f;

        [Header("Game Over Settings")]
        [SerializeField] private float _bottomViewportThreshold = -0.05f;

        private Camera _camera;
        private float _targetYPosition;
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
                _targetYPosition = _playerTransform.position.y + _offset.y;
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
            // Hitung posisi Y target (tetap auto-scroll, tetapi langsung maju jika pemain melompat lebih jauh)
            _targetYPosition = Mathf.Max(_targetYPosition + _autoScrollSpeed * Time.deltaTime, _playerTransform.position.y + _offset.y);

            Vector3 desiredPosition = new Vector3(_playerTransform.position.x + _offset.x, _targetYPosition, _offset.z);

            // Menggunakan SmoothDamp agar pergerakan sangat responsif tanpa lag/tertinggal
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