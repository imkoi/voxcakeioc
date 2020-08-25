using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VoxCake.IoC
{
    public interface IContainer
    {
        /// <summary>
        /// Raised when container bind dependencies to resolve them.
        /// </summary>
        event Action<IBinder> BindDependencies;
        
        /// <summary>
        /// Raised when all dependencies resolved and registered
        /// </summary>
        event Action DependenciesResolved;
        
        /// <summary>
        /// Return progress of container resolving. From 0.0f to 1.0f
        /// </summary>
        float ResolveProgress { get; }
        
        /// <summary>
        /// Return container dependencies;
        /// </summary>
        Dictionary<Type, object> Dependencies { get; }

        /// <summary>
        /// Resolve and register all dependencies in container
        /// </summary>
        /// <returns></returns>
        Task ResolveDependenciesAsync(int maxTaskFreezeMs = 16);
        
        /// <summary>
        /// Waiting until container will be resolved 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task WaitForContainerResolveAsync<T>();
        
        /// <summary>
        /// Return dependency from container if its exists
        /// </summary>
        /// <typeparam name="TDependencyKey"></typeparam>
        /// <returns></returns>
        TDependencyKey GetDependency<TDependencyKey>();

        /// <summary>
        /// Set container token to check if its resolved in other containers
        /// </summary>
        /// <typeparam name="TContainerToken"></typeparam>
        void SetToken<TContainerToken>();
        
        /// <summary>
        /// Remove all container resources and dependencies
        /// </summary>
        void Dispose();
    }
}