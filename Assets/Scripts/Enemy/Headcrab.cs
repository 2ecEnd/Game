using Assets.Scripts.Gameplay;
using UnityEngine;
using System.Collections;
using Assets.Scripts.Player;
using System.Collections.Generic;

namespace Assets.Scripts.Enemy
{
    public class Headcrab : EnemyBase, IDamagable
    {
        public float JumpRate = 0.5f;

        [Header("SFX")]
        public AudioSource AudioSource;
        public AudioClip JumpSfx;
        public AudioClip DieSfx;
        public AudioClip BiteSfx;

        [Header("Sin settings")]
        public float frequency = 2f;
        public float magnitude = 1f;

        bool isDead;
        float lastTimeAttacking = Mathf.NegativeInfinity;
        float lastTimeJumping = Mathf.NegativeInfinity;
        float distanceToPlayer = 0;
        
        Animator animator;

        void Start()
        {
            gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            arenaManager = gameController.GetComponent<ArenaManager>();
            characterController = gameObject.GetComponent<CharacterController>();

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            target = player.transform;
            PlayerController = player.GetComponent<PlayerCharacterController>();
            animator = GetComponent<Animator>();
            isDead = false;
        }

        void FixedUpdate()
        {
            if (!GlobalInspector.PlayerAlive)
            {
                return;
            }
            fromBodyToPlayer = target.position - transform.position;
            RaycastHit hit;
            if (Physics.Raycast(
                origin: AttackStartPoint.position,
                direction: fromBodyToPlayer,
                hitInfo: out hit,
                maxDistance: 0.3f))
            {
                Attack(hit.collider);
            }
            DestroyOnFall();
        }

        void Update()
        {
            if (!GlobalInspector.PlayerAlive)
            {
                animator.speed = 0;
                //animatorRatio = 0;
                return;
            }
            else if (animator.speed == 0)
            {
                animator.speed = 1;
                //animatorRatio = 1;
            }

            if (isDead && !(Physics.Raycast(
                origin: new Vector3(transform.position.x, transform.position.y - characterController.height/1.98f, transform.position.z),
                direction: -transform.up,
                maxDistance: 0.05f)))
            {
                transform.Translate(Vector3.down * GravityForce/2 * Time.deltaTime);
            }
            
            if (isDead) return;

            fromBodyToPlayer = target.position - transform.position;
            distanceToPlayer = fromBodyToPlayer.magnitude;
            fromBodyToPlayer = (new Vector3(fromBodyToPlayer.x, 0, fromBodyToPlayer.z)).normalized;
            Vector3 perpendicular = Vector3.Cross(fromBodyToPlayer, Vector3.up).normalized;

            float sin = Mathf.Sin(Time.time * frequency) * magnitude;
            Vector3 offset = perpendicular * sin;

            Vector3 targetVelocity = fromBodyToPlayer + offset;
            if (characterController.isGrounded)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetVelocity);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
                //transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
            }
            
            float verticalVelocity = characterVelocity.y - GravityForce * Time.deltaTime;
            if (characterController.isGrounded)
            {
                characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity * MaxSpeedOnGround, MovementSharpnessOnGround * Time.deltaTime);
                verticalVelocity = -GravityForce * 0.1f;
            }
            else
            {
                characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity * MaxSpeedInAir, AccelerationSpeedInAir * Time.deltaTime);
            }

            if (distanceToPlayer < 8 && characterController.isGrounded && lastTimeJumping + JumpRate < Time.time)
            {
                animator.SetTrigger("Attack");
                lastTimeJumping = Time.time;
                AudioSource.PlayOneShot(JumpSfx);

                Vector3 predictedPoint = PredictPlayerPosition();
                fromBodyToPlayer = predictedPoint - transform.position;
                fromBodyToPlayer = (new Vector3(fromBodyToPlayer.x, fromBodyToPlayer.y, fromBodyToPlayer.z)).normalized;
                transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

                characterVelocity = new Vector3(fromBodyToPlayer.x * 60, fromBodyToPlayer.y + 7, fromBodyToPlayer.z * 60);
            }
            else
            {
                characterVelocity = new Vector3(characterVelocity.x, verticalVelocity, characterVelocity.z);
            }

            animator.SetBool("IsGrounded", characterController.isGrounded ? true : false);
            
            characterController.Move(characterVelocity * Time.deltaTime);
        }

        protected void Attack(Collider collider)
        {
            if (isDead || lastTimeAttacking + AttackRate > Time.time) return;
            // if (collider.gameObject == target)

            if (collider.gameObject.CompareTag("Player"))
            {
                AudioSource.PlayOneShot(BiteSfx);
                lastTimeAttacking = Time.time;
                PlayerCharacterController player = collider.GetComponent<PlayerCharacterController>();
                player.ReceiveDamage(Damage);
                player.ExtraVelocity = new Vector3(fromBodyToPlayer.x * 10, 20, fromBodyToPlayer.z * 10); // Knockback
            }
        }

        public override void ReceiveDamage(float damage)
        {
            if (isDead) return;

            Health -= damage;

            if (Health <= 0)
                Die();
        }

        public override void Die(bool needScored = true)
        {
            isDead = true;
            animator.SetTrigger("Die");
            characterController.enabled = false;
            AudioSource.PlayOneShot(DieSfx);

            if (needScored)
                GlobalInspector.EnemyStatistics[KillsStatistic].Kills++;

            gameController.Enemies.Remove(gameObject);
            StartCoroutine(Disappeare());
        }

        protected override void DestroyOnFall()
        {
            if (transform.position.y < arenaManager.GetKillHeight())
                Die(false);
        }
    }
}