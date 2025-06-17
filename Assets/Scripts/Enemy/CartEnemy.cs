using Assets.Scripts.Gameplay;
using UnityEngine;
using System.Collections;
using Assets.Scripts.Player;
using Unity.Mathematics;

namespace Assets.Scripts.Enemy
{
    public class CartEnemy : EnemyBase, IDamagable
    {
        public float MaxAngularSpeed = 1;
        //public float ColorDuration = 0.2f;

        //Material material;
        //Color originalColor;

        void Start()
        {
            gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            arenaManager = gameController.GetComponent<ArenaManager>();
            characterController = gameObject.GetComponent<CharacterController>();

            target = GameObject.FindGameObjectWithTag("Player").transform;

            //material = GetComponent<Renderer>().material;
            //originalColor = material.color;
        }

        void FixedUpdate()
        {
            RaycastHit hit;
            if (Physics.Raycast(
                origin: AttackStartPoint.position,
                direction: transform.forward,
                hitInfo: out hit,
                maxDistance: 0.5f))
            {
                Attack(hit.collider);
            }

            DestroyOnFall();
        }

        void Update()
        {
            //directionToPlayer = target.position - transform.position;
            directionToPlayer = transform.worldToLocalMatrix.MultiplyPoint(target.position);
            directionToPlayer = (new Vector3(directionToPlayer.x, 0, directionToPlayer.z)).normalized;
            float angle;
            float acceleration;
            if (directionToPlayer.z > 0)
            {
                if (directionToPlayer.x > -0.05f && 0.05f < directionToPlayer.x)
                {
                    acceleration = Mathf.Clamp01(directionToPlayer.z / directionToPlayer.x);
                }
                else
                {
                    acceleration = 1;
                }
                angle = directionToPlayer.x / directionToPlayer.z;
            }
            else
            {
                acceleration = 0;
                if (directionToPlayer.x > 0)
                {
                    angle = 1;
                }
                else
                {
                    angle = -1;
                }
            }
            transform.Rotate(0, Mathf.Clamp(angle, -MaxAngularSpeed, MaxAngularSpeed), 0);
            //transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
            if (characterController.isGrounded)
            {
                Vector3 targetVelocity = transform.forward * acceleration * MaxSpeedOnGround;
                characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, MovementSharpnessOnGround * Time.deltaTime);
            }
            else
            {
                characterVelocity += transform.forward * acceleration * AccelerationSpeedInAir * Time.deltaTime;
                float verticalVelocity = characterVelocity.y;
                Vector3 horizontalVelocity = Vector3.ProjectOnPlane(characterVelocity, Vector3.up);
                horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, MaxSpeedInAir);
                characterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);
                characterVelocity += Vector3.down * GravityForce * Time.deltaTime;
            }
            characterController.Move(characterVelocity * Time.deltaTime);
        }

        protected override void Attack(Collider collider)
        {
            // if (collider.gameObject == target)
            if (collider.gameObject.CompareTag("Player"))
            {
                PlayerCharacterController player = collider.GetComponent<PlayerCharacterController>();
                player.ReceiveDamage(Damage);
                player.CharacterVelocity = new Vector3(directionToPlayer.x * 1000, 5, directionToPlayer.z * 1000); // Knockback
            }
        }

        public override void ReceiveDamage(float damage)
        {
            Health -= damage;
            //StartCoroutine(ChangeColor());

            if (Health <= 0)
                Die();
        }

        public override void Die()
        {
            gameController.Enemies.Remove(gameObject);
            Destroy(gameObject);
        }

        protected override void DestroyOnFall()
        {
            if (transform.position.y < arenaManager.getKillHeight())
                Die();
        }

        //IEnumerator ChangeColor()
        //{
        //    material.color = new Color(1f, 0.5f, 0f);
        //    yield return new WaitForSeconds(ColorDuration);
        //    material.color = originalColor;
        //}
    }
}