using System;
using System.Linq;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string _requireKey = "key_001";
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player.Inventory.FirstOrDefault(s => s == _requireKey) != null)
            {
                _animator.SetTrigger("Open");
                player.Inventory.Remove(_requireKey);
                this.GetComponent<BoxCollider>().enabled = false;
            }
        }
    }
}
