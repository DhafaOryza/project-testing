using UnityEngine;

namespace TopDownArenaSurvival.CoreGame
{    
    /// <summary>
    /// Handles free 8-directional movement and mouse aiming for the Top-Down Arena Survival prototype.
    /// Moves and rotates the player on the X-Z plane to match the top-down perspective.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;

        private Rigidbody _rigidbody;
        private Camera _mainCamera;
        private Vector3 _moveInput;

        /// <summary>
        /// Gets the current movement direction the player is holding, as a normalized vector.
        /// </summary>
        public Vector3 MoveInput
        {
            get { return _moveInput; }
            private set { _moveInput = value; }
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            ReadMoveInput();
            RotateToMouse();
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        /// <summary>
        /// Reads WASD/arrow key input and converts it into a normalized X-Z direction.
        /// </summary>
        private void ReadMoveInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            _moveInput = new Vector3(horizontal, 0f, vertical).normalized;
        }

        /// <summary>
        /// Rotates the player transform on the X-Z plane to continuously face the mouse cursor position.
        /// </summary>
        private void RotateToMouse()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    return;
                }
            }

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (groundPlane.Raycast(ray, out float entryDistance))
            {
                Vector3 targetPoint = ray.GetPoint(entryDistance);
                Vector3 lookDirection = targetPoint - transform.position;
                lookDirection.y = 0f;

                if (lookDirection.sqrMagnitude > 0.001f)
                {
                    transform.rotation = Quaternion.LookRotation(lookDirection);
                }
            }
        }

        /// <summary>
        /// Moves the player through the Rigidbody so collision with walls/enemies still works correctly.
        /// </summary>
        private void MovePlayer()
        {
            Vector3 targetVelocity = _moveInput * _moveSpeed;
            _rigidbody.linearVelocity = new Vector3(targetVelocity.x, _rigidbody.linearVelocity.y, targetVelocity.z);
        }
    }
}