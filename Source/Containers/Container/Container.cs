using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoxCake.Common.Utilities;
using VoxCake.IoC.Bindings;
using VoxCake.IoC.Types;
using VoxCake.IoC.Utilities;

namespace VoxCake.IoC
{
    public class Container : IContainer, IDisposable
    {
        public event Action<IBinder> OnBindDependencies;
        public event Action OnDependenciesResolved;

        float IContainer.ResolveProgress => _resolveProgress;

        private readonly int _maxTaskFreezeMs;
        private readonly Type _containerHandlerType;
        private readonly Dictionary<Type, object> _dependencies;

        private float _resolveProgress;
        private CancellationTokenSource _tokenSource;

        public Container(object containerHandler, int maxTaskFreezeMs = 16)
        {
            _tokenSource = new CancellationTokenSource();

            _maxTaskFreezeMs = maxTaskFreezeMs;
            _containerHandlerType = containerHandler.GetType();
            _dependencies = new Dictionary<Type, object>();
        }

        async Task IContainer.ResolveDependenciesAsync()
        {
            var dependencyBinder = new Binder(_dependencies, GlobalContainer.dependencies);
            OnBindDependencies?.Invoke(dependencyBinder);

            var dependencies = await dependencyBinder.GetDependenciesAsync(_maxTaskFreezeMs, _tokenSource.Token);
            var directDependencies = await dependencyBinder.GetDirectDependenciesAsync(_maxTaskFreezeMs,
                _tokenSource.Token);
            
            await InjectDependenciesAsync(dependencies, _maxTaskFreezeMs, _tokenSource.Token);
            await InjectDirectDependenciesAsync(directDependencies, _maxTaskFreezeMs, _tokenSource.Token);
            
            RegisterDependencies(dependencies);
            
            GlobalContainer.resolvedContainers.Add(_containerHandlerType);
            OnDependenciesResolved?.Invoke();
        }
        
        async Task IContainer.WaitForContainerResolveAsync<TContainerHandler>()
        {
            while (!IsContainerResolved<TContainerHandler>())
            {
                await Awaiter.WaitMs(_tokenSource.Token);
            }
        }

        TDependency IContainer.GetDependency<TDependency>()
        {
            var dependencyType = typeof(TDependency);
            
            var availableDependencies = GetAvailableDependencies();
            if (availableDependencies.ContainsKey(dependencyType))
            {
                return (TDependency)availableDependencies[dependencyType];
            }
            
            throw new Exception($"There are no dependency \"{typeof(TDependency).Name}\" in {GetType().Name}");
        }

        public void Dispose()
        {
            try
            {
                _tokenSource.Cancel();
            }
            catch
            {
                // ignored
            }
            finally
            {
                RemoveDependencies(_dependencies);
            }

            _tokenSource.Dispose();
            GlobalContainer.resolvedContainers.Remove(_containerHandlerType);
        }
        
        private object GetDependency(Type type, Dictionary<Type, object> availableDependencies)
        {
            if (availableDependencies.ContainsKey(type))
            {
                return availableDependencies[type];
            }
            
            throw new Exception($"There are no dependency \"{type.Name}\" in {GetType().Name}");
        }
        
        private async Task InjectDependenciesAsync(object[] instances, int maxTaskFreezeMs,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sw = Stopwatch.StartNew();
            var instancesLength = instances.Length;
            for (int i = 0; i < instancesLength; i++)
            {
                await ConstructorInjector.InjectDependenciesToInstanceAsync(_dependencies,
                    GlobalContainer.dependencies, instances[i], sw, maxTaskFreezeMs, cancellationToken);
                
                _resolveProgress = (i + 1) / (float)instancesLength;
            }
        }

        private async Task InjectDirectDependenciesAsync(Dictionary<Type, List<object>> directDependencies,
            int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            
            foreach (var keyValuePair in directDependencies)
            {
                var availableDependencies = await GetAvailableDependenciesAsync(sw, maxTaskFreezeMs,
                    cancellationToken);
                
                var dependency = GetDependency(keyValuePair.Key, availableDependencies);
                var dependencies = keyValuePair.Value;

                foreach (var availableDependency in availableDependencies)
                {
                    dependencies.Add(availableDependency.Value);
                    await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
                }

                await ConstructorInjector.InjectDependenciesToInstanceAsync(dependencies.ToArray(),
                    dependency, sw, maxTaskFreezeMs, cancellationToken);
            }
        }

        private void RegisterDependencies(object[] dependencies)
        {
            foreach (var dependency in dependencies)
            {
                RaiseDependencyCallback(dependency, DependencyCallbackType.OnRegister);
            }
        }

        private void RemoveDependencies(Dictionary<Type, object> dependencies)
        {
            foreach (var keyValuePair in dependencies)
            {
                RaiseDependencyCallback(keyValuePair.Value, DependencyCallbackType.OnRemove);
            }
            
            _dependencies.Clear();
        }
        
        private Dictionary<Type, object> GetAvailableDependencies()
        {
            var dependencies = new Dictionary<Type, object>();
            
            var availableDependencies = _dependencies.Concat(GlobalContainer.dependencies);
            foreach (var keyValuePair in availableDependencies)
            {
                dependencies.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return dependencies;
        }
        
        private async Task<Dictionary<Type, object>> GetAvailableDependenciesAsync(Stopwatch sw, int maxTaskFreezeMs,
            CancellationToken cancellationToken)
        {
            var dependencies = new Dictionary<Type, object>();
            
            var availableDependencies = _dependencies.Concat(GlobalContainer.dependencies);
            await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            
            foreach (var keyValuePair in availableDependencies)
            {
                dependencies.Add(keyValuePair.Key, keyValuePair.Value);
                await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            }

            return dependencies;
        }

        private void RaiseDependencyCallback(object dependency, DependencyCallbackType callback)
        {
            switch (callback)
            {
                case DependencyCallbackType.OnRegister:
                    if (dependency is IRegistrableDependency registrableDependency)
                    {
                        registrableDependency.OnRegister();
                    }
                    break;
                case DependencyCallbackType.OnRemove:
                    if (dependency is IRemovableDependency removableDependency)
                    {
                        removableDependency.OnRemove();
                    }
                    break;
            }
        }
        
        private bool IsContainerResolved<T>()
        {
            var targetContainer = typeof(T);
            if (GlobalContainer.resolvedContainers.Contains(targetContainer))
            {
                return true;
            }

            return false;
        }
    }
}