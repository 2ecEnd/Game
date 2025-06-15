using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;

namespace Assets.Scripts.Gameplay
{
    public class GameController : MonoBehaviour
    {
        [Header("Enemy prefabs")]
        public GameObject EnemyPrefab;

        [Header("Spawn settings")]
        public float SpawnInterval = 30f;
        public int MaxEnemies = 5;

        public List<GameObject> Enemies;
        float m_LastTimeSpawn = Mathf.NegativeInfinity;

        private ArenaManager arenaManager;

        void Start()
        {
            arenaManager = GetComponent<ArenaManager>();
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
                float x = Random.Range(0, arenaSize + 1);
                int i = (int)(x / arenaManager.getChunkScale());
                float z = Random.Range(0, arenaSize + 1);
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
                Enemies.Add(Instantiate(EnemyPrefab, spawnPosition, Quaternion.identity));
            }
            m_LastTimeSpawn = Time.time;
        }
    }
}