using UnityEngine;

namespace TrafficCrossing.CoreGame.Obstacle
{
    /// <summary>
    /// Defines the initial movement direction of the platform.
    /// </summary>
    public enum StartDirection
    {
        Right,
        Left
    }

    /// <summary>
    /// Moves a platform back and forth between two X-axis boundaries and transfers movement to player without parenting.
    /// </summary>
    public class MovingPlatform : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private StartDirection _initialDirection = StartDirection.Right;
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _leftBound = -5f;
        [SerializeField] private float _rightBound = 5f;

        private bool _movingRight = true;
        private Transform _playerTransform;
        private PlayerController _playerController;

        public StartDirection InitialDirection
        {
            get { return _initialDirection; }
            set { _initialDirection = value; }
        }

        public float MoveSpeed
        {
            get { return _moveSpeed; }
            set { _moveSpeed = value; }
        }

        public float LeftBound
        {
            get { return _leftBound; }
            set 
            { 
                _leftBound = value; 
                ValidateBounds();
            }
        }

        public float RightBound
        {
            get { return _rightBound; }
            set 
            { 
                _rightBound = value; 
                ValidateBounds();
            }
        }

        private void Awake()
        {
            ValidateBounds();
        }

        private void Start()
        {
            InitializeDirection();
        }

        private void Update()
        {
            MovePlatform();
        }

        private void OnValidate()
        {
            ValidateBounds();
        }

        private void ValidateBounds()
        {
            if (_leftBound > _rightBound)
            {
                float temp = _leftBound;
                _leftBound = _rightBound;
                _rightBound = temp;
            }
        }

        private void InitializeDirection()
        {
            _movingRight = (_initialDirection == StartDirection.Right);
        }

        private void MovePlatform()
        {
            Vector3 oldPosition = transform.position;
            Vector3 currentPosition = transform.position;

            // Gerakkan platform sesuai arah horizontal saat ini
            if (_movingRight)
            {
                currentPosition.x += _moveSpeed * Time.deltaTime;
                if (currentPosition.x >= _rightBound)
                {
                    currentPosition.x = _rightBound;
                    _movingRight = false;
                }
            }
            else
            {
                currentPosition.x -= _moveSpeed * Time.deltaTime;
                if (currentPosition.x <= _leftBound)
                {
                    currentPosition.x = _leftBound;
                    _movingRight = true;
                }
            }

            transform.position = currentPosition;

            // Hitung seberapa jauh platform bergeser frame ini
            Vector3 deltaPosition = currentPosition - oldPosition;

            // Jika Player berada di atas platform dan tidak sedang melompat/jatuh, ikuti gerak platform
            if (_playerTransform != null && _playerController != null)
            {
                if (!_playerController.IsHopping && !_playerController.IsFalling)
                {
                    _playerTransform.position += deltaPosition;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _playerTransform = other.transform;
                _playerController = other.GetComponent<PlayerController>();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.transform == _playerTransform)
            {
                _playerTransform = null;
                _playerController = null;
            }
        }
    }
}