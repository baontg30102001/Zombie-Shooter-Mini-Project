using System;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField]
    private Rigidbody bulletRigidBody;
    [SerializeField]
    float _speed = 10f;
    [SerializeField] 
    private int _damage = 20;
    
    private void Start()
    {
        bulletRigidBody.linearVelocity = transform.forward * _speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
        Target target = other.gameObject.GetComponent<Target>();
        if (target != null)
        {
            target.Hit();
        }
        else
        {
            Debug.Log("missing");
        }
    }
}
