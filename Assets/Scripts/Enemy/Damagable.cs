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
                parent.GetComponent<EnemyControl>().TakeDamage(damage);
                return;
            }
            if (parent.GetComponent<PlayerCharacterController>() != null)
            {
                parent.GetComponent<PlayerCharacterController>().TakeDamage(damage);
                return;
            }
        }
    }
}