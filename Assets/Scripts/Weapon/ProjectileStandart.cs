using System.Collections.Generic;
using Assets.Scripts.Enemy;
using Assets.Scripts.Player;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Scripts
{
    public class ProjectileStandard : ProjectileBase
    {
        [Header("General")]
        public Transform Tip;
        public Transform Tail;
        public float Radius = 0.01f;
        public float Speed = 20f;
        public float Damage = 40f;
        public float MaxLifeTime = 5f;
        public LayerMask HittableLayers;

        [Header("Audio")] 
        ProjectileBase m_ProjectileBase;

        private Vector3 m_Velocity;
        public float GravityDownAcceleration = 0f;

        List<Collider> m_IgnoredColliders;

        void OnEnable()
        {
            m_ProjectileBase = GetComponent<ProjectileBase>();
            m_ProjectileBase.OnShoot += OnShoot;

            Destroy(gameObject, MaxLifeTime);
        }

        void FixedUpdate()
        {
            RaycastHit hit;
            if (Physics.Raycast(
                origin: Tail.position,
                direction: transform.forward,
                hitInfo: out hit,
                maxDistance: 0.1f + Speed * Time.fixedDeltaTime,
                layerMask: HittableLayers))
            {
                if (IsHitValid(hit))
                {
                    OnHit(hit.point, hit.normal, hit.collider);
                }
            }

            transform.position += m_Velocity * Time.fixedDeltaTime;

            transform.forward = m_Velocity.normalized;

            if (GravityDownAcceleration > 0)
            {
                m_Velocity += Vector3.down * GravityDownAcceleration * Time.fixedDeltaTime;
            }
        }

        new void OnShoot()
        {
            m_Velocity = transform.forward * Speed;
            m_IgnoredColliders = new List<Collider>();
            //transform.position += m_ProjectileBase.InheritedMuzzleVelocity * Time.fixedDeltaTime;

            m_IgnoredColliders.AddRange(Owner.GetComponentsInChildren<Collider>());

        }

        bool IsHitValid(RaycastHit hit)
        {
            if (m_IgnoredColliders != null && m_IgnoredColliders.Contains(hit.collider))
            {
                return false;
            }
            return true;
        }
        
        void OnHit(Vector3 point, Vector3 normal, Collider collider)
        {
            if (collider.TryGetComponent(out IDamagable entity))
                entity.ReceiveDamage(Damage);

            Destroy(this.gameObject);
        }
    }
}