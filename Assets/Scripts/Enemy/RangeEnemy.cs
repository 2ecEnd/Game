using Assets.Scripts.Gameplay;
using UnityEngine;
using System.Collections;
using Assets.Scripts;

public class RangeEnemy : EnemyBase, IDamagable
{
    public float ColorDuration = 0.2f;

    Material material;
    Color originalColor;
    public WeaponHandler weaponHandler;

    private Vector3 fromMuzzleToPlayer;

    void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        arenaManager = gameController.GetComponent<ArenaManager>();
        characterController = gameObject.GetComponent<CharacterController>();

        target = GameObject.FindGameObjectWithTag("Player").transform;

        material = GetComponent<Renderer>().material;
        originalColor = material.color;

        weaponHandler.Owner = gameObject;
    }

    void FixedUpdate()
    {
        Attack();

        DestroyOnFall();
    }

    void Update()
    {
        fromBodyToPlayer = target.position - transform.position;

        float distanceToPlayer = Mathf.Sqrt(Mathf.Pow(fromBodyToPlayer.x, 2) + Mathf.Pow(fromBodyToPlayer.z, 2));

        fromBodyToPlayer = (new Vector3(fromBodyToPlayer.x, 0, fromBodyToPlayer.z)).normalized;
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
        weaponHandler.transform.LookAt(new Vector3(target.position.x, target.position.y, target.position.z));

        if (distanceToPlayer < 10)
        {
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
                characterVelocity += Vector3.down * GravityForce * Time.deltaTime;
            }
        }
        else if (distanceToPlayer > 20)
        {
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
                characterVelocity += Vector3.down * GravityForce * Time.deltaTime;
            }
        }
        else
            characterVelocity = new Vector3();

        characterController.Move(characterVelocity * Time.deltaTime);
    }

    protected void Attack()
    {
        if (weaponHandler.CurrentAmmo > 0)
            weaponHandler.HandleShootInputs(true, true);
        else
            weaponHandler.HandleReload();
    }

    public override void ReceiveDamage(float damage)
    {
        Health -= damage;
        StartCoroutine(ChangeColor());

        if (Health <= 0)
            Die();
    }

    public override void Die(bool needScored = true)
    {
        gameController.Enemies.Remove(gameObject);
        Destroy(gameObject);

        if (needScored)
            GlobalInspector.KilledRange++;
    }

    protected override void DestroyOnFall()
    {
        if (transform.position.y < arenaManager.getKillHeight())
            Die(false);
    }

    IEnumerator ChangeColor()
    {
        material.color = new Color(0f, 1f, 0.5f);
        yield return new WaitForSeconds(ColorDuration);
        material.color = originalColor;
    }
}
