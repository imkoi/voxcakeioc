using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoxCake.Common.AsyncCollections;
using VoxCake.Common.Utilities;
using VoxCake.IoC.Types;

namespace VoxCake.IoC.Bindings
{
    internal class BaseBinding
    {
        private readonly Dictionary<Type, Dependency> _localDependencies;
        private readonly Dictionary<Type, Dependency> _globalDependencies;
        private readonly Dictionary<Type, List<Dependency>> _directDependencies;

        protected BaseBinding(Dictionary<Type, Dependency> localDependencies,
            Dictionary<Type, Dependency> globalDependencies,
            Dictionary<Type, List<Dependency>> directDependencies)
        {
            _localDependencies = localDependencies;
            _globalDependencies = globalDependencies;
            _directDependencies = directDependencies;
        }

        protected IBinding Bind<T>()
        {
            var dependencyKey = typeof(T);
            var bindingType = dependencyKey.IsInterface
                ? BindingType.ImplementationBinding
                : BindingType.DependencyBinding;
            
            Dependency dependency = null;
            if (bindingType == BindingType.DependencyBinding)
            {
                dependency = AllocateDependency<T>(dependencyKey, false);
                _localDependencies.Add(dependencyKey, dependency);
            }

            return new Binding(_localDependencies, _globalDependencies, _directDependencies, dependency, dependencyKey,
                bindingType);
        }

        protected IBinding Bind(object dependency)
        {
            var dependencyKey = dependency.GetType();
            
            _localDependencies.Add(dependencyKey, dependency);

            return new Binding(_localDependencies, _globalDependencies, _directDependencies, dependency, dependencyKey,
                BindingType.DependencyBinding);
        }
        
        protected IEndBinding As<T>(Type dependencyKey)
        {
            var dependency = AllocateDependency<T>(dependencyKey, true, false);
            _localDependencies.Add(dependencyKey, dependency);

            return new EndBinding(_localDependencies, _globalDependencies, instance, dependencyKey);
        }

        protected IEndBinding As(object instance, Type dependencyKey)
        {
            var dependency = new Dependency(dependencyKey, instance, false, false);
            _localDependencies.Add(dependencyKey, dependency);

            return new EndBinding(_localDependencies, _globalDependencies, instance, dependencyKey);
        }
        
        protected IRawBinding Raw<T>(object dependency, Type dependencyType, BindingType bindingType,
            List<Dependency> directBindings)
        {
            var instance = InstanceAllocator.Allocate<T>();
            directBindings.Add(instance);
            
            return new RawBinding(_localDependencies, _globalDependencies, _directDependencies, dependency,
                dependencyType, bindingType, directBindings);
        }

        protected IRawBinding Raw(object instance, object dependency, Type dependencyType, BindingType bindingType,
            List<Dependency> directBindings)
        {
            directBindings.Add(instance);
            
            return new RawBinding(_localDependencies, _globalDependencies, _directDependencies, dependency,
                dependencyType, bindingType, directBindings);
        }
        
        protected IEndBinding To<T>(Type dependencyKey, BindingType bindingType, List<Dependency> directDependencies)
        {
            var instanceType = typeof(T);
            var instance = InstanceAllocator.Allocate<T>();
            var bindingKey = bindingType == BindingType.ImplementationBinding ? dependencyKey : instanceType;
            
            _localDependencies.Add(bindingKey, instance);
            _directDependencies.Add(bindingKey, directDependencies);

            return new EndBinding(_localDependencies, _globalDependencies, instance, bindingKey);
        }

        protected IEndBinding To(object instance, Type dependencyKey, BindingType bindingType, 
            List<Dependency> directBindings)
        {
            var bindingKey = bindingType == BindingType.ImplementationBinding ? dependencyKey : instance.GetType();

            _localDependencies.Add(bindingKey, instance);
            _directDependencies.Add(bindingKey, directBindings);

            return new EndBinding(_localDependencies, _globalDependencies, instance, bindingKey);
        }

        protected void ToGlobalContainer(Type dependencyKey, Dependency dependency)
        {
            _localDependencies.Remove(dependencyKey);
            _globalDependencies.Add(dependencyKey, dependency);
        }
        
        protected void RemoveDependencyFromLocalContainer(Type dependencyKey)
        {
            _localDependencies.Remove(dependencyKey);
        }

        protected async Task<Dictionary<Type, Dependency>> GetDependenciesAsync(Stopwatch sw, int maxTaskFreezeMs,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dependencies = new Dictionary<Type, Dependency>();

            await AddPairsToCollectionAsync(_localDependencies.ToArray(), dependencies,
                sw, maxTaskFreezeMs, cancellationToken);
            
            await AddPairsToCollectionAsync(_globalDependencies.ToArray(), dependencies,
                sw, maxTaskFreezeMs, cancellationToken);

            return dependencies;
        }
        
        protected async Task<Dictionary<Type, List<Dependency>>> GetDirectDependenciesAsync(Stopwatch sw, 
            int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            
            return _directDependencies;
        }
        
        private async Task AddPairsToCollectionAsync(KeyValuePair<Type, Dependency>[] pairs,
            Dictionary<Type, Dependency> dependencies,
            Stopwatch sw, int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            foreach (var pair in pairs)
            {
                await AsyncCollections.AddPairToCollectionAsync(pair, dependencies,
                    sw, maxTaskFreezeMs, cancellationToken);
            }
        }

        private Dependency AllocateDependency<T>(Type dependencyKey, bool isDirect)
        {
            var instance = InstanceAllocator.Allocate<T>();
            return new Dependency(dependencyKey, instance, true, isDirect);
        }
    }
}