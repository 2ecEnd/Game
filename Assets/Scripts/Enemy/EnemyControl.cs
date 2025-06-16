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
        public float GravityDownForce = 20;
        public float MaxSpeedOnGround = 10;
        public float MovementSharpnessOnGround = 15;
        public float MaxSpeedInAir = 10;
        public float AccelerationSpeedInAir = 25;

        private Rigidbody rb;
        private Material m_Material;
        private Color m_OriginalColor;
        private Transform m_target;
        private GameController m_GameController;
        private ArenaManager m_ArenaManager;
        private Vector3 VectorToPlayer;
        private Vector3 CharacterVelocity;
        CharacterController characterController;
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            m_target = GameObject.FindGameObjectWithTag("Player").transform;
            m_GameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            m_ArenaManager = m_GameController.GetComponent<ArenaManager>();
            m_Material = GetComponent<Renderer>().material;
            m_OriginalColor = m_Material.color;
            characterController = gameObject.GetComponent<CharacterController>();
        }
        void Update()
        {
            VectorToPlayer = m_target.position - transform.position;
            VectorToPlayer = (new Vector3(VectorToPlayer.x, 0, VectorToPlayer.z)).normalized;
            transform.LookAt(new Vector3(m_target.position.x, transform.position.y, m_target.position.z));
            if (characterController.isGrounded)
            {
                Vector3 targetVelocity = VectorToPlayer * MaxSpeedOnGround;
                CharacterVelocity = Vector3.Lerp(CharacterVelocity, targetVelocity, MovementSharpnessOnGround * Time.deltaTime);
            }
            else
            {
                CharacterVelocity += VectorToPlayer * AccelerationSpeedInAir * Time.deltaTime;
                float verticalVelocity = CharacterVelocity.y;
                Vector3 horizontalVelocity = Vector3.ProjectOnPlane(CharacterVelocity, Vector3.up);
                horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, MaxSpeedInAir);
                CharacterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);
                CharacterVelocity += Vector3.down * GravityDownForce * Time.deltaTime;
            }
            characterController.Move(CharacterVelocity * Time.deltaTime);
        }

        void FixedUpdate()
        {
            RaycastHit hit;
            if (Physics.Raycast(
                origin: AttackPoint.position,
                direction: transform.forward,
                hitInfo: out hit,
                maxDistance: 0.5f))
            {
                HandleAttack(hit.collider);
            }

            if (isFall())
                TakeDamage(Health);
        }

        void HandleAttack(Collider collider)
        {
            if (collider.gameObject.CompareTag("Player"))
            {
                collider.GetComponent<PlayerCharacterController>().TakeDamage(Damage);
                collider.GetComponent<PlayerCharacterController>().CharacterVelocity = new Vector3(VectorToPlayer.x * 1000, 5, VectorToPlayer.z * 1000);
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

        bool isFall()
        {
            return transform.position.y < m_ArenaManager.getKillHeight();
        }
    }
}