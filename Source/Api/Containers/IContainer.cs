using System;
using System.Threading.Tasks;
using VoxCake.Common.CustomAwaiters;

namespace VoxCake.IoC
{
    public interface IContainer
    {
        event Action<IBindingsBuilder> Bind;

        Task ResolveDependencies(TaskOptions taskOptions);

        void Dispose();
    }
}