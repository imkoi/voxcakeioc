using System;
using System.Threading.Tasks;

namespace VoxCake.IoC
{
    public interface IContainer
    {
        /// <summary>
        /// Raised when container bind dependencies to resolve them.
        /// </summary>
        event Action<IBinder> OnBindDependencies;
        
        /// <summary>
        /// Raised when all dependencies resolved and registered
        /// </summary>
        event Action OnDependenciesResolved;
        
        /// <summary>
        /// Return progress of container resolving. From 0.0f to 1.0f
        /// </summary>
        float ResolveProgress { get; }

        /// <summary>
        /// Resolve and register all dependencies in container
        /// </summary>
        /// <returns></returns>
        Task ResolveDependenciesAsync();
        
        /// <summary>
        /// Waiting until container will be resolved 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task WaitForContainerResolveAsync<T>();
        
        /// <summary>
        /// Return dependency from container if its exists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetDependency<T>();
        
        /// <summary>
        /// Remove all container resources and dependencies
        /// </summary>
        void Dispose();
    }
}