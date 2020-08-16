using System;
using System.Collections.Generic;
using VoxCake.IoC.Types;

namespace VoxCake.IoC.Bindings
{
    internal class Binding : BaseBinding, IBinding
    {
        private readonly object _dependency;
        private readonly Type _dependencyKey;
        private readonly BindingType _bindingType;
        private List<object> _directBindings;

        internal Binding(Dictionary<Type, object> localDependencies, Dictionary<Type, object> globalDependencies,
            Dictionary<Type, List<object>> directDependencies, object dependency, Type dependencyKey,
            BindingType bindingType) 
            : base(localDependencies, globalDependencies, directDependencies)
        {
            _dependency = dependency;
            _dependencyKey = dependencyKey;
            _bindingType = bindingType;
        }

        IEndBinding IBinding.As<T>()
        {
            return base.As<T>(_dependencyKey);
        }
        
        IEndBinding IBinding.As(object instance)
        {
            return base.As(instance, _dependencyKey);
        }
        
        IDirectBinding IBinding.And<T>()
        {
            AllocateDirectBindings();
            return base.And<T>(_dependency, _dependencyKey, _bindingType, _directBindings);
        }
        
        IDirectBinding IBinding.And(object instance)
        {
            AllocateDirectBindings();
            return base.And(instance, _dependency, _dependencyKey, _bindingType, _directBindings);
        }

        IEndBinding IBinding.To<T>()
        {
            AllocateDirectBindings();
            return base.To<T>(_dependencyKey, _bindingType, _directBindings);
        }

        IEndBinding IBinding.To(object instance)
        {
            AllocateDirectBindings();
            return base.To(instance, _dependencyKey, _bindingType, _directBindings);
        }

        void IBinding.ToGlobalContainer()
        {
            base.ToGlobalContainer(_dependency, _dependencyKey);
        }

        private void AllocateDirectBindings()
        {
            _directBindings = new List<object>();
            if (_bindingType == BindingType.DependencyBinding)
            {
                RemoveDependencyFromLocalContainer(_dependencyKey);
                _directBindings.Add(_dependency);
            }
        }
    }
}