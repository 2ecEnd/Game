using Assets.Scripts.Player;
using UnityEngine;
public class HealthBox : BuffBox
{
    void Start()
    {
        meshGO = transform.GetChild(0).gameObject;
        meshPosition = Vector3.zero;
    }

    void FixedUpdate()
    {
        Flex();
    }

    protected override void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            collider.gameObject.GetComponent<PlayerCharacterController>().ReceiveDamage(-25);
            Destroy(gameObject);
        }
    }

    protected override void Flex()
    {
        if (meshPosition.y < FlexRange)
        {
            meshSpeed += FlexAcceleration * Time.fixedDeltaTime;
        }
        else if (meshPosition.y > FlexRange)
        {
            meshSpeed -= FlexAcceleration * Time.fixedDeltaTime;
        }
        meshPosition.y += meshSpeed;
        meshGO.transform.localPosition = meshPosition;
        meshGO.transform.Rotate(0, RotationSpeed, 0);
    }
}