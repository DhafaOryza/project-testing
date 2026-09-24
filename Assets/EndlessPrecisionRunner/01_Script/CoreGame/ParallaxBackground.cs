using UnityEngine;

namespace EndlessPrecisionRunner.CoreGame
{
    /// <summary>
    /// Scroll the background main texture UV offset continuously to create a parallax scrolling effect.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Render Settings")]
        [SerializeField] private Renderer _backgroundRenderer;

        [Header("Parallax Settings")]
        [SerializeField] private float _scrollSpeed = 0.2f;

        private Vector2 _savedOffset;

        /// <summary>
        /// Gets or sets the MeshRenderer component responsible for rendering the background.
        /// </summary>
        public Renderer BackgroundRenderer
        {
            get { return _backgroundRenderer; }
            set { _backgroundRenderer = value; }
        }

        /// <summary>
        /// Gets or sets the horizontal UV scrolling speed.
        /// </summary>
        public float ScrollSpeed
        {
            get { return _scrollSpeed; }
            set { _scrollSpeed = value; }
        }

        private void Start()
        {
            InitializeRenderer();
        }

        private void Update()
        {
            ScrollTexture();
        }

        /// <summary>
        /// Auto-assigns the Renderer component if not linked via Inspector.
        /// </summary>
        private void InitializeRenderer()
        {
            if (_backgroundRenderer == null)
            {
                _backgroundRenderer = GetComponent<Renderer>();
            }

            if (_backgroundRenderer != null)
            {
                _savedOffset = _backgroundRenderer.material.mainTextureOffset;
            }
        }

        /// <summary>
        /// Advances the texture UV offset frame by frame to achieve infinite horizontal movement.
        /// </summary>
        private void ScrollTexture()
        {
            if (_backgroundRenderer == null)
            {
                return;
            }

            float xOffset = Mathf.Repeat(Time.time * _scrollSpeed, 1f);
            Vector2 newOffset = new Vector2(xOffset, _savedOffset.y);

            _backgroundRenderer.material.mainTextureOffset = newOffset;
        }
    }
}