using System;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private Vector3 _spawnPosition;
    [SerializeField] private Vector3 _spawnEnemyPosition;
    [SerializeField] private Vector3 _spawnRangeEnemyPosition;
    [SerializeField] private Player _player;
    [SerializeField] private RangeZombie _zombie;

    [SerializeField] private UIGameplay uiGameplay;
    
    private void Start()
    {
        SpawnPlayer();
        SpawmZombie();
    }

    private void SpawnPlayer()
    {
        _player.Initialize();
    }

    private void SpawmZombie()
    {
        // _zombie.InitializeFromData("zb_001");
        _zombie.InitializeFromData("zb_101");
    }
}
