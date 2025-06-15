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
        public float Speed = 0.01f;

        private Rigidbody rb;
        private Material m_Material;
        private Color m_OriginalColor;
        private Transform m_target;
        private GameController m_GameController;
        private Vector3 VectorToPlayer;
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            m_target = GameObject.FindGameObjectWithTag("Player").transform;
            m_GameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            m_Material = GetComponent<Renderer>().material;
            m_OriginalColor = m_Material.color;
        }

        void FixedUpdate()
        {
            VectorToPlayer = (m_target.position - transform.position).normalized;
            rb.MovePosition(transform.position + VectorToPlayer * Speed);
            transform.Rotate(0, 25, 0);
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
                collider.GetComponent<PlayerCharacterController>().TakeDamage(Damage);
            }
        }

        public void TakeDamage(float damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                m_GameController.Enemies.Remove(gameObject);
                Destroy(gameObject);
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