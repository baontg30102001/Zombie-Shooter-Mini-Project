using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        InitSignalBus();
        
        
    }

    private void InitSignalBus()
    {
        SignalBusInstaller.Install(Container);

        #region Declare Signal

        //Declare Signal here

        #endregion
    }
}