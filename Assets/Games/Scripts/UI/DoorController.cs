using System;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private void OnCollisionEnter(Collision other)
    {
        Debug.Log(other.gameObject.name);
        if (other.gameObject.CompareTag("Player"))
        {
            _animator.SetTrigger("Open");
        }
    }
}
