using Assets.Scripts.Gameplay;
using UnityEngine;
using System.Collections;
using Assets.Scripts.Player;
using UnityEngine.VFX;
using System.Collections.Generic;

namespace Assets.Scripts.Enemy
{
    public class Vortigaunt : EnemyBase, IDamagable
    {
        [Header("SFX")]
        public AudioSource AudioSource;
        public List<AudioClip> FootstepsSfx;
        public List<AudioClip> AttacksSfx;
        public List<AudioClip> IdlesSfx;
        public List<AudioClip> DieSfx;
        public AudioClip RangeAttackChargeSfx;
        public AudioClip RangeAttackShootSfx;
        public float FootstepSfxFrequency = 1f;
        public float IdleSfxFrequency = 10f;

        [Header("Range Attack")]
        public float RangeAttackDistance;
        public float RangeAttackCooldown;
        public float RangeAttackDamage;
        public WeaponHandler weaponHandler;
        public VisualEffect PlasmaEffect;

        bool isDead;
        float lastTimeAttacking = Mathf.NegativeInfinity;
        float lastTimeRangeAttacking = Mathf.NegativeInfinity;
        float lastTimePlayingIdle = Mathf.NegativeInfinity;
        float footstepDistanceCounter;
        float distanceToPlayer = 0;
        string currentAttackMode = "melee";
        bool isAttacking = false;
        Animator animator;
        ChaseType chaseType;

        void Start()
        {
            gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            arenaManager = gameController.GetComponent<ArenaManager>();
            characterController = gameObject.GetComponent<CharacterController>();

            target = GameObject.FindGameObjectWithTag("Player").transform;
            PlayerController = target.GetComponent<PlayerCharacterController>();
            animator = GetComponent<Animator>();
            isDead = false;
            PlasmaEffect.Stop();

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
            fromBodyToPlayer = new Vector3(target.position.x, target.position.y+1f, target.position.z) - transform.position;
            if (Physics.Raycast(
                origin: AttackStartPoint.position,
                direction: fromBodyToPlayer,
                hitInfo: out hit,
                maxDistance: 1f))
            {
                if (currentAttackMode == "melee") Attack(hit.collider);
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

            if (lastTimeRangeAttacking + RangeAttackCooldown + Random.Range(0, 2) < Time.time)
                currentAttackMode = "range";

            if (!GlobalInspector.PlayerAlive)
            {
                animator.speed = 0;
                AudioSource.volume = 0;
                //animatorRatio = 0;
                return;
            }
            else if (animator.speed == 0)
            {
                animator.speed = 1;
                AudioSource.volume = 1;
                //animatorRatio = 1;
            }
            if (isDead || lastTimeAttacking + AttackRate > Time.time) return;

            fromBodyToPlayer = target.position - transform.position;
            distanceToPlayer = fromBodyToPlayer.magnitude;
            if (chaseType == ChaseType.Intercept && distanceToPlayer > 5)
            {
                Vector3 predictedPos = PredictPlayerPosition();
                fromBodyToPlayer = predictedPos - transform.position;
            }
            fromBodyToPlayer = (new Vector3(fromBodyToPlayer.x, 0, fromBodyToPlayer.z)).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(isAttacking ? target.position - transform.position : fromBodyToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
            //transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
            weaponHandler.transform.LookAt(new Vector3(target.position.x, target.position.y + 1f, target.position.z));

            float verticalVelocity = characterVelocity.y - GravityForce * Time.deltaTime;

            Vector3 targetVelocity = Vector3.zero;
            if (currentAttackMode == "melee" || (distanceToPlayer > RangeAttackDistance && !isAttacking))
            {
                targetVelocity = fromBodyToPlayer;
                animator.SetFloat("RunningSpeed", 1);
                animator.SetBool("IsRunning", true);
            }
            else
            {
                animator.SetBool("IsRunning", false);
                if (distanceToPlayer < RangeAttackDistance / 1.5 && !isAttacking)
                {
                    targetVelocity = -fromBodyToPlayer;
                    animator.SetFloat("RunningSpeed", -1);
                    animator.SetBool("IsRunning", true);
                }
                else if (!isAttacking)
                {
                    StartCoroutine(RangeAttack());
                }
            }

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
                animator.SetTrigger("MeleeAttack");
                lastTimeAttacking = Time.time;
                AudioSource.PlayOneShot(AttacksSfx[Random.Range(0, AttacksSfx.Count)]);

                PlayerCharacterController player = collider.GetComponent<PlayerCharacterController>();
                player.ReceiveDamage(Damage);
                player.ExtraVelocity = new Vector3(fromBodyToPlayer.x * 100, 20, fromBodyToPlayer.z * 100); // Knockback
            }
        }

        IEnumerator RangeAttack()
        {
            animator.SetTrigger("RangeAttack");
            isAttacking = true;
            lastTimeRangeAttacking = Time.time;
            PlasmaEffect.Play();
            AudioSource.PlayOneShot(RangeAttackChargeSfx);
            yield return new WaitForSeconds(1.2f);

            if (!isDead)
            {
                float prevSpeed = InterceptSpeed;
                InterceptSpeed = 45;

                Vector3 predictedPos = PredictPlayerPosition();
                weaponHandler.transform.LookAt(new Vector3(predictedPos.x, target.position.y + 1f, predictedPos.z));

                weaponHandler.HandleShootInputs(true, true);
                AudioSource.PlayOneShot(RangeAttackShootSfx);

                InterceptSpeed = prevSpeed;
                yield return new WaitForSeconds(0.5f);
            }

            PlasmaEffect.Stop();
            currentAttackMode = "melee";
            isAttacking = false;
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
            
            AudioSource.PlayOneShot(DieSfx[Random.Range(0, DieSfx.Count)]);
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