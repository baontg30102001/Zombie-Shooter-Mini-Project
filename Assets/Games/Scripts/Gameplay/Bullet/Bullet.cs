using System;
using System.Collections;
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
    [SerializeField] private Transform _shooter;
    protected BulletData _bulletData = new BulletData();

    public virtual void Initialize(BulletData bulletData, Transform shooter)
    {
        _bulletData = bulletData;

        _speed = _bulletData.bulletSpeed;
        _damage = _bulletData.bulletDamage;
        _lifeTime = _bulletData.bulletLifetime;
        _radius = _bulletData.bulletRadius;
        _gravity = _bulletData.bulletGravity;
        _bulletType = _bulletData.bulletType;
        _hitVFX = _bulletData.hitVFX;
        _explosionEffect = _bulletData.explosionVFX;
        _shooter = shooter;
    }

    public void Shooting()
    {
        _bulletRigidBody.linearVelocity = transform.forward * _speed;
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        if (_radius > 0)
        {
            Explode(gameObject.transform);
        }
        else
        {
            HitEnemies(other);

            if (_hitVFX != null && _shooter != null)
            {
                // var effectIstance = Instantiate(_hitVFX, transform.position, transform.rotation);
                // effectIstance.transform.LookAt(_shooter);
            }
        }
    }

    private void HitEnemies(Collider other)
    {
        if (other.TryGetComponent<Zombie>(out var zombie))
        {
            zombie.TakeDamage(_damage);
        }
        else if (other.TryGetComponent<Player>(out var player))
        {
            player.TakeDamage(_damage);
        }
    }

    private void Explode(Transform otherTransform)
    {
        Collider[] colliders = Physics.OverlapSphere(otherTransform.position, _radius);
        foreach (var collider in colliders)
        {
            HitEnemies(collider);
        }

        if (_explosionEffect != null && _shooter != null)
        {
            StartCoroutine(ExplosionVFX(otherTransform));
        }
    }

    IEnumerator ExplosionVFX(Transform transform)
    {
        var vfx = Instantiate(_explosionEffect, transform.position, transform.rotation);
        yield return new WaitForSeconds(0.5f);
        Destroy(vfx);
    }

}
