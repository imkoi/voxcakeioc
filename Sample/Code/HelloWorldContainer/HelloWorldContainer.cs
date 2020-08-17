using UnityEngine;
using VoxCake.IoC;

namespace MyNamespace
{
    public class HelloWorldContainer : MonoBehaviour
    {
        private IContainer _container;
        
        private async void Awake()
        {
            _container = new Container(this);
            
            _container.OnBindDependencies += BindDependencies;

            await _container.WaitForContainerResolveAsync<MainContainerHandler>();
            await _container.ResolveDependenciesAsync();
            
            _container.GetDependency<HelloWorldObserver>().Dispatch();
        }

        private void BindDependencies(IBinder binder)
        {
            binder.Bind<HelloWorldObserver>();
            binder.Bind<HelloWorldLogger>();
        }

        public void OnDisable()
        {
            _container?.Dispose();
        }
    }
}

