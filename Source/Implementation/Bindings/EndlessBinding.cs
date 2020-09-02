using System.Threading.Tasks;
using VoxCake.Common.CustomAwaiters;

namespace VoxCake.IoC
{
    internal class EndlessBinding : BaseBinding, IEndlessBinding
    {
        internal EndlessBinding()
        {
            
        }
        
        public new IEndlessBinding With<TDirectDependency>()
        {
            return base.With<TDirectDependency>();
        }

        public new IFinalBinding AsSingle()
        {
            return base.AsSingle();
        }

        public new async Task Done(TaskOptions taskOptions)
        {
            await base.Done(taskOptions);
        }
    }
}