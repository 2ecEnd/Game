using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;

namespace Assets.Scripts.Gameplay
{
    [System.Serializable]
    public class Enemy
    {
        public GameObject Perfab;
        public int Random;
        //public int Kills;
        public int Score;
    }
    public class GameController : MonoBehaviour
    {
        [Header("Enemy prefabs")]
        public Enemy[] EnemyPrefabs;
        //public GameObject[] EnemyPrefabs;
        //public int[] EnemyRandom;

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
        private int CoinLenth;

        void Start()
        {
            GlobalInspector.EnemyStatistics = new EnemyStatistic[EnemyPrefabs.Length];
            for (int i = 0; i < EnemyPrefabs.Length; i++)
            {
                GlobalInspector.EnemyStatistics[i] = new EnemyStatistic(EnemyPrefabs[i].Score);
            }
            arenaManager = GetComponent<ArenaManager>();
            enemiesGO = new GameObject("Enemies");
            buffBoxGO = new GameObject("BuffBox");
            arenaManager.BuffBoxGO = buffBoxGO;
            CoinLenth = 0;
            for(int i = 0; i < EnemyPrefabs.Length; i++)
            {
                CoinLenth += EnemyPrefabs[i].Random;
            }
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
            float arenaSize = (arenaManager.GetArenaSize() - 1) * arenaManager.GetChunkScale();
            float x = Random.Range(0, arenaSize);
            int i = (int)((x + arenaManager.GetChunkScale() / 2) / arenaManager.GetChunkScale());
            float z = Random.Range(0, arenaSize);
            int j = (int)((z + arenaManager.GetChunkScale() / 2) / arenaManager.GetChunkScale());
            Vector3 spawnPosition = new Vector3(x, arenaManager.heightMap[i, j] + 1, z);
            int coin = Random.Range(0, BuffBoxPrefabs.Length);
            Instantiate(BuffBoxPrefabs[coin], spawnPosition, Quaternion.identity, buffBoxGO.transform);
        }
        void SpawnEnemies()
        {
            float arenaSize = (arenaManager.GetArenaSize() - 1) * arenaManager.GetChunkScale();
            while (Enemies.Count < MaxEnemies)
            {
                float x = Random.Range(0, arenaSize);
                int i = (int)((x + arenaManager.GetChunkScale() / 2) / arenaManager.GetChunkScale());
                float z = Random.Range(0, arenaSize);
                int j = (int)((z + arenaManager.GetChunkScale() / 2) / arenaManager.GetChunkScale());
                Vector3 spawnPosition = new Vector3(x, arenaManager.heightMap[i, j] + 1, z);
                int coin = Random.Range(0, CoinLenth);
                int a = 0;
                for(int c  = 0; c < EnemyPrefabs.Length; c++)
                {
                    a += EnemyPrefabs[c].Random;
                    if (coin < a)
                    {
                        GameObject newEnemy = Instantiate(EnemyPrefabs[c].Perfab, spawnPosition, Quaternion.identity, enemiesGO.transform);
                        newEnemy.GetComponent<EnemyBase>().KillsStatistic = c;
                        Enemies.Add(newEnemy);
                        break;
                    }
                }
            }
            m_LastTimeSpawn = Time.time;
        }
    }
}