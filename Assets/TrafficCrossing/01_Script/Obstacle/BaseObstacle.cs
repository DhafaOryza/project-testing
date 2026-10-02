using UnityEngine;

namespace TrafficCrossing.CoreGame.Obstacle
{
    /// <summary>
    /// Sisi tempat obstacle muncul (Kiri atau Kanan).
    /// </summary>
    public enum SpawnSide
    {
        Left,  // Muncul dari KIRI -> Bergerak ke KANAN (Vector3.right)
        Right  // Muncul dari KANAN -> Bergerak ke KIRI (Vector3.left)
    }

    public abstract class BaseObstacle : MonoBehaviour
    {
        [Header("Base Obstacle Settings")]
        [SerializeField] protected float _moveSpeed = 5f;

        [Tooltip("Sisi kemunculan obstacle (Left = dari kiri ke kanan, Right = dari kanan ke kiri).")]
        [SerializeField] protected SpawnSide _spawnSide = SpawnSide.Left;

        public virtual float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = value;
        }

        public virtual SpawnSide SpawnSide
        {
            get => _spawnSide;
            set => _spawnSide = value;
        }

        /// <summary>
        /// Mengembalikan atau mengatur arah pergerakan (Vector3) secara otomatis berdasarkan SpawnSide.
        /// </summary>
        public virtual Vector3 MoveDirection
        {
            get
            {
                // Jika muncul dari Kiri, bergerak ke KANAN (Vector3.right)
                // Jika muncul dari Kanan, bergerak ke KIRI (Vector3.left)
                return _spawnSide == SpawnSide.Left ? Vector3.right : Vector3.left;
            }
            set
            {
                // Konversi Vektor ke Enum SpawnSide jika di-assign via kode
                if (value.x < 0f)
                {
                    _spawnSide = SpawnSide.Right;
                }
                else
                {
                    _spawnSide = SpawnSide.Left;
                }
            }
        }

        protected virtual void Update()
        {
            Move();
        }

        /// <summary>
        /// Logika pergerakan khusus yang wajib diimplementasikan oleh setiap rintangan turunan.
        /// </summary>
        protected abstract void Move();

        /// <summary>
        /// Logika ketika rintangan bersentuhan dengan Player (dapat di-override).
        /// </summary>
        protected virtual void HandlePlayerTouch(GameObject player) { }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                HandlePlayerTouch(other.gameObject);
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                HandlePlayerTouch(collision.gameObject);
            }
        }
    }
}