using System.Threading.Tasks;
using VoxCake.Common.CustomAwaiters;

namespace VoxCake.IoC
{
    public interface IFinalBinding
    {
        Task Done(TaskOptions taskOptions);
    }
}