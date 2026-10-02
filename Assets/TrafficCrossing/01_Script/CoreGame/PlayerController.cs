using TrafficCrossing.CoreGame.Obstacle;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrafficCrossing.CoreGame
{
    /// <summary>
    /// Handles grid-based hop movement for the Traffic Crossing prototype using mouse input.
    /// Includes ground detection and Z-axis fall mechanism.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float _gridSize = 1f;

        [Header("Hop Animation Settings")]
        [SerializeField] private float _hopDuration = 0.15f;
        [SerializeField] private float _hopHeight = 0.5f;

        [Header("Mouse Input Settings")]
        [SerializeField] private float _swipeThreshold = 50f;

        [Header("Ground & Fall Settings")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _targetZ = 10f;

        private bool _isGameOver = false;
        private bool _isOnPlatform = false; 
        private bool _isHopping;
        private bool _isFalling;
        private Vector3 _startMousePosition;
        private bool _hasSwiped;

        public float GridSize
        {
            get { return _gridSize; }
            private set { _gridSize = value; }
        }

        public bool IsHopping
        {
            get { return _isHopping; }
            private set { _isHopping = value; }
        }

        public bool IsFalling
        {
            get { return _isFalling; }
            private set { _isFalling = value; }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Platform"))
            {
                _isOnPlatform = true;
            }
            else if (other.CompareTag("Obstacle"))
            {
                StartCoroutine(CheckHazardCollision());
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Platform"))
            {
                _isOnPlatform = false;
            }
        }

        /// <summary>
        /// Menunda pengecekan hingga seluruh Trigger pada frame ini selesai dieksekusi Unity.
        /// </summary>
        private IEnumerator CheckHazardCollision()
        {
            yield return new WaitForEndOfFrame();

            if (!_isOnPlatform && !_isFalling)
            {
                StartCoroutine(FallToZPositive());
            }
        }

        private void Update()
        {
            ReadMouseInput();
        }

        private void ReadMouseInput()
        {
            if (_isHopping || _isFalling)
            {
                return;
            }
        
            if (Input.GetMouseButtonDown(0))
            {
                _startMousePosition = Input.mousePosition;
                _hasSwiped = false;
            }
        
            if (Input.GetMouseButton(0) && !_hasSwiped)
            {
                float deltaX = Input.mousePosition.x - _startMousePosition.x;
        
                if (Mathf.Abs(deltaX) >= _swipeThreshold)
                {
                    _hasSwiped = true;
                    Vector3 direction = deltaX > 0f ? Vector3.right : Vector3.left;
                    TryHop(direction);
                }
            }
        
            if (Input.GetMouseButtonUp(0))
            {
                if (!_hasSwiped)
                {
                    TryHop(Vector3.up);
                }
            }
        }

        private void TryHop(Vector3 direction)
        {
            Vector3 targetPosition = transform.position + direction * _gridSize;
            StartCoroutine(HopToPosition(targetPosition, direction));
        }

        private IEnumerator HopToPosition(Vector3 targetPosition, Vector3 direction)
        {
            _isHopping = true;

            Vector3 startPosition = transform.position;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            float elapsedTime = 0f;

            while (elapsedTime < _hopDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / _hopDuration;

                Vector3 flatPosition = Vector3.Lerp(startPosition, targetPosition, progress);
                float hopArc = Mathf.Sin(progress * Mathf.PI) * _hopHeight;

                transform.position = new Vector3(flatPosition.x, flatPosition.y, startPosition.z - hopArc);

                yield return null;
            }

            transform.position = SnapToGrid(targetPosition);
            
            // Periksa apakah mendarat di atas platform yang valid
            CheckGroundStatus();
        }

        /// <summary>
        /// Checks if the player landed on a platform layer collider.
        /// </summary>
        private void CheckGroundStatus()
        {
            Collider2D hit = Physics2D.OverlapPoint(transform.position, _groundLayer);

            if (hit == null)
            {
                // Tidak ada collider di bawah player -> Jatuh ke sumbu +Z
                StartCoroutine(FallToZPositive());
            }
            else
            {
                _isHopping = false;
            }
        }

        /// <summary>
        /// Animates player falling into the positive Z axis (background depth) and reloads scene.
        /// </summary>
        private IEnumerator FallToZPositive()
        {
            _isFalling = true;

            Vector3 startPos = transform.position;
            Vector3 targetPos = new Vector3(startPos.x, startPos.y, _targetZ);
            Vector3 startScale = transform.localScale;

            float duration = 0.3f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;

                // Geser ke Z positif & kecilkan skala sprite
                transform.position = Vector3.Lerp(startPos, targetPos, progress);
                transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);

                yield return null;
            }

            // Reload scene saat selesai jatuh
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private Vector3 SnapToGrid(Vector3 position)
        {
            float snappedX = Mathf.Round(position.x / _gridSize) * _gridSize;
            float snappedY = Mathf.Round(position.y / _gridSize) * _gridSize;
            return new Vector3(snappedX, snappedY, position.z);
        }
    }
}