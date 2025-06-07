using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    public class Damagable : MonoBehaviour
    {
        public void ReceivedDamage(float damage)
        {
            GameObject parent = transform.parent.gameObject;
            if (parent.GetComponent<EnemyControl>() != null)
            {
                parent.GetComponent<EnemyControl>().ReceivedDamage(damage);
                return;
            }
            if (parent.GetComponent<PlayerCharacterController>() != null)
            {
                parent.GetComponent<PlayerCharacterController>().ReceivedDamage(damage);
                return;
            }
        }
    }
}