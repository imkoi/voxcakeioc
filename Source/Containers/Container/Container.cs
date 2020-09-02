using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoxCake.Common.AsyncCollections;
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
        Dictionary<Type, Dependency> IContainer.Dependencies => _containerDependencies;

        private readonly Dictionary<Type, Dependency> _localDependencies;
        private readonly Dictionary<Type, Dependency> _containerDependencies;

        private Type _containerToken;
        private float _resolveProgress;
        private CancellationTokenSource _tokenSource;

        public Container(object containerHandler)
        {
            _localDependencies = new Dictionary<Type, Dependency>();
            _containerDependencies = new Dictionary<Type, Dependency>();

            _containerToken = containerHandler.GetType();
            _tokenSource = new CancellationTokenSource();
        }

        async Task IContainer.ResolveDependenciesAsync(int maxTaskFreezeMs)
        {
            var sw = Stopwatch.StartNew();

            var globalDependencies = new Dictionary<Type, Dependency>();
            var dependencyBinder = GetBindings(_localDependencies, globalDependencies);
            await AddDependenciesToGlobalContainerAsync(globalDependencies, sw, maxTaskFreezeMs, _tokenSource.Token);
            await FillContainerDependencies(globalDependencies, sw, maxTaskFreezeMs, _tokenSource.Token);

            var dependencies = await dependencyBinder.GetDependenciesAsync(sw, maxTaskFreezeMs,
                _tokenSource.Token);
            var directDependencies = await dependencyBinder.GetDirectDependenciesAsync(sw,
                maxTaskFreezeMs, _tokenSource.Token);

            await InjectDependenciesAsync(dependencies, directDependencies, sw, maxTaskFreezeMs, _tokenSource.Token);

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

        TDependencyKey IContainer.GetInstance<TDependencyKey>()
        {
            var dependencyType = typeof(TDependencyKey);

            var availableDependencies = AsyncCollections.GetMergedDictionaryAsync(
                _localDependencies, GlobalContainer.dependencies,
                Stopwatch.StartNew(), 0, CancellationToken.None).GetAwaiter().GetResult();
            
            if (availableDependencies.ContainsKey(dependencyType))
            {
                return (TDependencyKey) availableDependencies[dependencyType].Value;
            }

            throw new Exception($"There are no dependency \"{typeof(TDependencyKey).Name}\" in {GetType().Name}");
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

        private async Task AddDependenciesToGlobalContainerAsync(Dictionary<Type, Dependency> dependencies, Stopwatch sw,
            int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var dependency in dependencies)
            {
                GlobalContainer.dependencies.Add(dependency.Key, dependency.Value);
                await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            }
        }
        
        private async Task FillContainerDependencies(Dictionary<Type, Dependency> globalDependencies,
            Stopwatch sw, int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            foreach (var dependency in _localDependencies)
            {
                await AsyncCollections.AddPairToCollectionAsync(dependency, _containerDependencies,
                    sw, maxTaskFreezeMs, cancellationToken);
            }

            foreach (var dependency in globalDependencies)
            {
                await AsyncCollections.AddPairToCollectionAsync(dependency, _containerDependencies,
                    sw, maxTaskFreezeMs, cancellationToken);
            }
        }

        private async Task InjectDependenciesAsync(Dictionary<Type, Dependency> dependencies,
            Dictionary<Type, List<Dependency>> directDependencies,
            Stopwatch sw, int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dependenciesCount = dependencies.Count;
            var index = 0;

            foreach (var keyValuePair in dependencies)
            {
                var dependency = keyValuePair.Value;
                var dependencyKey = keyValuePair.Key;
                if (dependency.IsInjectable)
                {
                    var availableDependencies = await AsyncCollections.GetMergedDictionaryAsync(
                        _localDependencies, GlobalContainer.dependencies,
                        sw, maxTaskFreezeMs, cancellationToken);

                    if (dependency.IsDirect)
                    {
                        foreach (var bindedDependency in directDependencies[dependencyKey])
                        {
                            var dependencyPair = new KeyValuePair<Type, Dependency>(bindedDependency.GetType(),
                                bindedDependency);
                            await AsyncCollections.AddPairToCollectionAsync(dependencyPair, availableDependencies,
                                sw, maxTaskFreezeMs, cancellationToken);
                        }
                    }

                    await ConstructorInjector.InjectDependenciesToInstanceAsync(availableDependencies, dependency,
                        sw, maxTaskFreezeMs, cancellationToken);
                }

                _resolveProgress = (index + 1) / (float) dependenciesCount;
                index++;
            }
        }

        private Binder GetBindings(Dictionary<Type, Dependency> localDependencies,
            Dictionary<Type, Dependency> globalDependencies)
        {
            var dependencyBinder = new Binder(localDependencies, globalDependencies);
            BindDependencies?.Invoke(dependencyBinder);

            return dependencyBinder;
        }

        private void RegisterDependencies(Dictionary<Type, Dependency> dependencies)
        {
            foreach (var keyValuePair in dependencies)
            {
                RaiseDependencyCallback(keyValuePair.Value, DependencyCallbackType.OnRegister);
            }
        }
        
        private void MarkContainerAsResolved(Type containerHandlerType)
        {
            GlobalContainer.resolvedContainers.Add(containerHandlerType);
        }

        private void RemoveDependencies(Dictionary<Type, Dependency> dependencies)
        {
            foreach (var keyValuePair in dependencies)
            {
                RaiseDependencyCallback(keyValuePair.Value, DependencyCallbackType.OnRemove);
            }

            _localDependencies.Clear();
        }

        private void RaiseDependencyCallback(Dependency dependency, DependencyCallbackType callback)
        {
            switch (callback)
            {
                case DependencyCallbackType.OnRegister:
                    if (dependency.Value is IRegistrableDependency registrableDependency)
                    {
                        registrableDependency.OnRegister();
                    }
                    break;
                case DependencyCallbackType.OnRemove:
                    if (dependency.Value is IRemovableDependency removableDependency)
                    {
                        removableDependency.OnRemove();
                    }
                    break;
            }
        }

        private bool IsContainerResolved<T>()
        {
            var targetContainer = typeof(T);
            return GlobalContainer.resolvedContainers.Contains(targetContainer);
        }
    }
}