using Assets.Scripts.Gameplay;
using UnityEngine;
using System.Collections;
using Assets.Scripts.Player;
using System.Collections.Generic;

namespace Assets.Scripts.Enemy
{
    public class MeleeEnemy : EnemyBase, IDamagable
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

        [Header("Wandering")]
        public float WanderStartDistance;

        bool isDead;
        bool isWandering;
        float lastTimeAttacking = Mathf.NegativeInfinity;
        float lastTimePlayingIdle = Mathf.NegativeInfinity;
        float footstepDistanceCounter;
        float randomSpeedCoeff;
        ChaseType chaseType;
        GameObject Player;
        Animator animator;
        Vector3 targetDirection;
        Vector3 WanderPosition;

        void Start()
        {
            gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            arenaManager = gameController.GetComponent<ArenaManager>();
            characterController = gameObject.GetComponent<CharacterController>();

            Player = GameObject.FindGameObjectWithTag("Player");
            PlayerController = Player.GetComponent<PlayerCharacterController>();
            target = Player.transform;
            animator = GetComponent<Animator>();
            isDead = false;
            randomSpeedCoeff = Random.Range(0.9f, 1.1f);
            animator.SetFloat("SpeedCoef", randomSpeedCoeff);

            isWandering = false;

            chaseType = Random.Range(0, 2) == 0 ? ChaseType.Direct : ChaseType.Intercept;
            if (chaseType == ChaseType.Intercept)
                InterceptSpeed = Random.Range(25, 35);
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
                maxDistance: 1f))
            {
                Attack(hit.collider);
            }

            DestroyOnFall();
        }

        void Update()
        {
            fromBodyToPlayer = target.position - transform.position;

            if (fromBodyToPlayer.magnitude > WanderStartDistance)
            {
                Wander();
            }
            else
            {
                if (chaseType == ChaseType.Intercept && fromBodyToPlayer.magnitude > 4)
                {
                    Vector3 predictedPos = PredictPlayerPosition();
                    fromBodyToPlayer = predictedPos - transform.position;
                }
                targetDirection = (new Vector3(fromBodyToPlayer.x, 0, fromBodyToPlayer.z)).normalized;
                isWandering = false;
            }

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
            
            if (isDead && !(Physics.Raycast(
                origin: new Vector3(transform.position.x, transform.position.y - characterController.height/1.98f, transform.position.z),
                direction: -transform.up,
                maxDistance: 0.05f)))
            {
                transform.Translate(Vector3.down * GravityForce/2 * Time.deltaTime);
            }

            if (isDead || lastTimeAttacking + AttackRate > Time.time) return;

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);

            //transform.LookAt(new Vector3(targetPosition.x, transform.position.y, targetPosition.z));
            Vector3 targetVelocity = targetDirection;
            float verticalVelocity = characterVelocity.y - GravityForce * Time.deltaTime;
            if (characterController.isGrounded)
            {
                characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity * MaxSpeedOnGround * randomSpeedCoeff, MovementSharpnessOnGround * Time.deltaTime);
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
                player.ExtraVelocity = new Vector3(targetDirection.x * 100, 20, targetDirection.z * 100); // Knockback
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

        protected override void DestroyOnFall()
        {
            if (transform.position.y < arenaManager.GetKillHeight())
                Die(false);
        }

        public void Wander()
        {
            if (!isWandering)
            {
                Vector3 randomPos = arenaManager.GetRandomPoint();
                targetDirection = (new Vector3(randomPos.x - transform.position.x, 0, randomPos.z - transform.position.z)).normalized;
                WanderPosition = randomPos;
                isWandering = true;
            }
            else
            {
                if ((WanderPosition - transform.position).magnitude < 4f)
                {
                    isWandering = false;
                }
            }
        }
    }
}