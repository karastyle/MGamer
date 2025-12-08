// <copyright file="UtilityAgentSpawner.cs" company="CarlosLab">
//     Copyright (c) CarlosLab. All rights reserved.
//     https://carloslab-ai.com
// </copyright>

using UnityEngine;

namespace CarlosLab.UtilityIntelligence.Examples
{
    public class UtilityAgentSpawner : MonoBehaviour
    {
        [SerializeField]
        private UtilityWorldController world;

        [SerializeField]
        private UtilityAgentController cyanPrefab;

        [SerializeField]
        private int cyanSpawnCount;

        [SerializeField]
        private UtilityAgentController yellowPrefab;

        [SerializeField]
        private int yellowSpawnCount;

        [SerializeField]
        private UtilityAgentController orangePrefab;

        [SerializeField]
        private int orangeSpawnCount;

        private void Start()
        {
            SpawnAgents();
        }

        private void SpawnAgents()
        {
            SpawnAgents("Cyan", cyanPrefab, cyanSpawnCount);
            SpawnAgents("Yellow", yellowPrefab, yellowSpawnCount);
            SpawnAgents("Orange", orangePrefab, orangeSpawnCount);
        }

        private void SpawnAgents(string prefixName, UtilityAgentController prefab, int spawnCount)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                Vector2 randomPoint = Random.insideUnitCircle * 90;
                Vector3 spawnPoint = new(randomPoint.x, 0, randomPoint.y);
                UtilityAgentController agentController = Instantiate(prefab, spawnPoint, Quaternion.identity);
                agentController.Name = $"{prefixName}_" + i;

                agentController.Register(world);
            }
        }
    }
}