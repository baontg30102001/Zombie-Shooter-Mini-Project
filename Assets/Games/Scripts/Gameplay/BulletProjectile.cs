using System;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField]
    private Rigidbody bulletRigidBody;

    private void Start()
    {
        float _speed = 10f;
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
