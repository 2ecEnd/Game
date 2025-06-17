using Assets.Scripts.Gameplay;
using Assets.Scripts.Player;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamagable
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
    protected Vector3 fromBodyToPlayer;

    public abstract void ReceiveDamage(float damage);

    public abstract void Die();

    protected abstract void DestroyOnFall();

}