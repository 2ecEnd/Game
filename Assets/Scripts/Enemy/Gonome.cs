using Assets.Scripts.Gameplay;
using UnityEngine;
using System.Collections;
using Assets.Scripts.Player;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Assets.Scripts.Enemy
{
    public class Gonome : EnemyBase, IDamagable
    {
        [Header("SFX")]
        public AudioSource AudioSource;
        public AudioSource BreatheSource;
        public List<AudioClip> FootstepsSfx;
        public List<AudioClip> AttacksSfx;
        public List<AudioClip> IdlesSfx;
        public List<AudioClip> DieSfx;
        public float FootstepSfxFrequency = 1f;
        public float IdleSfxFrequency = 10f;
        

        bool isDead;
        ChaseType chaseType;
        float lastTimeAttacking = Mathf.NegativeInfinity;
        float lastTimePlayingIdle = Mathf.NegativeInfinity;
        float footstepDistanceCounter;
        Animator animator;

        void Start()
        {
            gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            arenaManager = gameController.GetComponent<ArenaManager>();
            characterController = gameObject.GetComponent<CharacterController>();

            target = GameObject.FindGameObjectWithTag("Player").transform;
            PlayerController = target.GetComponent<PlayerCharacterController>();
            animator = GetComponent<Animator>();
            isDead = false;

            chaseType = Random.Range(0, 2) == 0 ? ChaseType.Direct : ChaseType.Intercept;
            if (chaseType == ChaseType.Intercept)
                InterceptSpeed = Random.Range(20, 30);
        }

        void FixedUpdate()
        {
            if (!GlobalInspector.PlayerAlive)
            {
                return;
            }
            RaycastHit hit;
            fromBodyToPlayer = target.position - transform.position;
            if (Physics.Raycast(
                origin: AttackStartPoint.position,
                direction: fromBodyToPlayer,
                hitInfo: out hit,
                maxDistance: 1.5f))
            {
                Attack(hit.collider);
            }

            DestroyOnFall();
        }

        void Update()
        {
            if (lastTimePlayingIdle + IdleSfxFrequency + Random.Range(0, 5) < Time.time)
            {
                AudioSource.PlayOneShot(IdlesSfx[Random.Range(0, IdlesSfx.Count)]);
                lastTimePlayingIdle = Time.time;
            }

            if (!GlobalInspector.PlayerAlive)
            {
                animator.speed = 0;
                AudioSource.volume = 0;
                if (BreatheSource != null)
                    BreatheSource.volume = 0;
                //animatorRatio = 0;
                return;
            }
            else if (animator.speed == 0)
            {
                animator.speed = 1;
                AudioSource.volume = 1;
                if (BreatheSource != null)
                    BreatheSource.volume = 1;
                //animatorRatio = 1;
            }
            if (isDead || lastTimeAttacking + AttackRate > Time.time) return;

            fromBodyToPlayer = target.position - transform.position;
            if (chaseType == ChaseType.Intercept && fromBodyToPlayer.magnitude > 5)
            {
                Vector3 predictedPos = PredictPlayerPosition();
                fromBodyToPlayer = predictedPos - transform.position;
            }

            fromBodyToPlayer = (new Vector3(fromBodyToPlayer.x, 0, fromBodyToPlayer.z)).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(fromBodyToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);

            //transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
            Vector3 targetVelocity = fromBodyToPlayer;
            float verticalVelocity = characterVelocity.y - GravityForce * Time.deltaTime;
            if (characterController.isGrounded)
            {
                characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity * MaxSpeedOnGround, MovementSharpnessOnGround * Time.deltaTime);
                verticalVelocity = -GravityForce * 0.1f;

                if (footstepDistanceCounter >= 1f / FootstepSfxFrequency)
                {
                   footstepDistanceCounter = 0f;
                   AudioSource.PlayOneShot(FootstepsSfx[Random.Range(0, FootstepsSfx.Count)]);
                }
                footstepDistanceCounter += characterVelocity.magnitude * Time.deltaTime;
            }
            else
            {
                characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity * MaxSpeedInAir, AccelerationSpeedInAir * Time.deltaTime);
            }
            characterVelocity = new Vector3(characterVelocity.x, verticalVelocity, characterVelocity.z);
            characterController.Move(characterVelocity * Time.deltaTime);
        }

        protected void Attack(Collider collider)
        {
            if (isDead || lastTimeAttacking + AttackRate > Time.time) return;
            // if (collider.gameObject == target)
            if (collider.gameObject.CompareTag("Player"))
            {
                animator.SetTrigger("Attack");
                lastTimeAttacking = Time.time;
                AudioSource.PlayOneShot(AttacksSfx[Random.Range(0, AttacksSfx.Count)]);

                PlayerCharacterController player = collider.GetComponent<PlayerCharacterController>();
                player.ReceiveDamage(Damage);
                player.ExtraVelocity = new Vector3(fromBodyToPlayer.x * 100, 20, fromBodyToPlayer.z * 100); // Knockback
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
            AudioSource.PlayOneShot(DieSfx[Random.Range(0, DieSfx.Count)]);
            if (BreatheSource != null)
                    BreatheSource.volume = 0;
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