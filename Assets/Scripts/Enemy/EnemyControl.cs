using Assets.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Assets.Scripts.Player;

namespace Assets.Scripts.Enemy
{
    public class EnemyControl : MonoBehaviour
    {
        public float Health = 100;
        public float ColorDuration = 0.2f;
        public Transform AttackPoint;
        public float Damage = 20f;


        private Material m_Material;
        private Color m_OriginalColor;
        private Transform m_target;
        private NavMeshAgent m_agent;
        private GameController m_GameController;
        void Start()
        {
            m_agent = GetComponent<NavMeshAgent>();
            m_target = GameObject.FindGameObjectWithTag("Player").transform;
            m_GameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            m_Material = transform.GetChild(0).GetComponent<Renderer>().material;
            m_OriginalColor = m_Material.color;
        }

        void FixedUpdate()
        {
            m_agent.SetDestination(m_target.position);

            RaycastHit hit;
            if (Physics.Raycast(
                origin: AttackPoint.position,
                direction: transform.forward,
                hitInfo: out hit,
                maxDistance: 0.2f))
            {
                HandleAttack(hit.collider);
            }
        }

        void HandleAttack(Collider collider)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                collider.gameObject.GetComponent<PlayerCharacterController>().TakeDamage(Damage);
            }
        }

        public void TakeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Destroy(this.gameObject);
                m_GameController.HandleEnemyKill();
            }
            StartCoroutine(ChangeColor());
        }

        IEnumerator ChangeColor()
        {
            m_Material.color = new Color(1f, 0.5f, 0f);
            yield return new WaitForSeconds(ColorDuration);
            m_Material.color = m_OriginalColor;
        }
    }
}