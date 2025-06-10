using UnityEngine;
using System.Collections;

namespace Assets.Scripts.Gameplay
{
    public class GameController : MonoBehaviour
    {
        [Header("Enemy prefabs")]
        public GameObject EnemyPrefab;

        [Header("Spawn settings")]
        public float SpawnInterval = 30f;
        public int MaxEnemies = 5;
        public float SpawnAreaWidth = 10f;
        public float SpawnAreaLength = 10f;

        int m_currentEnemies = 0;
        float m_LastTimeSpawn = Mathf.NegativeInfinity;

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
            while (m_currentEnemies < MaxEnemies)
            {
                Vector3 spawnPosition = new Vector3(
                    transform.position.x + Random.Range(-SpawnAreaWidth / 2, SpawnAreaWidth / 2),
                    transform.position.y,
                    transform.position.z + Random.Range(-SpawnAreaLength / 2, SpawnAreaLength / 2)
                );

                Instantiate(EnemyPrefab, spawnPosition, Quaternion.identity);
                m_currentEnemies++;
            }
            m_LastTimeSpawn = Time.time;
        }

        public void HandleEnemyKill()
        {
            m_currentEnemies--;
        }
    }
}