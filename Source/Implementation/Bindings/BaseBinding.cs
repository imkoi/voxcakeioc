using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoxCake.Common.CustomAwaiters;

namespace VoxCake.IoC
{
    internal class BaseBinding
    {
        private readonly Dictionary<Type, List<object>> _bindings;

        internal BaseBinding()
        {
            _bindings = new Dictionary<Type, List<object>>(); 
        }
        
        internal IEndlessBinding To<TImplementation>()
        {
            return new EndlessBinding();
        }
        
        internal IEndlessBinding With<TDirectDependency>()
        {
            return new EndlessBinding();
        }

        internal IFinalBinding AsSingle()
        {
            return new FinalBinding();
        }
        
        internal async Task Done(TaskOptions taskOptions)
        {
            
            await CustomAwaiter.WaitMs(taskOptions);
        }
    }
}