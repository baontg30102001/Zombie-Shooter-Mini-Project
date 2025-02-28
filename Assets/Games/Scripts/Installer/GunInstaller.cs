using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class GunInstaller : Installer<GunInstaller>
{
    [Inject] 
    private Settings _settings;

    public override void InstallBindings()
    {
        Container.BindFactory<M4A1, M4A1.Factory>()
            .FromComponentInNewPrefab(_settings.m4a1Prefab)
            .WithGameObjectName(_settings.m4a1Prefab.name);
        Container.BindFactory<M32A1, M32A1.Factory>()
            .FromComponentInNewPrefab(_settings.m32a1Prefab)
            .WithGameObjectName(_settings.m32a1Prefab.name);
    }
    [Serializable]
    public class Settings
    {
        [FoldoutGroup("Prefabs")] public GameObject m4a1Prefab;
        [FoldoutGroup("Prefabs")] public GameObject m32a1Prefab;
        
        [FoldoutGroup("Gun Data")]
        public List<GunData> GunDatas;

        public GunData GetGunDataById(string gunId)
        {
            return GunDatas.FirstOrDefault(gun => gun.gunId == gunId);
        }
    }
}