using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.VFX;
using System.Xml.Serialization;

namespace Assets.Scripts.Gameplay
{
    [System.Serializable]
    public class Wave
    {
        public int TotalEnemies;
        public int MaxEnemies;
        public int SpawnInterval;
        public int[] EnemiesSpawnRate;
    }
    [System.Serializable]
    public class Enemy
    {
        public string Name;
        public GameObject Perfab;
        public int SpawnRate;
        public int Score;
    }
    public class GameController : MonoBehaviour
    {
        [Header("Waves settings")]
        public Wave[] Waves;
        public float WaveInterval = 10f;

        [Header("Enemy prefabs")]
        public Enemy[] EnemyPrefabs;

        [Header("BuffBox prefabs")]
        public GameObject[] BuffBoxPrefabs;

        [Header("Spawn settings")]
        public int HealthSpawnRate;
        public float SpawnInterval = 30f;
        public int MaxEnemies = 5;
        public int TotalEnemies;
        public int EnemiesSpawned;
        public int WaveNumber;
        public VisualEffect PortalEffect;

        public List<GameObject> Enemies;
        private float nextTimeSpawn = Mathf.NegativeInfinity;

        private ArenaManager arenaManager;
        private GameObject enemiesGO;
        private GameObject buffBoxGO;
        private int CoinLenth;

        void Start()
        {
            GlobalInspector.GameController = this;
            GlobalInspector.EnemyStatistics = new EnemyStatistic[EnemyPrefabs.Length];
            for (int i = 0; i < EnemyPrefabs.Length; i++)
            {
                GlobalInspector.EnemyStatistics[i] = new EnemyStatistic(EnemyPrefabs[i].Name, EnemyPrefabs[i].Score);
            }
            arenaManager = GetComponent<ArenaManager>();
            enemiesGO = new GameObject("Enemies");
            buffBoxGO = new GameObject("BuffBox");
            arenaManager.BuffBoxGO = buffBoxGO;
            WaveNumber = 0;
            NewWave();
        }

        void Update()
        {
            if (!GlobalInspector.PlayerAlive || GlobalInspector.Win)
            {
                return;
            }
            if (EnemiesSpawned < TotalEnemies)
            {
                if (Enemies.Count < MaxEnemies && nextTimeSpawn < Time.time)
                {
                    GlobalInspector.Rest = false;
                    SpawnEnemy();

                    if (Random.Range(0, 100) < HealthSpawnRate)
                        SpawnBuffBox();

                    nextTimeSpawn = Time.time + SpawnInterval;
                }
            }
            else if (Enemies.Count == 0)
            {
                if (WaveNumber + 1 < Waves.Length)
                {
                    WaveNumber++;
                    NewWave();
                }
                else
                {
                    GlobalInspector.PlayerWin();
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
        public void Restart()
        {
            WaveNumber = 0;
            NewWave();
        }
        void NewWave()
        {
            EnemiesSpawned = 0;
            CoinLenth = 0;
            for (int i = 0; i < EnemyPrefabs.Length; i++)
            {
                EnemyPrefabs[i].SpawnRate = Waves[WaveNumber].EnemiesSpawnRate[i];
                CoinLenth += EnemyPrefabs[i].SpawnRate;
            }
            SpawnInterval = Waves[WaveNumber].SpawnInterval;
            MaxEnemies = Waves[WaveNumber].MaxEnemies;
            TotalEnemies = Waves[WaveNumber].TotalEnemies;
            nextTimeSpawn = Time.time + WaveInterval;
            GlobalInspector.WaveNumber = WaveNumber;
            GlobalInspector.Rest = true;
        }

        void SpawnBuffBox()
        {
            Vector3 spawnPosition = arenaManager.GetRandomPoint();
            int coin = Random.Range(0, BuffBoxPrefabs.Length);

            float randomYRotation = Random.Range(0f, 360f);
            Quaternion randomRotation = Quaternion.Euler(0f, randomYRotation, 0f);

            Instantiate(BuffBoxPrefabs[coin], spawnPosition, randomRotation, buffBoxGO.transform);
        }

        void SpawnEnemy()
        {
            // float arenaSize = (arenaManager.GetArenaSize() - 1) * arenaManager.GetChunkScale();
            // float x = Random.Range(0, arenaSize);
            // int i = (int)((x + arenaManager.GetChunkScale() / 2) / arenaManager.GetChunkScale());
            // float z = Random.Range(0, arenaSize);
            // int j = (int)((z + arenaManager.GetChunkScale() / 2) / arenaManager.GetChunkScale());
            // Vector3 spawnPosition = new Vector3(x, arenaManager.HeightMap[i, j] + 2, z);
            Vector3 randomPoint = arenaManager.GetRandomPoint();
            Vector3 spawnPosition = new Vector3(randomPoint.x, randomPoint.y + 2, randomPoint.z);
            int coin = Random.Range(0, CoinLenth);
            int a = 0;
            for (int c = 0; c < EnemyPrefabs.Length; c++)
            {
                a += EnemyPrefabs[c].SpawnRate;
                if (coin < a)
                {
                    EnemiesSpawned++;
                    GameObject newEnemy = Instantiate(EnemyPrefabs[c].Perfab, spawnPosition, Quaternion.identity, enemiesGO.transform);
                    newEnemy.SetActive(false);
                    newEnemy.GetComponent<EnemyBase>().KillsStatistic = c;
                    Enemies.Add(newEnemy);
                    
                    StartCoroutine(SpawnVFX(spawnPosition, newEnemy));
                    break;
                }
            }
        }

        IEnumerator SpawnVFX(Vector3 spawnPosition, GameObject enemy)
        {
            VisualEffect newVFX = Instantiate(PortalEffect, spawnPosition, Quaternion.identity);
            newVFX.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            Destroy(newVFX.gameObject, 5.0f);

            yield return new WaitForSeconds(1f);
            enemy.SetActive(true);
            yield return new WaitForSeconds(1f);
            newVFX.Stop();
        }
    }
}