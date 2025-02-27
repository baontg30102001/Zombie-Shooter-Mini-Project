using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public partial class Player : MonoBehaviour
{
    [SerializeField] private int health = 100;
    [SerializeField] private Gun currentGun;
    [SerializeField] private List<Gun> _gunInInventory;
    [SerializeField] private Dictionary<string, int> _inventory;
    
    private void Start()
    {
        SetupMovementComponents();
    }

    private void Update()
    {
        HandlerShooting();
        HandlerMovement();
    }
    
    public class Factory : PlaceholderFactory<Player>
    {
        
    }
}
