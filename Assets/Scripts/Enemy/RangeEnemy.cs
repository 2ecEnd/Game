using Assets.Scripts.Gameplay;
using UnityEngine;
using System.Collections;
using Assets.Scripts;
using Assets.Scripts.Player;
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

    public float NumberOfShotsPerBurst = 5;
    public WeaponHandler weaponHandler;
    public float AttackAnimationDelay = 1f;

    bool isDead;
    bool PlayerIsVisible;
    float lastTimeAttacking = Mathf.NegativeInfinity;
    float currentBurstCount = 0;
    float distanceToPlayer = 0;
    private float arenaSize;
    //private float distanceToEdgeX;
    //private float distanceToEdgeZ;
    Animator animator;
    private float animatorRatio;
    float lastTimePlayingIdle = Mathf.NegativeInfinity;
    float footstepDistanceCounter;
    Vector3 moveOffset;

    void Start()
    {
        PlayerIsVisible = true;
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        arenaManager = gameController.GetComponent<ArenaManager>();
        characterController = gameObject.GetComponent<CharacterController>();

        target = GameObject.FindGameObjectWithTag("Player").transform;
        PlayerController = target.GetComponent<PlayerCharacterController>();
        animator = GetComponent<Animator>();
        isDead = false;
        weaponHandler.Owner = gameObject;
        arenaSize = (arenaManager.GetArenaSize() - 0.5f) * arenaManager.GetChunkScale();

        StartCoroutine(CheckPlayerVisibility());

        moveOffset = UnityEngine.Random.insideUnitSphere * 25;

        InterceptSpeed = UnityEngine.Random.Range(70, 140);
    }

    void FixedUpdate()
    {
        if (!GlobalInspector.PlayerAlive)
            return;

        Attack();
        DestroyOnFall();

        if (isDead && !(Physics.Raycast(
            origin: new Vector3(transform.position.x, transform.position.y - characterController.height / 1.9f, transform.position.z),
            direction: -transform.up,
            maxDistance: 0.1f)))
        {
            transform.Translate(Vector3.down * GravityForce/2 * Time.deltaTime);
        }
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
        Vector3 moveDirection = (target.position + moveOffset) - transform.position;
        moveDirection = new Vector3(moveDirection.x, 0, moveDirection.z);   
        fromBodyToPlayer = target.position - transform.position;
        fromBodyToPlayer = new Vector3(fromBodyToPlayer.x, 0, fromBodyToPlayer.z);
        distanceToPlayer = fromBodyToPlayer.magnitude;
        //fromBodyToPlayer = fromBodyToPlayer.normalized;

        //transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
        weaponHandler.transform.LookAt(new Vector3(target.position.x, target.position.y + 1, target.position.z));
        if (distanceToPlayer < 10)
        {
            //animator.SetBool("IsRunning", true);
            //animator.SetFloat("RunningSpeed", -1);
            targetVelocity = -fromBodyToPlayer;
            animatorRatio = -1;
        }
        else if (distanceToPlayer > 60 || !PlayerIsVisible)
        {
            //animator.SetBool("IsRunning", true);
            //animator.SetFloat("RunningSpeed", 1);
            targetVelocity = moveDirection;
            animatorRatio = 1;
        }
        else
        {
            //animator.SetBool("IsRunning", false);
        }

        Quaternion targetRotation = Quaternion.LookRotation(distanceToPlayer > 60 || !PlayerIsVisible ? moveDirection : fromBodyToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);


        if (lastTimeAttacking + AttackAnimationDelay > Time.time) return;

        if (targetVelocity.magnitude > 1)
        {
            targetVelocity = targetVelocity.normalized;
        }
        if (transform.position.x < 2)
        {
            if (targetVelocity.x < 0)
                targetVelocity = new Vector3(0, 0, targetVelocity.z);
        }
        if (transform.position.x > arenaSize)
        {
            if (targetVelocity.x > 0)
                targetVelocity = new Vector3(0, 0, targetVelocity.z);
        }
        if (transform.position.z < 2)
        {
            if (targetVelocity.z < 0)
                targetVelocity = new Vector3(targetVelocity.x, 0, 0);
        }
        if (transform.position.z > arenaSize)
        {
            if (targetVelocity.z > 0)
                targetVelocity = new Vector3(targetVelocity.x, 0, 0);
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
        if (isDead || 
            lastTimeAttacking + AttackRate > Time.time || 
            distanceToPlayer < 10 || 
            distanceToPlayer > 60 || 
            !characterController.isGrounded) 
            return;

        StartCoroutine(Shooting());
    }

    IEnumerator Shooting()
    {
        animator.SetTrigger("Attack");
        lastTimeAttacking = Time.time;
        yield return new WaitForSeconds(0.3f);

        while (currentBurstCount < NumberOfShotsPerBurst)
        {
            Vector3 predictedPos = PredictPlayerPosition();
            weaponHandler.transform.LookAt(new Vector3(predictedPos.x, target.position.y + 1f, predictedPos.z));

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

    IEnumerator CheckPlayerVisibility()
    {
        while (true)
        {
            RaycastHit hit;
            Vector3 pos = transform.position;
            pos.y += 1;
            fromBodyToPlayer = new Vector3(target.position.x, target.position.y + 1, target.position.z) - pos;

            if (Physics.Raycast(
                origin: pos,
                direction: fromBodyToPlayer,
                hitInfo: out hit,
                maxDistance: 100f))
            {
                if (!hit.collider.gameObject.CompareTag("Player"))
                    PlayerIsVisible = false;
                else
                    PlayerIsVisible = true;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    protected override void DestroyOnFall()
    {
        if (transform.position.y < arenaManager.GetKillHeight())
            Die(false);
    }
}
