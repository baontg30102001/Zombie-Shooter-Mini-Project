using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class EntityIntaller : Installer<EntityIntaller>
{
    [Inject]
    private Settings _settings;
    public override void InstallBindings()
    {
    }
    
    [Serializable]
    public class Settings
    {
        [FoldoutGroup("Player Data")]
        public PlayerData PlayerData;

        [FoldoutGroup("Melee Zombie Data")] 
        public List<MeleeZombieData> MeleeZombieDatas;

        [FoldoutGroup("Range Zombie Data")] 
        public List<RangeZombieData> RangeZombieDatas;

        [FoldoutGroup("Boss Zombie Data")] 
        public List<BossZombieData> BossZombieDatas;
        
        public MeleeZombieData GetMeleeZombieDataById(string meleeZombieId)
        {
            return MeleeZombieDatas.FirstOrDefault(data => data.entityId == meleeZombieId);
        }
        
        public RangeZombieData GetRangeZombieDataById(string rangeZombieId)
        {
            return RangeZombieDatas.FirstOrDefault(data => data.entityId == rangeZombieId);
        }
        
        public BossZombieData GetBossZombieDataById(string bossZombieId)
        {
            return BossZombieDatas.FirstOrDefault(data => data.entityId == bossZombieId);
        }
    }

    
}