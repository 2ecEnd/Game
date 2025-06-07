using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.Enemy
{
    public class EnemyControl : MonoBehaviour
    {
        public int health = 100;
        public Transform target;
        private NavMeshAgent agent;
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        void FixedUpdate()
        {
            agent.SetDestination(target.position);
        }
    }
}