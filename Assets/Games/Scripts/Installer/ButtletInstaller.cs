using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Zenject;

public class ButtletInstaller : Installer<ButtletInstaller>
{
    [Inject]
    private Settings _settings;
    
    public override void InstallBindings()
    {
        
    }
    [Serializable]
    public class Settings
    {
        [FoldoutGroup("Bullet Data")]
        public List<BulletData> BulletDatas;
    }
}