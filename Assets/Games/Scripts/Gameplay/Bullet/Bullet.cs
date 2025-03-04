using System;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class Bullet : MonoBehaviour
{
    [SerializeField] protected Rigidbody _bulletRigidBody;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _damage = 20;
    [SerializeField] protected float _lifeTime;
    [SerializeField] protected float _radius = 0f;
    [SerializeField] private float _gravity = 0f;
    [SerializeField] private BulletType _bulletType;
    [SerializeField] private GameObject _hitVFX;
    [SerializeField] private GameObject _explosionEffect;

    protected BulletData _bulletData = new BulletData();
    
   

    // public void Initialize(string bulletId)
    // {
    //     _speed = _bulletData.bulletSpeed;
    //     _damage = _bulletData.bulletDamage;
    //     _lifeTime = _bulletData.bulletLifetime;
    //     _radius = _bulletData.bulletRadius;
    //     _gravity = _bulletData.bulletGravity;
    //     _bulletType = _bulletData.bulletType;
    // }
    
    public virtual void Initialize(BulletData bulletData)
    {
        _bulletData = bulletData;

        _speed = _bulletData.bulletSpeed;
        _damage = _bulletData.bulletDamage;
        _lifeTime = _bulletData.bulletLifetime;
        _radius = _bulletData.bulletRadius;
        _gravity = _bulletData.bulletGravity;
        _bulletType = _bulletData.bulletType;
    }

    public void Shooting()
    {
        _bulletRigidBody.linearVelocity = transform.forward * _speed;
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        // Debug.Log($"hit {other.name}");
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
        if (other.TryGetComponent<Zombie>(out var zombie))
        {
            zombie.TakeDamage(_damage);
        }
        else if(other.TryGetComponent<Player>(out var player))
        {
            player.TakeDamage(_damage);
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
