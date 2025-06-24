using Assets.Scripts.Gameplay;
using UnityEngine;
using System.Collections;
using Assets.Scripts.Player;

namespace Assets.Scripts.Enemy
{
    public class Headcrab : EnemyBase, IDamagable
    {
        public float JumpRate = 0.5f;

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

            target = GameObject.FindGameObjectWithTag("Player").transform;
            animator = GetComponent<Animator>();
            isDead = false;
        }

        void FixedUpdate()
        {
            if (!GlobalInspector.PlayerAlive)
            {
                return;
            }
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
            if (isDead || lastTimeAttacking + AttackRate > Time.time) return;

            fromBodyToPlayer = target.position - transform.position;
            distanceToPlayer = fromBodyToPlayer.magnitude;
            fromBodyToPlayer = (new Vector3(fromBodyToPlayer.x, 0, fromBodyToPlayer.z)).normalized;
            
            if (characterController.isGrounded)
                transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
            
            Vector3 targetVelocity = fromBodyToPlayer;
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

            if (distanceToPlayer < 10 && characterController.isGrounded && lastTimeJumping + JumpRate < Time.time)
            {
                animator.SetTrigger("Attack");
                lastTimeJumping = Time.time;
                characterVelocity = new Vector3(fromBodyToPlayer.x * 60, 8, fromBodyToPlayer.z * 60);
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

            lastTimeAttacking = Time.time;

            if (collider.gameObject.CompareTag("Player"))
            {
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

            if (needScored)
                GlobalInspector.EnemyStatistics[KillsStatistic].Kills++;

            gameController.Enemies.Remove(gameObject);
            StartCoroutine(Disappeare());
        }

        IEnumerator Disappeare()
        {
            // yield return new WaitForSeconds(0.1f);
            // float fallTimer = 0f;
            // while (fallTimer < 0.25)
            // {
            //     transform.Translate(Vector3.down * 5 * Time.deltaTime);
            //     fallTimer += Time.deltaTime;
            //     yield return null;
            // }

            yield return new WaitForSeconds(2f);

            float sinkTimer = 0f;
            while (sinkTimer < DisappearanceRate)
            {
                transform.Translate(Vector3.down * SinkSpeed * Time.deltaTime);
                sinkTimer += Time.deltaTime;
                yield return null;
            }

            gameController.Enemies.Remove(gameObject);
            Destroy(gameObject);
        }

        protected override void DestroyOnFall()
        {
            if (transform.position.y < arenaManager.GetKillHeight())
                Die(false);
        }
    }
}