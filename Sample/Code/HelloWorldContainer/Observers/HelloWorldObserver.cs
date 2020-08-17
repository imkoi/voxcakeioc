using VoxCake.Framework;
using VoxCake.IoC;

namespace MyNamespace
{
    public class HelloWorldObserver : Observer, IRemovableDependency
    {
        void IRemovableDependency.OnRemove()
        {
            RemoveAllListeners();
        }
    }
}