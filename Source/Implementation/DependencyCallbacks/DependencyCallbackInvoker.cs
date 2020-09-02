using System;
using System.Collections.Generic;

namespace VoxCake.IoC
{
    internal class DependenciesCallbackInvoker
    {
        private readonly Dictionary<Type, Dependency> _dependencies;
        
        internal DependenciesCallbackInvoker(Dictionary<Type, Dependency> dependencies)
        {
            _dependencies = dependencies;
        }
        
        internal void Invoke(DependencyCallbackType dependencyCallback)
        {
            foreach (var pair in _dependencies)
            {
                var dependency = pair.Value;
                
                if(IsCallbackInvokable(dependency, dependencyCallback))
                {
                    InvokeCallbackOnDependency(dependency, dependencyCallback);
                }
            }
        }

        private bool IsCallbackInvokable(Dependency dependency, DependencyCallbackType dependencyCallback)
        {
            var isInvokable = false;
            
            switch (dependencyCallback)
            {
                case DependencyCallbackType.OnRegister:
                    isInvokable = dependency.isRegistrable;
                    break;
                case DependencyCallbackType.OnUnregister:
                    isInvokable = dependency.isUnregistrable;
                    break;
            }
            
            return isInvokable;
        }

        private void InvokeCallbackOnDependency(Dependency dependency, DependencyCallbackType dependencyCallback)
        {
            switch (dependencyCallback)
            {
                case DependencyCallbackType.OnRegister:
                    dependency.registrable.OnRegister();
                    break;
                case DependencyCallbackType.OnUnregister:
                    dependency.unregistrable.OnUnregister();
                    break;
            }
        }
    }
}