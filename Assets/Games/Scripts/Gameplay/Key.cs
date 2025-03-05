using System;
using UnityEngine;

public class Key : MonoBehaviour
{
    [SerializeField] private string key;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            player.Inventory.Add(key);
            Destroy(gameObject);
        }
    }
}
