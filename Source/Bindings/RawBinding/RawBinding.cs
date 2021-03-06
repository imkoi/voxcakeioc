﻿using System;
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
        private readonly List<object> _directBindings;
        
        internal RawBinding(Dictionary<Type, object> localDependencies, Dictionary<Type, object> globalDependencies,
            Dictionary<Type, List<object>> directDependencies, object dependency, Type dependencyKey,
            BindingType bindingType, List<object> directBindings) 
            : base(localDependencies, globalDependencies, directDependencies)
        {
            _dependency = dependency;
            _dependencyKey = dependencyKey;
            _bindingType = bindingType;
            _directBindings = directBindings;
        }

        public IRawBinding Raw<T>()
        {
            return base.And<T>(_dependency, _dependencyKey, _bindingType, _directBindings);
        }

        public IRawBinding Raw(object instance)
        {
            return base.And(instance, _dependency, _dependencyKey, _bindingType, _directBindings);
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