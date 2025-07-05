using UnityEngine;
public abstract class BuffBox : MonoBehaviour
{
    [Header("General")]
    public int BuffRate;
    [Header("Flex")]
    public float FlexAcceleration = 0.1f;
    public float FlexRange = 0.5f;
    public float RotationSpeed = 0.1f;

    protected GameObject meshGO;
    protected Vector3 meshPosition;
    protected float meshSpeed;
    protected abstract void OnTriggerEnter(Collider collider);
}