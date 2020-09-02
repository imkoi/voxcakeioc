using System.Threading.Tasks;
using VoxCake.Common.CustomAwaiters;

namespace VoxCake.IoC
{
    public interface IEndlessBinding
    {
        IEndlessBinding With<TDirectDependency>();
        IFinalBinding AsSingle();
        Task Done(TaskOptions taskOptions);
    }
}