using System.Threading.Tasks;
using VoxCake.Common.CustomAwaiters;

namespace VoxCake.IoC
{
    internal class Binding<TDependency> : BaseBinding, IBinding<TDependency>
    {
        internal Binding()
        {
            
        }
        
        public new IEndlessBinding To<TImplementation>() where TImplementation : TDependency
        {
            return base.To<TImplementation>();
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