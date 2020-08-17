using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VoxCake.Common.Utilities;
using VoxCake.IoC.Types;

namespace VoxCake.IoC.Bindings
{
    internal class BaseBinding
    {
        private readonly Dictionary<Type, object> _localDependencies;
        private readonly Dictionary<Type, object> _globalDependencies;
        private readonly Dictionary<Type, List<object>> _directDependencies;

        protected BaseBinding(Dictionary<Type, object> localDependencies, Dictionary<Type, object> globalDependencies,
            Dictionary<Type, List<object>> directDependencies)
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
            
            T dependency = default;
            if (bindingType == BindingType.DependencyBinding)
            {
                dependency = InstanceAllocator.Allocate<T>();
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
            var instance = InstanceAllocator.Allocate<T>();
            
            _localDependencies.Add(dependencyKey, instance);

            return new EndBinding(_localDependencies, _globalDependencies, instance, dependencyKey);
        }

        protected IEndBinding As(object instance, Type dependencyKey)
        {
            _localDependencies.Add(dependencyKey, instance);

            return new EndBinding(_localDependencies, _globalDependencies, instance, dependencyKey);
        }
        
        protected IDirectBinding And<T>(object dependency, Type dependencyType, BindingType bindingType,
            List<object> directBindings)
        {
            var instance = InstanceAllocator.Allocate<T>();
            directBindings.Add(instance);
            
            return new DirectBinding(_localDependencies, _globalDependencies, _directDependencies, dependency,
                dependencyType, bindingType, directBindings);
        }

        protected IDirectBinding And(object instance, object dependency, Type dependencyType, BindingType bindingType,
            List<object> directBindings)
        {
            directBindings.Add(instance);
            
            return new DirectBinding(_localDependencies, _globalDependencies, _directDependencies, dependency,
                dependencyType, bindingType, directBindings);
        }
        
        protected IEndBinding To<T>(Type dependencyKey, BindingType bindingType, List<object> directDependencies)
        {
            var instanceType = typeof(T);
            var instance = InstanceAllocator.Allocate<T>();
            var bindingKey = bindingType == BindingType.ImplementationBinding ? dependencyKey : instanceType;
            
            _localDependencies.Add(bindingKey, instance);
            _directDependencies.Add(bindingKey, directDependencies);

            return new EndBinding(_localDependencies, _globalDependencies, instance, bindingKey);
        }

        protected IEndBinding To(object instance, Type dependencyKey, BindingType bindingType, 
            List<object> directBindings)
        {
            var bindingKey = bindingType == BindingType.ImplementationBinding ? dependencyKey : instance.GetType();

            _localDependencies.Add(bindingKey, instance);
            _directDependencies.Add(bindingKey, directBindings);

            return new EndBinding(_localDependencies, _globalDependencies, instance, bindingKey);
        }

        protected void ToGlobalContainer(object dependency, Type dependencyKey)
        {
            _localDependencies.Remove(dependencyKey);
            _globalDependencies.Add(dependencyKey, dependency);
        }

        protected void RemoveDependencyFromLocalContainer<T>()
        {
            var dependencyKey = typeof(T);
            RemoveDependencyFromLocalContainer(dependencyKey);
        }

        protected void RemoveDependencyFromLocalContainer(Type dependencyKey)
        {
            _localDependencies.Remove(dependencyKey);
        }

        protected async Task<object[]> GetDependenciesAsync(Stopwatch sw, int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dependencies = new List<object>();

            await AddCollectionToListAsync(_localDependencies, dependencies, sw, maxTaskFreezeMs, cancellationToken);
            await AddCollectionToListAsync(_globalDependencies, dependencies, sw, maxTaskFreezeMs, cancellationToken);
            await AddCollectionToListAsync(_directDependencies, dependencies, sw, maxTaskFreezeMs, cancellationToken);

            var dependencyArray = dependencies.ToArray();
            await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            
            return dependencyArray;
        }
        
        protected async Task<Dictionary<Type, List<object>>> GetDirectDependenciesAsync(Stopwatch sw, 
            int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            
            return _directDependencies;
        }
        
        private async Task AddCollectionToListAsync(IEnumerable collection, List<object> list,
            Stopwatch sw, int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            foreach (var suspectDependency in collection)
            {
                if (suspectDependency is KeyValuePair<Type, object> keyValuePair)
                {
                    list.Add(keyValuePair.Value);
                }
                else if(suspectDependency is KeyValuePair<Type, List<object>> bindingDependenciesPair)
                {
                    list.AddRange(bindingDependenciesPair.Value);
                }
                else
                {
                    list.Add(suspectDependency);
                }

                await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            }
        }
    }
}