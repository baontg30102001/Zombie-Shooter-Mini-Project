using System;
using System.Collections;
using UnityEngine;
using Zenject;

public class Bullet_556 : Bullet, IPoolable<Vector3, IMemoryPool>, IDisposable
{
    private IMemoryPool _pool;

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        // Dispose();
    }

    public void OnDespawned()
    {
        _pool = null;
    }

    public void OnSpawned(Vector3 p1, IMemoryPool p2)
    {
        _pool = p2;
        transform.position = p1;

        StartCoroutine(LifeTimeCountdown());
    }

    private IEnumerator LifeTimeCountdown()
    {
        yield return new WaitForSeconds(_lifeTime);
        Dispose();
    }

    public void Dispose()
    {
        _pool.Despawn(this);
    }
    
    public class Factory : PlaceholderFactory<Vector3, Bullet_556>
    {
       
    }
}
