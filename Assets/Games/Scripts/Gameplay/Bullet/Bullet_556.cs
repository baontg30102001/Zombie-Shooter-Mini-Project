using System;
using UnityEngine;
using Zenject;

public class Bullet_556 : BulletProjectile, IPoolable<Vector3, IMemoryPool>, IDisposable
{
    private Vector3 _firePosition;
    private IMemoryPool _pool;

    public void OnDespawned()
    {
        _pool = null;
    }

    public void OnSpawned(Vector3 p1, IMemoryPool p2)
    {
        transform.position = p1;
    }
    
    public class Factory : PlaceholderFactory<Vector3, Bullet_556>
    {
       
    }

    public void Dispose()
    {
        _pool.Despawn(this);
    }
}
