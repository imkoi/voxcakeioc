using UnityEngine;
using VoxCake.IoC;

namespace MyNamespace
{
    public class MainContainerHandler : MonoBehaviour
    {
        [SerializeField] private HudView _hudView;
        private IContainer _container;
        
        private async void Awake()
        {
            _container = new Container(this);
            
            _container.OnBindDependencies += BindDependencies;
            
            await _container.ResolveDependenciesAsync();

            _container.GetDependency<MessageObserver>().Dispatch("Got a message from MessageObserver!");
        }

        private void BindDependencies(IBinder binder)
        {
            binder.Bind(_hudView).To<HudMediator>();

            binder.Bind<MessageObserver>();
            binder.Bind<IMessageLogger>().As<MessageLogger>().ToGlobalContainer();
        }

        public void OnDisable()
        {
            _container?.Dispose();
        }
    }
}
