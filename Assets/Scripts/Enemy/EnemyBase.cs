using Assets.Scripts.Gameplay;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("General")]
    public float Health;
    public Transform AttackStartPoint;
    public float Damage;
    public float GravityForce;
    public float MaxSpeedOnGround;
    public float MovementSharpnessOnGround;
    public float MaxSpeedInAir;
    public float AccelerationSpeedInAir;
    public Vector3 characterVelocity;

    [Header("Scripts")]
    protected GameController gameController;
    protected ArenaManager arenaManager;
    protected CharacterController characterController;

    [Header("Target Parameters")]
    protected Transform target;
    protected Vector3 directionToPlayer;

    protected abstract void Attack(Collider collider);

    public abstract void TakeDamage(float damage);

    protected abstract void DestroyOnFall();

    protected abstract void Die();
}
