using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VoxCake.IoC.Utilities
{
    internal static class ConstructorInjector
    {
        private const BindingFlags BindingFlag = BindingFlags.FlattenHierarchy 
                                         | BindingFlags.Public 
                                         | BindingFlags.Instance 
                                         | BindingFlags.InvokeMethod;
        
        internal static void InjectDependenciesToInstance(Dictionary<Type, object> localDependencies,
            Dictionary<Type, object> globalDependencies, object instance)
        {
            var instanceType = instance.GetType();
            var constructorParamsCollection = GetInjectableConstructors(instanceType);

            foreach (var constructorParamsPair in constructorParamsCollection)
            {
                var constructor = constructorParamsPair.Key;
                var parameters = constructorParamsPair.Value;

                var isInjectable = true;
                var parametersCount = parameters.Length;
                var constructorDependencies = new object[parametersCount];
                
                for (var i = 0; i < parametersCount; i++)
                {
                    var constructorDependency = GetDependencyOfType(parameters[i], localDependencies,
                        globalDependencies);
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
            }
        }

        internal static ConstructorInfo GetConstructorWithDependencies(Type type, object[] dependencies)
        {
            var constructors = type.GetConstructors(BindingFlag);
            var constructorsCount = constructors.Length;
            ConstructorInfo targetConstructor = null;

            if (constructorsCount > 0)
            {
                var targetParametersCount = dependencies.Length;

                foreach (var suspectConstructor in constructors)
                {
                    var suspectParameters = suspectConstructor.GetParameters();
                    var suspectParametersCount = suspectParameters.Length;

                    if (suspectParametersCount == targetParametersCount)
                    {
                        var isTargetConstructor = true;
                        var suspectParametersTypes = GetParametersTypes(suspectParameters, suspectParametersCount);

                        foreach (var dependency in dependencies)
                        {
                            if (!suspectParametersTypes.Contains(dependency.GetType()))
                            {
                                isTargetConstructor = false;
                                break;
                            }
                        }

                        if (isTargetConstructor)
                        {
                            targetConstructor = suspectConstructor;
                        }
                    }
                }
            }

            return targetConstructor;
        }

        private static Dictionary<ConstructorInfo, Type[]> GetInjectableConstructors(Type type)
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
            }

            return constructorParamsCollection;
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

        private static object GetDependencyOfType(Type type, Dictionary<Type, object> localDependencies,
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