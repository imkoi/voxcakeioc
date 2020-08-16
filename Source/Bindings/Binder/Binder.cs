using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace VoxCake.IoC.Bindings
{
    internal class Binder : BaseBinding, IBinder
    {
        internal Binder(Dictionary<Type, object> localDependencies, Dictionary<Type, object> globalDependencies)
        : base(localDependencies, globalDependencies, new Dictionary<Type, List<object>>())
        {
        }
        
        IBinding IBinder.Bind<TDependency>()
        {
            return base.Bind<TDependency>();
        }
        
        IBinding IBinder.Bind(object dependency)
        {
            return base.Bind(dependency);
        }

        internal new async Task<object[]> GetDependenciesAsync(int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            return await base.GetDependenciesAsync(maxTaskFreezeMs, cancellationToken);
        }
    }
}