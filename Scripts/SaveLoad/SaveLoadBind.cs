using UnityEngine;
using Zenject;


public class SaveLoadBind : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<SaveManager>().AsSingle();
        Container.Bind<SaveLoadService>().AsSingle();
    }
}