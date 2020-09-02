using System.Threading.Tasks;
using VoxCake.Common.CustomAwaiters;

namespace VoxCake.IoC
{
    internal class FinalBinding : BaseBinding, IFinalBinding
    {
        internal FinalBinding()
        {
            
        }
        
        public new async Task Done(TaskOptions taskOptions)
        {
            await base.Done(taskOptions);
        }
    }
}