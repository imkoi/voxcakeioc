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
        public event Action<IBinder> BindDependencies;
        public event Action DependenciesResolved;

        float IContainer.ResolveProgress => _resolveProgress;
        Dictionary<Type, object> IContainer.Dependencies => _containerDependencies;
        
        private readonly Dictionary<Type, object> _localDependencies;
        private readonly Dictionary<Type, object> _containerDependencies;

        private Type _containerToken;
        private float _resolveProgress;
        private CancellationTokenSource _tokenSource;

        public Container(object containerHandler)
        {
            _localDependencies = new Dictionary<Type, object>();
            _containerDependencies = new Dictionary<Type, object>();

            _containerToken = containerHandler.GetType();
            _tokenSource = new CancellationTokenSource();
        }

        async Task IContainer.ResolveDependenciesAsync(int maxTaskFreezeMs)
        {
            var sw = Stopwatch.StartNew();
            
            var globalDependencies = new Dictionary<Type, object>();
            var dependencyBinder = GetBindings(_localDependencies, globalDependencies);
            await AddDependenciesToGlobalContainerAsync(globalDependencies, sw, maxTaskFreezeMs, _tokenSource.Token);
            await FillContainerDependencies(globalDependencies, sw, maxTaskFreezeMs, _tokenSource.Token);
            
            var dependencies = await dependencyBinder.GetDependenciesAsync(sw, maxTaskFreezeMs,
                _tokenSource.Token);
            var directDependencies = await dependencyBinder.GetDirectDependenciesAsync(sw,
                maxTaskFreezeMs, _tokenSource.Token);
            
            await InjectDependenciesAsync(dependencies, sw, maxTaskFreezeMs, _tokenSource.Token);
            await InjectDirectDependenciesAsync(directDependencies, sw, maxTaskFreezeMs, _tokenSource.Token);
            
            RegisterDependencies(dependencies);
            MarkContainerAsResolved(_containerToken);
            
            DependenciesResolved?.Invoke();
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

        void IContainer.SetToken<TContainerToken>()
        {
            _containerToken = typeof(TContainerToken);
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
                RemoveDependencies(_localDependencies);
            }

            _tokenSource.Dispose();
            GlobalContainer.resolvedContainers.Remove(_containerToken);
        }

        private async Task AddDependenciesToGlobalContainerAsync(Dictionary<Type, object> dependencies, Stopwatch sw,
            int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            foreach (var dependency in dependencies)
            {
                GlobalContainer.dependencies.Add(dependency.Key, dependency.Value);
                await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            }
        }
        
        private async Task InjectDependenciesAsync(object[] instances, Stopwatch sw, int maxTaskFreezeMs,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var instancesLength = instances.Length;
            for (int i = 0; i < instancesLength; i++)
            {
                await ConstructorInjector.InjectDependenciesToInstanceAsync(_localDependencies,
                    GlobalContainer.dependencies, instances[i], sw, maxTaskFreezeMs, cancellationToken);
                
                _resolveProgress = (i + 1) / (float)instancesLength;
            }
        }
        
        private async Task InjectDirectDependenciesAsync(Dictionary<Type, List<object>> directDependencies,
            Stopwatch sw, int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
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
        
        private async Task<Dictionary<Type, object>> GetAvailableDependenciesAsync(Stopwatch sw, int maxTaskFreezeMs,
            CancellationToken cancellationToken)
        {
            var dependencies = new Dictionary<Type, object>();
            
            var availableDependencies = _localDependencies.Concat(GlobalContainer.dependencies);
            await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            
            foreach (var keyValuePair in availableDependencies)
            {
                dependencies.Add(keyValuePair.Key, keyValuePair.Value);
                await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            }

            return dependencies;
        }
        
        private async Task FillContainerDependencies(Dictionary<Type, object> globalDependencies, Stopwatch sw,
            int maxTaskFreezeMs, CancellationToken cancellationToken) // TODO: Refactor this task
        {
            foreach (var dependency in _localDependencies)
            {
                _containerDependencies.Add(dependency.Key, dependency.Value);
                await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            }
            
            foreach (var dependency in globalDependencies)
            {
                _containerDependencies.Add(dependency.Key, dependency.Value);
                await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            }
        }
        
        private Dictionary<Type, object> GetAvailableDependencies()
        {
            var dependencies = new Dictionary<Type, object>();
            
            var availableDependencies = _localDependencies.Concat(GlobalContainer.dependencies);
            foreach (var keyValuePair in availableDependencies)
            {
                dependencies.Add(keyValuePair.Key, keyValuePair.Value);
            }

            return dependencies;
        }
        
        private Binder GetBindings(Dictionary<Type, object> localDependencies,
            Dictionary<Type, object> globalDependencies)
        {
            var dependencyBinder = new Binder(localDependencies, globalDependencies);
            BindDependencies?.Invoke(dependencyBinder);

            return dependencyBinder;
        }

        private object GetDependency(Type type, Dictionary<Type, object> availableDependencies)
        {
            if (availableDependencies.ContainsKey(type))
            {
                return availableDependencies[type];
            }
            
            throw new Exception($"There are no dependency \"{type.Name}\" in {GetType().Name}");
        }

        private void MarkContainerAsResolved(Type containerHandlerType)
        {
            GlobalContainer.resolvedContainers.Add(containerHandlerType);
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
            
            _localDependencies.Clear();
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