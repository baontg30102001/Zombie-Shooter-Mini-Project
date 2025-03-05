using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private Vector3 _spawnPosition;
    [SerializeField] private Vector3 _spawnEnemyPosition;
    [SerializeField] private Vector3 _spawnRangeEnemyPosition;
    [SerializeField] private Player _player;
    [SerializeField] private List<MeleeZombie> _meleeZombies;
    [SerializeField] private List<RangeZombie> _rangeZombies;
    [SerializeField] private List<BossZombie> _bossZombies;

    [SerializeField] private UIGameplay _uiGameplay;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

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
        if (!_meleeZombies.IsNullOrEmpty())
        {
            _meleeZombies.ForEach(zb => zb.InitializeFromData("zb_001"));
        }
        if (!_rangeZombies.IsNullOrEmpty())
        {
            _rangeZombies.ForEach(zb => zb.InitializeFromData("zb_101"));
        }
        if (!_bossZombies.IsNullOrEmpty())
        {
            _bossZombies.ForEach(zb => zb.InitializeFromData("zb_201"));
        }
    }

    public void RecheckZombies()
    {
        if (_player.GetPlayerHP() > 0)
        {
            bool allMeleeZombiesDead = _meleeZombies.All(zombie => zombie.GetHP() <= 0);
            bool allRangeZombiesDead = _rangeZombies.All(zombie => zombie.GetHP() <= 0);
            bool allBossZombiesDead = _bossZombies.All(zombie => zombie.GetHP() <= 0);

            if (allMeleeZombiesDead && allRangeZombiesDead && allBossZombiesDead)
            {
                _uiGameplay.GameWinOrLose(true);
            }
        }
        else
        {
            _uiGameplay.GameWinOrLose(false);   
        }
    }
}
