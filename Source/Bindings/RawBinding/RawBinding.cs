using System;
using System.Collections.Generic;
using VoxCake.IoC.Bindings;
using VoxCake.IoC.Types;

namespace VoxCake.IoC
{
    internal class RawBinding : BaseBinding, IRawBinding
    {
        private readonly object _dependency;
        private readonly Type _dependencyKey;
        private readonly BindingType _bindingType;
        private readonly List<Dependency> _directBindings;
        
        internal RawBinding(Dictionary<Type, Dependency> localDependencies, Dictionary<Type, Dependency> globalDependencies,
            Dictionary<Type, List<Dependency>> directDependencies, object dependency, Type dependencyKey,
            BindingType bindingType, List<Dependency> directBindings) 
            : base(localDependencies, globalDependencies, directDependencies)
        {
            _dependency = dependency;
            _dependencyKey = dependencyKey;
            _bindingType = bindingType;
            _directBindings = directBindings;
        }

        public IRawBinding Raw<T>()
        {
            return base.Raw<T>(_dependency, _dependencyKey, _bindingType, _directBindings);
        }

        public IRawBinding Raw(object instance)
        {
            return base.Raw(instance, _dependency, _dependencyKey, _bindingType, _directBindings);
        }

        public IEndBinding To<T>()
        {
            
            return base.To<T>(_dependencyKey, _bindingType, _directBindings);
        }

        public IEndBinding To(object instance)
        {
            return base.To(instance, _dependencyKey, _bindingType, _directBindings);
        }
    }
}