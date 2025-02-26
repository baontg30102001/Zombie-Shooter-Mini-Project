using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Zenject;

public class GunInstaller : Installer<GunInstaller>
{
    [Inject] 
    private Settings _settings;

    public override void InstallBindings()
    {
        
    }
    [Serializable]
    public class Settings
    {
        [FoldoutGroup("Gun Data")]
        public List<GunData> GunDatas;
    }
}