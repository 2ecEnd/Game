using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class ProjectileStandard : ProjectileBase
    {
        public float Radius = 0.01f;
        public float Speed = 20f;
        public float Damage = 40f;
        public float MaxLifeTime = 5f;
        public LayerMask HittableLayers;
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

        void Update()
        {
            transform.position += m_Velocity * Time.deltaTime;

            transform.forward = m_Velocity.normalized;

            if (GravityDownAcceleration > 0)
            {
                m_Velocity += Vector3.down * GravityDownAcceleration * Time.deltaTime;
            }
        }

        new void OnShoot()
        {
            m_Velocity = transform.forward * Speed;
            m_IgnoredColliders = new List<Collider>();
            transform.position += m_ProjectileBase.InheritedMuzzleVelocity * Time.deltaTime;

            m_IgnoredColliders.AddRange(Owner.GetComponentsInChildren<Collider>());
            Debug.Log(transform.position);
        }
    }
}