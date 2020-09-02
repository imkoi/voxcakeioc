using System;
using System.Reflection;

namespace VoxCake.IoC.Utilities
{
    internal class ReflectedConstructor
    {
        private const BindingFlags BindingFlag = BindingFlags.FlattenHierarchy 
                                                 | BindingFlags.Public 
                                                 | BindingFlags.Instance 
                                                 | BindingFlags.InvokeMethod;
        
        public Type[] Parameters => _parametersTypes;
        
        private readonly ConstructorInfo _constructorInfo;
        private readonly Type[] _parametersTypes;
        
        internal ReflectedConstructor(Type type)
        {
            _constructorInfo = GetConstructor(type);
            _parametersTypes = GetParameters(_constructorInfo);
        }

        internal void Invoke(object instance, params object[] parameters)
        {
            _constructorInfo.Invoke(instance, parameters);
        }

        private ConstructorInfo GetConstructor(Type type)
        {
            var constructors = type.GetConstructors(BindingFlag);

            if (constructors.Length > 0)
            {
                return constructors[0];
            }
            
            throw new Exception($"Class {type.Name} has more than one constructor!");
        }

        private Type[] GetParameters(ConstructorInfo constructorInfo)
        {
            var parameters = constructorInfo.GetParameters();
            var parametersCount = parameters.Length;
            
            return GetParametersTypes(parameters, parametersCount);
        }
        
        private Type[] GetParametersTypes(ParameterInfo[] parameters, int parametersCount)
        {
            var parametersTypes = new Type[parametersCount];
            for (var i = 0; i < parametersCount; i++)
            {
                parametersTypes[i] = parameters[i].ParameterType;
            }

            return parametersTypes;
        }
    }
}