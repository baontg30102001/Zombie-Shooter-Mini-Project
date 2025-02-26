using UnityEngine;
using Zenject;

[CreateAssetMenu(menuName = "Zombie-Shooter-Mini-Project/Game Settings")]
public class GameSettingInstaller : ScriptableObjectInstaller<GameSettingInstaller>
{
    public GunInstaller.Settings gunSettings;
    public ButtletInstaller.Settings bulletSettings;
    public EntityIntaller.Settings entitySettings;
    public override void InstallBindings()
    {
        Container.BindInstance(gunSettings).IfNotBound();
        
        Container.BindInstance(bulletSettings).IfNotBound();
        
        Container.BindInstance(entitySettings).IfNotBound();
    }
}