using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class BulletInstaller : Installer<BulletInstaller>
{
    [Inject]
    private Settings _settings;
    
    public override void InstallBindings()
    {
        Container.BindFactory<Vector3, Bullet_556, Bullet_556.Factory>()
            .FromMonoPoolableMemoryPool(x => x.WithInitialSize(6)
            .FromComponentInNewPrefab(_settings.b556Prefab)
            .WithGameObjectName(_settings.b556Prefab.name));
        Container.BindFactory<Vector3, Bullet_40, Bullet_40.Factory>()
            .FromMonoPoolableMemoryPool(x => x.WithInitialSize(6)
                .FromComponentInNewPrefab(_settings.b40Prefab)
                .WithGameObjectName(_settings.b40Prefab.name));

    }
    [Serializable]
    public class Settings
    {
        [FoldoutGroup("Prefabs")] public GameObject b556Prefab;
        [FoldoutGroup("Prefabs")] public GameObject b40Prefab;
        
        [FoldoutGroup("Bullet Data")]
        public List<BulletData> BulletDatas;
        
        public BulletData GetBulletDataById(string bulletId)
        {
            return BulletDatas.FirstOrDefault(bullet => bullet.bulletId == bulletId);
        }
    }
}