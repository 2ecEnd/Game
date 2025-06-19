using Assets.Scripts.Gameplay;
using UnityEngine;
using System.Collections;
using Assets.Scripts;

public class RangeEnemy : EnemyBase, IDamagable
{
    public float NumberOfShotsPerBurst = 5;
    public WeaponHandler weaponHandler;
    public float AttackAnimationDelay = 1f;

    bool isDead;
    float lastTimeAttacking = Mathf.NegativeInfinity;
    float currentBurstCount = 0;
    float distanceToPlayer = 0;
    private Vector3 fromMuzzleToPlayer;
    Animator animator;

    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        arenaManager = gameController.GetComponent<ArenaManager>();
        characterController = gameObject.GetComponent<CharacterController>();

        target = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        isDead = false;
        weaponHandler.Owner = gameObject;
    }

    void FixedUpdate()
    {
        Attack();

        DestroyOnFall();
    }

    void Update()
    {
        if (isDead) return;

        fromBodyToPlayer = target.position - transform.position;

        distanceToPlayer = Mathf.Sqrt(Mathf.Pow(fromBodyToPlayer.x, 2) + Mathf.Pow(fromBodyToPlayer.z, 2));

        fromBodyToPlayer = (new Vector3(fromBodyToPlayer.x, 0, fromBodyToPlayer.z)).normalized;
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
        weaponHandler.transform.LookAt(new Vector3(target.position.x, target.position.y, target.position.z));

        if (distanceToPlayer < 10)
        {
            animator.SetBool("IsRunning", true);
            animator.SetFloat("RunningSpeed", -1);

            if (characterController.isGrounded)
            {
                Vector3 targetVelocity = fromBodyToPlayer * -MaxSpeedOnGround;
                characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, MovementSharpnessOnGround * Time.deltaTime);
            }
            else
            {
                characterVelocity -= fromBodyToPlayer * AccelerationSpeedInAir * Time.deltaTime;
                float verticalVelocity = characterVelocity.y;
                Vector3 horizontalVelocity = Vector3.ProjectOnPlane(characterVelocity, Vector3.up);
                horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, MaxSpeedInAir);
                characterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);
            }
        }
        else if (distanceToPlayer > 20)
        {
            animator.SetBool("IsRunning", true);
            animator.SetFloat("RunningSpeed", 1);

            if (characterController.isGrounded)
            {
                Vector3 targetVelocity = fromBodyToPlayer * MaxSpeedOnGround;
                characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, MovementSharpnessOnGround * Time.deltaTime);
            }
            else
            {
                characterVelocity += fromBodyToPlayer * AccelerationSpeedInAir * Time.deltaTime;
                float verticalVelocity = characterVelocity.y;
                Vector3 horizontalVelocity = Vector3.ProjectOnPlane(characterVelocity, Vector3.up);
                horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, MaxSpeedInAir);
                characterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);
            }
        }
        else
        {
            animator.SetBool("IsRunning", false);

            characterVelocity = new Vector3();
        }

        if (!characterController.isGrounded) characterVelocity += Vector3.down * GravityForce * Time.deltaTime;
        
        if (lastTimeAttacking + AttackAnimationDelay > Time.time) return;
        
        characterController.Move(characterVelocity * Time.deltaTime);
    }

    protected void Attack()
    {
        if (isDead || lastTimeAttacking + AttackRate > Time.time
        || distanceToPlayer < 10 || distanceToPlayer > 20) return;

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

    public override void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        characterController.enabled = false;

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
        if (transform.position.y < arenaManager.getKillHeight())
            Die();
    }
}
