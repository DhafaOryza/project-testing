using UnityEngine;
using System.Collections.Generic;

namespace Assets.PoolingSystem
{

    /// <summary>
    /// Groups a set of pool configurations together, e.g. all pools used by one prototype.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPoolCatalog", menuName = "Pooling/Pool Catalog")]
    public class PoolCatalogSO : ScriptableObject
    {
        [SerializeField] private List<PoolDataSO> _pools = new List<PoolDataSO>();

        /// <summary>
        /// Gets the list of pool configurations contained in this catalog.
        /// </summary>
        public IReadOnlyList<PoolDataSO> Pools => _pools;
    }
}
