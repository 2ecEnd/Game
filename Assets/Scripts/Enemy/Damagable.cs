using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    public class Damagable : MonoBehaviour
    {
        public void ReceivedDamage(float damage)
        {
            if (gameObject.GetComponent<EnemyControl>() != null)
            {
                gameObject.GetComponent<EnemyControl>().TakeDamage(damage);
                return;
            }
            if (gameObject.GetComponent<PlayerCharacterController>() != null)
            {
                gameObject.GetComponent<PlayerCharacterController>().TakeDamage(damage);
                return;
            }
        }
    }
}