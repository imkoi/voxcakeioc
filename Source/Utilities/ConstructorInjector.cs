using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using VoxCake.Common.Utilities;

namespace VoxCake.IoC.Utilities
{
    internal static class ConstructorInjector
    {
        private const BindingFlags BindingFlag = BindingFlags.FlattenHierarchy 
                                         | BindingFlags.Public 
                                         | BindingFlags.Instance 
                                         | BindingFlags.InvokeMethod;
        
        internal static async Task InjectDependenciesToInstanceAsync(Dictionary<Type, object> localDependencies,
            Dictionary<Type, object> globalDependencies, object instance, Stopwatch sw, int maxTaskFreezeMs, 
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var instanceType = instance.GetType();
            var constructorParamsCollection = await GetInjectableConstructorsAsync(instanceType,
                sw, maxTaskFreezeMs, cancellationToken);

            foreach (var constructorParamsPair in constructorParamsCollection)
            {
                var constructor = constructorParamsPair.Key;
                var parameters = constructorParamsPair.Value;

                var isInjectable = true;
                var parametersCount = parameters.Length;
                var constructorDependencies = new object[parametersCount];
                
                for (var i = 0; i < parametersCount; i++)
                {
                    var constructorDependency = GetDependencyOfTypeAsync(parameters[i], localDependencies,
                        globalDependencies);
                    constructorDependencies[i] = constructorDependency;
                    
                    await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
                    
                    if (constructorDependency == null)
                    {
                        isInjectable = false;
                        break;
                    }
                }

                if (isInjectable)
                {
                    constructor.Invoke(instance, constructorDependencies);
                }

                await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            }
        }
        
        internal static async Task InjectDependenciesToInstanceAsync(object[] availableDependencies, object instance,
            Stopwatch sw, int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var instanceType = instance.GetType();
            var constructorParamsCollection = await GetInjectableConstructorsAsync(instanceType,
                sw, maxTaskFreezeMs, cancellationToken);

            foreach (var constructorParamsPair in constructorParamsCollection)
            {
                var constructor = constructorParamsPair.Key;
                var parameters = constructorParamsPair.Value;

                var isInjectable = true;
                var parametersCount = parameters.Length;
                var constructorDependencies = new object[parametersCount];
                
                for (var i = 0; i < parametersCount; i++)
                {
                    var constructorDependency = await GetDependencyOfTypeAsync(parameters[i], availableDependencies,
                        sw, maxTaskFreezeMs, cancellationToken);
                    
                    constructorDependencies[i] = constructorDependency;

                    if (constructorDependency == null)
                    {
                        isInjectable = false;
                        break;
                    }
                }

                if (isInjectable)
                {
                    constructor.Invoke(instance, constructorDependencies);
                }
                
                await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            }
        }

        private static async Task<Dictionary<ConstructorInfo, Type[]>> GetInjectableConstructorsAsync(Type type,
            Stopwatch sw, int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            var constructors = type.GetConstructors(BindingFlag);
            var constructorParamsCollection = new Dictionary<ConstructorInfo, Type[]>();

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                var parametersCount = parameters.Length;

                if (parametersCount > 0)
                {
                    var parametersTypes = GetParametersTypes(parameters, parametersCount);
                    constructorParamsCollection.Add(constructor, parametersTypes);
                }

                await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            }

            return constructorParamsCollection;
        }

        private static async Task<object> GetDependencyOfTypeAsync(Type type, object[] dependencies, Stopwatch sw, 
            int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            foreach (var dependency in dependencies)
            {
                if (type == dependency.GetType())
                {
                    return dependency;
                }

                await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            }

            return null;
        }
        
        private static Type[] GetParametersTypes(ParameterInfo[] parameters, int parametersCount)
        {
            var parametersTypes = new Type[parametersCount];
            for (var i = 0; i < parametersCount; i++)
            {
                parametersTypes[i] = parameters[i].ParameterType;
            }

            return parametersTypes;
        }

        private static object GetDependencyOfTypeAsync(Type type, Dictionary<Type, object> localDependencies,
            Dictionary<Type, object> globalDependencies)
        {
            return GetDependencyInCollection(type, localDependencies) 
                   ?? GetDependencyInCollection(type, globalDependencies);
        }

        private static object GetDependencyInCollection(Type type, Dictionary<Type, object> collection)
        {
            if (collection.ContainsKey(type))
            {
                return collection[type];
            }

            return null;
        }
    }
}