using System.Threading.Tasks;
using VoxCake.Common.CustomAwaiters;

namespace VoxCake.IoC
{
    public interface IBinding<TDependency>
    {
        IEndlessBinding To<TImplementation>() where TImplementation : TDependency;
        IEndlessBinding With<TDirectDependency>();
        IFinalBinding AsSingle();
        Task Done(TaskOptions taskOptions);
    }
}