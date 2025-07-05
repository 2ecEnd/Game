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
        DestroyOnFall();
    }

    protected override void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            collider.gameObject.GetComponent<PlayerCharacterController>().ReceiveDamage(-25);
            collider.gameObject.GetComponent<PlayerCharacterController>().PlayHealSound();
            Destroy(gameObject);
        }
    }

    void DestroyOnFall()
    {
        if (transform.position.y < -100)
            Destroy(gameObject);
    }
}