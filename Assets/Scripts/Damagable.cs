using Assets.Scripts.Player;
using UnityEngine;

public interface IDamagable
{
    public void ReceiveDamage(float damage);

    public void Die(bool needScored = true);
}
