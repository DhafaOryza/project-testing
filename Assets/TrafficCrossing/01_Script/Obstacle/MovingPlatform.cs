using UnityEngine;

namespace TrafficCrossing.CoreGame.Obstacle
{
    public class MovingPlatform : BaseObstacle
    {
        [Header("Platform Bounds")]
        [SerializeField] private float _leftBound = -5f;
        [SerializeField] private float _rightBound = 5f;

        private bool _movingRight = true;
        private Transform _playerTransform;
        private PlayerController _playerController;

        public float LeftBound
        {
            get => _leftBound;
            set { _leftBound = value; ValidateBounds(); }
        }

        public float RightBound
        {
            get => _rightBound;
            set { _rightBound = value; ValidateBounds(); }
        }

        private void Awake()
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

        protected override void Move()
        {
            Vector3 oldPosition = transform.position;
            Vector3 currentPosition = transform.position;

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

            // Bawa Player jika berdiri di atas platform
            Vector3 deltaPosition = currentPosition - oldPosition;
            if (_playerTransform != null && _playerController != null)
            {
                if (!_playerController.IsHopping && !_playerController.IsFalling)
                {
                    _playerTransform.position += deltaPosition;
                }
            }
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);

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