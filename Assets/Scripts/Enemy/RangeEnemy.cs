using Assets.Scripts.Gameplay;
using UnityEngine;
using System.Collections;
using Assets.Scripts.Player;
using static UnityEngine.GraphicsBuffer;
using Assets.Scripts;

public class RangeEnemy : EnemyBase
{
    public float ColorDuration = 0.2f;

    Material material;
    Color originalColor;
    public WeaponHandler weaponHandler;

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
        directionToPlayer = target.position - transform.position;
        float distanceToPlayer = Mathf.Sqrt(Mathf.Pow(directionToPlayer.x, 2) + Mathf.Pow(directionToPlayer.z, 2));
        directionToPlayer = (new Vector3(directionToPlayer.x, 0, directionToPlayer.z)).normalized;
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));

        if (distanceToPlayer < 10)
        {
            if (characterController.isGrounded)
            {
                Vector3 targetVelocity = directionToPlayer * -MaxSpeedOnGround;
                characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, MovementSharpnessOnGround * Time.deltaTime);
            }
            else
            {
                characterVelocity -= directionToPlayer * AccelerationSpeedInAir * Time.deltaTime;
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
                Vector3 targetVelocity = directionToPlayer * MaxSpeedOnGround;
                characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, MovementSharpnessOnGround * Time.deltaTime);
            }
            else
            {
                characterVelocity += directionToPlayer * AccelerationSpeedInAir * Time.deltaTime;
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


        weaponHandler.HandleShootInputs(true, true);

    }

    protected override void Attack(Collider collider)
    {
        // if (collider.gameObject == target)
        if (collider.gameObject.CompareTag("Player"))
        {
            PlayerCharacterController player = collider.GetComponent<PlayerCharacterController>();
            player.TakeDamage(Damage);
            player.CharacterVelocity = new Vector3(directionToPlayer.x * 1000, 5, directionToPlayer.z * 1000); // Knockback
        }
    }

    public override void TakeDamage(float damage)
    {
        Health -= damage;
        StartCoroutine(ChangeColor());

        if (Health <= 0)
            Die();
    }

    IEnumerator ChangeColor()
    {
        material.color = new Color(0f, 1f, 0.5f);
        yield return new WaitForSeconds(ColorDuration);
        material.color = originalColor;
    }

    protected override void DestroyOnFall()
    {
        if (transform.position.y < arenaManager.getKillHeight())
            Die();
    }

    protected override void Die()
    {
        gameController.Enemies.Remove(gameObject);
        Destroy(gameObject);
    }
}
