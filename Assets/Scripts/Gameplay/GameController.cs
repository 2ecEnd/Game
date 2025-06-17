using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;

namespace Assets.Scripts.Gameplay
{
    public class GameController : MonoBehaviour
    {
        [Header("Enemy prefabs")]
        public GameObject MeleeEnemyPrefab;
        public GameObject RangeEnemyPrefab;
        public GameObject CartEnemyPrefab;

        [Header("Spawn settings")]
        public float SpawnInterval = 30f;
        public int MaxEnemies = 5;

        public List<GameObject> Enemies;
        float m_LastTimeSpawn = Mathf.NegativeInfinity;

        private ArenaManager arenaManager;
        private GameObject enemiesGO;

        void Start()
        {
            arenaManager = GetComponent<ArenaManager>();
            enemiesGO = new GameObject("Enemies");
        }

        void Update()
        {
            TrySpawn();
        }

        void TrySpawn()
        {
            if (m_LastTimeSpawn + SpawnInterval < Time.time)
            {
                SpawnEnemies();
            }
        }

        void SpawnEnemies()
        {
            float arenaSize = arenaManager.getArenaSize() * arenaManager.getChunkScale();
            while (Enemies.Count < MaxEnemies)
            {
                float x = Random.Range(0, arenaSize);
                int i = (int)(x / arenaManager.getChunkScale());
                float z = Random.Range(0, arenaSize);
                int j = (int)(z / arenaManager.getChunkScale());

                float y = arenaManager.heightMap[i, j];
                if (y != 0)
                    y++;
                else
                {
                    if (i != 0)
                        y = Mathf.Max(y, arenaManager.heightMap[i - 1, j]);
                    if (i != arenaManager.getArenaSize() - 1)
                        y = Mathf.Max(y, arenaManager.heightMap[i + 1, j]);
                    if (j != 0)
                        y = Mathf.Max(y, arenaManager.heightMap[i, j - 1]);
                    if (j != arenaManager.getArenaSize() - 1)
                        y = Mathf.Max(y, arenaManager.heightMap[i, j + 1]);

                    y++;
                }

                Vector3 spawnPosition = new Vector3(x, y, z);
                int coin = Random.Range(0, 3);

                if (coin == 0)
                    Enemies.Add(Instantiate(MeleeEnemyPrefab, spawnPosition, Quaternion.identity, enemiesGO.transform));
                else if (coin == 1)
                    Enemies.Add(Instantiate(RangeEnemyPrefab, spawnPosition, Quaternion.identity, enemiesGO.transform));
                else
                    Enemies.Add(Instantiate(CartEnemyPrefab, spawnPosition, Quaternion.identity, enemiesGO.transform));
            }
            m_LastTimeSpawn = Time.time;
        }
    }
}