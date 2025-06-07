using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    public class EnemyControl : MonoBehaviour
    {
        public float Health = 100;
        private Transform m_target;
        private NavMeshAgent m_agent;
        void Start()
        {
            m_agent = GetComponent<NavMeshAgent>();
            m_target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        void FixedUpdate()
        {
            m_agent.SetDestination(m_target.position);
        }

        public void ReceivedDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Destroy(this.gameObject);
            }
        }
    }
}