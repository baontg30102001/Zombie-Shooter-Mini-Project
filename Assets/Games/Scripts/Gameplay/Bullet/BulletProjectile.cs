using System;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] protected Rigidbody _bulletRigidBody;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _damage = 20;
    [SerializeField] protected float _lifeTime;
    [SerializeField] private GameObject _hitVFX;
    [SerializeField] private float _radius = 0f;
    [SerializeField] private GameObject _explosionEffect;

    private BulletInstaller.Settings _bulletConfig;
    protected BulletData _bulletData = new BulletData();
    
    [Inject]
    public void Construct(BulletInstaller.Settings bulletConfig)
    {
        _bulletConfig = bulletConfig;
    }

    public void Initialize(string bulletId)
    {
        _bulletData = _bulletConfig.GetBulletDataById(bulletId);

        _speed = _bulletData.bulletSpeed;
        _damage = _bulletData.bulletDamage;
        _lifeTime = _bulletData.bulletLifetime;
        _radius = _bulletData.bulletRadius;
    }

    public void Shooting()
    {
        _bulletRigidBody.linearVelocity = transform.forward * _speed;
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log($"hit {other.name}");
        if (_radius > 0)
        {
            Explode();
        }
        else
        {
            HitEnemies(other);
        }
    }

    private void HitEnemies(Collider other)
    {
        Target target = other.GetComponent<Target>();
        if (target != null)
        {
            target.TakeDamage(_damage);
        }
        if (_hitVFX != null)
        {
            Instantiate(_hitVFX, transform.position, transform.rotation);
        }
    }

    private void Explode()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _radius);
        foreach (var collider in colliders)
        {
            HitEnemies(collider);
        }

        if (_explosionEffect != null)
        {
            Instantiate(_explosionEffect, transform.position, transform.rotation);
        }
    }
}
