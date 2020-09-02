using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace VoxCake.IoC.Bindings
{
    internal class Binder : BaseBinding, IBinder
    {
        internal Binder(Dictionary<Type, Dependency> localDependencies, Dictionary<Type, Dependency> globalDependencies)
        : base(localDependencies, globalDependencies, new Dictionary<Type, List<Dependency>>())
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

        internal new async Task<Dictionary<Type, Dependency>> GetDependenciesAsync(Stopwatch sw, int maxTaskFreezeMs,
            CancellationToken cancellationToken)
        {
            return await base.GetDependenciesAsync(sw, maxTaskFreezeMs, cancellationToken);
        }
        
        internal new async Task<Dictionary<Type, List<Dependency>>> GetDirectDependenciesAsync(Stopwatch sw,
            int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            return await base.GetDirectDependenciesAsync(sw, maxTaskFreezeMs, cancellationToken);
        }
    }
}