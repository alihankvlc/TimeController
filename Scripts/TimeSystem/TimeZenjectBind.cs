using UnityEngine;
using Zenject;

namespace _Project.Scripts.TimeSystem
{
    public class TimeZenjectBind : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<TimeSettings>().FromComponentsInHierarchy().AsSingle();
        }
    }
}