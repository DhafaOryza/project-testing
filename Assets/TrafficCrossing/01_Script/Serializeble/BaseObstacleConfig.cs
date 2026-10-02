using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.PoolingSystem;
using TrafficCrossing.CoreGame.Struct;

namespace TrafficCrossing.CoreGame.Serializeble
{
    [Serializable]
    public abstract class BaseObstacleConfig
    {
        public abstract void SpawnObstacle(
            PoolManager poolManager, 
            Vector3 position, 
            GameObject chunkInstance, 
            int gridOffset, 
            List<SpawnedObstacle> spawnedObstacles);
    }
}