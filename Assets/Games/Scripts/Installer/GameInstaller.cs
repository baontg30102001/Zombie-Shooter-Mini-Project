using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        InstallEntity();
        InstallGun();
        InstallBullet();
    }

    private void InstallGun()
    {
        GunInstaller.Install(Container);   
    }

    private void InstallBullet()
    {
        BulletInstaller.Install(Container);
    }

    private void InstallEntity()
    {
        EntityIntaller.Install(Container);
    }
}