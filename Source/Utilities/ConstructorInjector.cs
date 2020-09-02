using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VoxCake.Common.Utilities;

namespace VoxCake.IoC.Utilities
{
    public static class ConstructorInjector
    {
        public static async Task InjectDependenciesToInstanceAsync(Dictionary<Type, Dependency> dependencies, object instance,
            Stopwatch sw, int maxTaskFreezeMs, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var instanceType = instance.GetType();
            var constructor = new ReflectedConstructor(instanceType);
            var parameters = constructor.Parameters;
            
            var parametersCount = parameters.Length;
            var constructorDependencies = new object[parametersCount];
            
            await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
                
            for (var i = 0; i < parametersCount; i++)
            {
                var parameterType = parameters[i];
                var constructorDependency = GetDependencyOfType(instanceType,
                    parameterType, dependencies);
                
                constructorDependencies[i] = constructorDependency;

                await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
            }
                
            constructor.Invoke(instance, constructorDependencies);
            
            await Awaiter.ReduceTaskFreezeAsync(sw, maxTaskFreezeMs, cancellationToken);
        }

        private static object GetDependencyOfType(Type instanceType, Type dependencyType,
            Dictionary<Type, Dependency> collection)
        {
            if (collection.ContainsKey(dependencyType))
            {
                return collection[dependencyType];
            }
            
            throw new Exception($"Dependency \"{dependencyType.Name}\" for instance of {instanceType.Name}" +
                                " not found in container!");
        }
    }
}