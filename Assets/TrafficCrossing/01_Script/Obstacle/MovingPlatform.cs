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
    /// Moves a platform back and forth between two X-axis boundaries and attaches the player while standing on it.
    /// </summary>
    public class MovingPlatform : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private StartDirection _initialDirection = StartDirection.Right;
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _leftBound = -5f;
        [SerializeField] private float _rightBound = 5f;

        private bool _movingRight = true;

        /// <summary>
        /// Gets or sets the initial movement direction of the platform.
        /// </summary>
        public StartDirection InitialDirection
        {
            get { return _initialDirection; }
            set { _initialDirection = value; }
        }

        /// <summary>
        /// Gets or sets the movement speed of the platform.
        /// </summary>
        public float MoveSpeed
        {
            get { return _moveSpeed; }
            set { _moveSpeed = value; }
        }

        /// <summary>
        /// Gets or sets the left movement limit.
        /// </summary>
        public float LeftBound
        {
            get { return _leftBound; }
            set 
            { 
                _leftBound = value; 
                ValidateBounds();
            }
        }

        /// <summary>
        /// Gets or sets the right movement limit.
        /// </summary>
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

        /// <summary>
        /// Swaps left and right bounds if configured inversely to prevent movement glitches.
        /// </summary>
        private void ValidateBounds()
        {
            if (_leftBound > _rightBound)
            {
                float temp = _leftBound;
                _leftBound = _rightBound;
                _rightBound = temp;
            }
        }

        /// <summary>
        /// Configures the movement direction based on the inspector selection.
        /// </summary>
        private void InitializeDirection()
        {
            _movingRight = (_initialDirection == StartDirection.Right);
        }

        /// <summary>
        /// Moves the platform horizontally between the left and right boundaries.
        /// </summary>
        private void MovePlatform()
        {
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
        }

        private void OnCollisionEnter(Collision collision)
        {
            AttachPlayer(collision.gameObject);
        }

        private void OnCollisionExit(Collision collision)
        {
            DetachPlayer(collision.gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            AttachPlayer(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            DetachPlayer(other.gameObject);
        }

        /// <summary>
        /// Parents the player transform to the platform so it moves along with it.
        /// </summary>
        private void AttachPlayer(GameObject target)
        {
            if (target.CompareTag("Player"))
            {
                target.transform.SetParent(transform);
            }
        }

        /// <summary>
        /// Removes the platform parent from the player when stepping off.
        /// </summary>
        private void DetachPlayer(GameObject target)
        {
            if (target.CompareTag("Player"))
            {
                target.transform.SetParent(null);
            }
        }
    }
}