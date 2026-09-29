using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrafficCrossing.CoreGame.Obstacle
{
    /// <summary>
    /// Detects when the player falls into the water gap and reloads the current scene.
    /// </summary>
    public class WaterHazard : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            CheckPlayerFall(other.gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            CheckPlayerFall(collision.gameObject);
        }

        /// <summary>
        /// Evaluates if the object entering the hazard is the player and triggers game over.
        /// </summary>
        private void CheckPlayerFall(GameObject target)
        {
            if (target.CompareTag("Player"))
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// Reloads the active scene upon failure.
        /// </summary>
        private void TriggerGameOver()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}