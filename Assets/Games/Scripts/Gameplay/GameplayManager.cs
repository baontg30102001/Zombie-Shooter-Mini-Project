using System;
using UnityEngine;
using Zenject;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private Vector3 _spawnPosition;
    [SerializeField] private Player _player;
    private void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        _player.Initialize();
    }
}
