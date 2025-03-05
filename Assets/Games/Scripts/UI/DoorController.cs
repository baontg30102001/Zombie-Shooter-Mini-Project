using System;
using System.Linq;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string _requireKey = "key_001";
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.TryGetComponent(out Player player))
        {
            player = other.GetComponent<Player>();
            if (_requireKey != "")
            {
                if (player.Inventory.FirstOrDefault(s => s == _requireKey) != null)
                {
                    player.Inventory.Remove(_requireKey);
                    _animator.SetTrigger("Open");
                    this.GetComponent<BoxCollider>().enabled = false;
                }
            }
            else
            {
                _animator.SetTrigger("Open");
                this.GetComponent<BoxCollider>().enabled = false;
            }
            
        }
    }
}
