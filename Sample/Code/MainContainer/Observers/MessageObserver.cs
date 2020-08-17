using VoxCake.Framework;
using VoxCake.IoC;

namespace MyNamespace
{
    public class MessageObserver : Observer<string>, IRemovableDependency
    {
        void IRemovableDependency.OnRemove()
        {
            RemoveAllListeners();
        }
    }
}