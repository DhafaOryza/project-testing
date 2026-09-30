using UnityEngine;

namespace Assets.PoolingSystem
{    
    /// <summary>
    /// Unique identifier asset used as a key for a specific pool.
    /// Create one asset per poolable prefab type (e.g. "Obstacle_Spike", "Obstacle_Wall").
    /// </summary>
    [CreateAssetMenu(fileName = "NewPoolId", menuName = "Pooling/Pool Id")]
    public class PoolIdSO : ScriptableObject
    {
    }
}