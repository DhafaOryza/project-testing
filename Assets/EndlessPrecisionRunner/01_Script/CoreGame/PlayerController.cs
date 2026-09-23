using UnityEngine;
using UnityEngine.SceneManagement;

namespace EndlessPrecisionRunner.CoreGame
{    
    /// <summary>
    /// Controls the player for the Endless Precision Runner prototype (Geometry Dash-style).
    /// Handles automatic forward movement, jump input, ground detection, and death on obstacle hit.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _forwardSpeed = 8f;
        [SerializeField] private float _jumpForce = 7f;

        [Header("Ground Check")]
        [SerializeField] private Transform _groundCheckPoint;
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask _groundLayer;

        private Rigidbody _rigidbody;
        private bool _isGrounded;

        /// <summary>
        /// Gets whether the player is currently touching the ground.
        /// </summary>
        public bool IsGrounded => _isGrounded;

        /// <summary>
        /// Gets the current forward speed of the player.
        /// </summary>
        public float ForwardSpeed => _forwardSpeed;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            CheckGrounded();
            HandleJumpInput();
        }

        private void FixedUpdate()
        {
            MoveForward();
        }

        /// <summary>
        /// Moves the player forward automatically on every physics frame.
        /// </summary>
        private void MoveForward()
        {
            Vector3 velocity = _rigidbody.linearVelocity;
            velocity.x = _forwardSpeed; // Moves forward on the X axis to match the orthographic 2D side view
            _rigidbody.linearVelocity = velocity;
        }

        /// <summary>
        /// Reads jump input and applies jump force only when the player is grounded.
        /// </summary>
        private void HandleJumpInput()
        {
            bool jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);

            if (jumpPressed)
            {
                // TEMPORARY: remove this line once the cause is confirmed
                Debug.Log($"[Debug] Jump pressed. IsGrounded = {_isGrounded}, GroundCheckPoint = {_groundCheckPoint.position}");
            }

            if (jumpPressed && _isGrounded)
            {
                Vector3 velocity = _rigidbody.linearVelocity;
                velocity.y = _jumpForce;
                _rigidbody.linearVelocity = velocity;
            }
        }

        /// <summary>
        /// Updates the grounded state using an overlap sphere check at the ground check point.
        /// </summary>
        private void CheckGrounded()
        {
            _isGrounded = Physics.CheckSphere(_groundCheckPoint.position, _groundCheckRadius, _groundLayer);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Obstacle"))
            {
                HandlePlayerDeath();
            }
        }

        /// <summary>
        /// Handles what happens when the player hits an obstacle.
        /// For now it simply reloads the current scene.
        /// </summary>
        private void HandlePlayerDeath()
        {
            // TODO: replace with GameManager.OnPlayerDied() once game manager exists
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}