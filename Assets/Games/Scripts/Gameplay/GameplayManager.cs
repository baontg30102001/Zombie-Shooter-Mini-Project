using System;
using UnityEngine;
using Zenject;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private Vector3 _spawnPosition;
    [SerializeField] private Vector3 _spawnEnemyPosition;
    [SerializeField] private Vector3 _spawnRangeEnemyPosition;
    [SerializeField] private Player _player;
    [SerializeField] private MeleeZombie _zombie;
    [SerializeField] private RangeZombie _zombieRange;
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
        _zombieRange.InitializeFromData("zb_101");
    }
}
