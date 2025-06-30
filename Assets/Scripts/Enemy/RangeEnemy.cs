using Assets.Scripts.Gameplay;
using UnityEngine;
using System.Collections;
using Assets.Scripts;
using Unity.Mathematics;
using System.Collections.Generic;

public class RangeEnemy : EnemyBase, IDamagable
{
    [Header("SFX")]
    public AudioSource AudioSource;
    public AudioSource BreatheSource;
    public List<AudioClip> FootstepsSfx;
    public List<AudioClip> IdlesSfx;
    public List<AudioClip> DieSfx;
    public float FootstepSfxFrequency = 1f;
    public float IdleSfxFrequency = 10f;
    public Transform Leader;
    public Vector3 PositionToLeader;

    public float NumberOfShotsPerBurst = 5;
    public WeaponHandler weaponHandler;
    public float AttackAnimationDelay = 1f;

    bool isDead;
    float lastTimeAttacking = Mathf.NegativeInfinity;
    float currentBurstCount = 0;
    float distanceToPlayer = 0;
    private Vector3 fromMuzzleToPlayer;
    private float arenaSize;
    //private float distanceToEdgeX;
    //private float distanceToEdgeZ;
    Animator animator;
    private float animatorRatio;
    float lastTimePlayingIdle = Mathf.NegativeInfinity;
    float footstepDistanceCounter;

    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        arenaManager = gameController.GetComponent<ArenaManager>();
        characterController = gameObject.GetComponent<CharacterController>();

        target = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        isDead = false;
        weaponHandler.Owner = gameObject;
        arenaSize = (arenaManager.GetArenaSize() - 0.5f) * arenaManager.GetChunkScale();
    }

    void FixedUpdate()
    {
        if (!GlobalInspector.PlayerAlive)
        {
            return;
        }
        Attack();

        DestroyOnFall();
    }

    void Update()
    {
        if (lastTimePlayingIdle + IdleSfxFrequency + UnityEngine.Random.Range(0, 5) < Time.time)
        {
            AudioSource.PlayOneShot(IdlesSfx[UnityEngine.Random.Range(0, IdlesSfx.Count)]);
            lastTimePlayingIdle = Time.time;
        }

        if (!GlobalInspector.PlayerAlive)
        {
            animator.speed = 0;
            AudioSource.volume = 0;
            BreatheSource.volume = 0;
            //animatorRatio = 0;
            return;
        }
        else if (animator.speed == 0)
        {
            animator.speed = 1;
            AudioSource.volume = 1;
            BreatheSource.volume = 1;
            //animatorRatio = 1;
        }
        //distanceToEdgeX = math.min(transform.position.x, arenaSize - transform.position.x);
        //print(distanceToEdgeX);
        if (isDead) return;
        Vector3 targetVelocity = Vector3.zero;
        if (Leader == null)
        {
            fromBodyToPlayer = target.position - transform.position;
            fromBodyToPlayer = new Vector3(fromBodyToPlayer.x, 0, fromBodyToPlayer.z);
            distanceToPlayer = fromBodyToPlayer.magnitude;
            //fromBodyToPlayer = fromBodyToPlayer.normalized;

            Quaternion targetRotation = Quaternion.LookRotation(fromBodyToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);

            //transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
            weaponHandler.transform.LookAt(new Vector3(target.position.x, target.position.y + 1, target.position.z));
            if (distanceToPlayer < 10)
            {
                //animator.SetBool("IsRunning", true);
                //animator.SetFloat("RunningSpeed", -1);
                targetVelocity = -fromBodyToPlayer;
                animatorRatio = -1;
            }
            else if (distanceToPlayer > 20)
            {
                //animator.SetBool("IsRunning", true);
                //animator.SetFloat("RunningSpeed", 1);
                targetVelocity = fromBodyToPlayer;
                animatorRatio = 1;
            }
            else
            {
                //animator.SetBool("IsRunning", false);
            }
        }
        else
        {
            distanceToPlayer = 15;
            targetVelocity = Leader.position + PositionToLeader - transform.position;
            //targetVelocity = Leader.position + Leader.right * 3 - transform.position;
            transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
            weaponHandler.transform.LookAt(new Vector3(target.position.x, target.position.y + 1, target.position.z));
            animatorRatio = 1;
            //transform.rotation = Leader.rotation;
        }

        if (lastTimeAttacking + AttackAnimationDelay > Time.time) return;

        if (targetVelocity.magnitude > 1)
        {
            targetVelocity = targetVelocity.normalized;
        }
        if (transform.position.x < 2)
        {
            if (targetVelocity.x < 0)
            {
                targetVelocity = new Vector3(0, 0, targetVelocity.z);
            }
        }
        if (transform.position.x > arenaSize)
        {
            if (targetVelocity.x > 0)
            {
                targetVelocity = new Vector3(0, 0, targetVelocity.z);
            }
        }
        if (transform.position.z < 2)
        {
            if (targetVelocity.z < 0)
            {
                targetVelocity = new Vector3(targetVelocity.x, 0, 0);
            }
        }
        if (transform.position.z > arenaSize)
        {
            if (targetVelocity.z > 0)
            {
                targetVelocity = new Vector3(targetVelocity.x, 0, 0);
            }
        }
        if (targetVelocity.magnitude > 0.2f)
        {
            animator.SetBool("IsRunning", true);
            animator.SetFloat("RunningSpeed", targetVelocity.magnitude * animatorRatio);
        }
        else
        {
            animator.SetBool("IsRunning", false);
        }
        float verticalVelocity = characterVelocity.y - GravityForce * Time.deltaTime;
        if (characterController.isGrounded)
        {
            characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity * MaxSpeedOnGround, MovementSharpnessOnGround * Time.deltaTime);
            verticalVelocity = -GravityForce * 0.1f;

            if (footstepDistanceCounter >= 1f / FootstepSfxFrequency && targetVelocity.magnitude > 0.1f)
            {
                footstepDistanceCounter = 0f;
                AudioSource.PlayOneShot(FootstepsSfx[UnityEngine.Random.Range(0, FootstepsSfx.Count)]);
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

    protected void Attack()
    {
        if (isDead || lastTimeAttacking + AttackRate > Time.time || distanceToPlayer < 10 || distanceToPlayer > 20 || !characterController.isGrounded) return;

        StartCoroutine(Shooting());
    }

    IEnumerator Shooting()
    {
        animator.SetTrigger("Attack");
        lastTimeAttacking = Time.time;
        yield return new WaitForSeconds(0.3f);

        while (currentBurstCount < NumberOfShotsPerBurst)
        {
            bool hasFired = weaponHandler.HandleShootInputs(true, true);
            yield return new WaitForSeconds(0.01f);
            if (hasFired) currentBurstCount++;
        }

        currentBurstCount = 0;
    }

    public override void ReceiveDamage(float damage)
    {
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
        AudioSource.PlayOneShot(DieSfx[UnityEngine.Random.Range(0, DieSfx.Count)]);
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

        Destroy(gameObject);
    }

    protected override void DestroyOnFall()
    {
        if (transform.position.y < arenaManager.GetKillHeight())
            Die(false);
    }
}
