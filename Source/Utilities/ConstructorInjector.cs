using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoxCake.Common.CustomAwaiters;

namespace VoxCake.IoC.Utilities
{
    public static class ConstructorInjector
    {
        public static async Task InjectDependenciesToInstance(Dictionary<Type, Dependency> dependencies, object instance,
            TaskOptions taskOptions)
        {
            taskOptions.cancellationToken.ThrowIfCancellationRequested();
            
            var instanceType = instance.GetType();
            var constructor = new ReflectedConstructor(instanceType);
            var parameters = constructor.Parameters;
            
            var parametersCount = parameters.Length;
            var constructorDependencies = new object[parametersCount];
            
            await CustomAwaiter.ReduceCpuSleep(taskOptions);
                
            for (var i = 0; i < parametersCount; i++)
            {
                var parameterType = parameters[i];
                var constructorDependency = GetDependencyOfType(instanceType,
                    parameterType, dependencies);
                
                constructorDependencies[i] = constructorDependency;

                await CustomAwaiter.ReduceCpuSleep(taskOptions);
            }
                
            constructor.Invoke(instance, constructorDependencies);
            
            await CustomAwaiter.ReduceCpuSleep(taskOptions);
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