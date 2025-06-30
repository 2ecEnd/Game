using Assets.Scripts.Gameplay;
using Assets.Scripts.Player;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour, IDamagable
{
    [Header("General")]
    public float Health;
    public Transform AttackStartPoint;
    public float Damage;
    public float AttackRate = 0.5f;
    public int KillsStatistic = 0;

    [Header("Movement")]
    public float GravityForce;
    public float MaxSpeedOnGround;
    public float MovementSharpnessOnGround;
    public float MaxSpeedInAir;
    public float AccelerationSpeedInAir;
    public float RotationSpeed;
    public Vector3 characterVelocity;
    public int PredictionSteps;
    public float InterceptSpeed;

    [Header("Disappearance")]
    public float DisappearanceRate = 10;
    public float SinkSpeed = 5;

    [Header("Scripts")]
    protected GameController gameController;
    protected ArenaManager arenaManager;
    protected CharacterController characterController;

    [Header("Target Parameters")]
    protected Transform target;
    protected PlayerCharacterController PlayerController;
    protected Vector3 fromBodyToPlayer;

    public abstract void ReceiveDamage(float damage);

    public abstract void Die(bool needScored = true);

    protected abstract void DestroyOnFall();

    protected Vector3 PredictPlayerPosition()
    {
        Vector3 predictedPosition = target.position;

        if (PlayerController.CharacterVelocity.magnitude < 0.1f)
            return predictedPosition;
        
        float predictedTime = 1f; 
        for (int i = 0; i < PredictionSteps; i++)
        {
            predictedPosition = target.position + PlayerController.CharacterVelocity * predictedTime;

            float distanceToPredictedPos = Vector3.Distance(transform.position, predictedPosition);

            predictedTime = distanceToPredictedPos / InterceptSpeed;
        }

        return predictedPosition;
    }
}