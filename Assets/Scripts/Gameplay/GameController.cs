using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;

namespace Assets.Scripts.Gameplay
{
    public class GameController : MonoBehaviour
    {
        [Header("Enemy prefabs")]
        public GameObject[] EnemyPrefabs;

        [Header("BuffBox prefabs")]
        public GameObject[] BuffBoxPrefabs;

        [Header("Spawn settings")]
        public float SpawnInterval = 30f;
        public int MaxEnemies = 5;

        public List<GameObject> Enemies;
        float m_LastTimeSpawn = Mathf.NegativeInfinity;

        private ArenaManager arenaManager;
        private GameObject enemiesGO;
        private GameObject buffBoxGO;

        void Start()
        {
            arenaManager = GetComponent<ArenaManager>();
            enemiesGO = new GameObject("Enemies");
            buffBoxGO = new GameObject("BuffBox");
            arenaManager.BuffBoxGO = buffBoxGO;
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
                SpawnBuffBox();
            }
        }

        void SpawnBuffBox()
        {
            float arenaSize = (arenaManager.getArenaSize() - 1) * arenaManager.getChunkScale();
            //while (Enemies.Count < MaxEnemies)
            {
                int y = 0;
                while (y == 0)
                {
                    float x = Random.Range(0, arenaSize);
                    int i = (int)((x + 2) / arenaManager.getChunkScale());
                    float z = Random.Range(0, arenaSize);
                    int j = (int)((z + 2) / arenaManager.getChunkScale());

                    y = arenaManager.heightMap[i, j];
                    if (y > 0)
                    {
                        y++;
                        Vector3 spawnPosition = new Vector3(x, y, z);
                        int coin = Random.Range(0, BuffBoxPrefabs.Length);
                        Instantiate(BuffBoxPrefabs[coin], spawnPosition, Quaternion.identity, buffBoxGO.transform);
                    }
                }
            }
        }
        void SpawnEnemies()
        {
            float arenaSize = (arenaManager.getArenaSize() - 1) * arenaManager.getChunkScale();
            while (Enemies.Count < MaxEnemies)
            {
                float x = Random.Range(0, arenaSize);
                int i = (int)((x + 2) / arenaManager.getChunkScale());
                float z = Random.Range(0, arenaSize);
                int j = (int)((z + 2) / arenaManager.getChunkScale());

                int y = arenaManager.heightMap[i, j];
                if (y == 0)
                {
                    if (i != 0)
                        y = Mathf.Max(y, arenaManager.heightMap[i - 1, j]);
                    if (i != arenaManager.getArenaSize() - 1)
                        y = Mathf.Max(y, arenaManager.heightMap[i + 1, j]);
                    if (j != 0)
                        y = Mathf.Max(y, arenaManager.heightMap[i, j - 1]);
                    if (j != arenaManager.getArenaSize() - 1)
                        y = Mathf.Max(y, arenaManager.heightMap[i, j + 1]);
                }
                y++;

                Vector3 spawnPosition = new Vector3(x, y, z);
                int coin = Random.Range(0, EnemyPrefabs.Length);
                Enemies.Add(Instantiate(EnemyPrefabs[coin], spawnPosition, Quaternion.identity, enemiesGO.transform));
            }
            m_LastTimeSpawn = Time.time;
        }
    }
}