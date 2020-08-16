using System;
using System.Collections.Generic;
using VoxCake.IoC.Types;

namespace VoxCake.IoC.Bindings
{
    internal class EndBinding : BaseBinding, IEndBinding
    {
        private readonly object _dependency;
        private readonly Type _dependencyKey;

        internal EndBinding(Dictionary<Type, object> localDependencies, Dictionary<Type, object> globalDependencies,
            object dependency, Type dependencyKey)
            : base(localDependencies, globalDependencies, null)
        {
            _dependency = dependency;
            _dependencyKey = dependencyKey;
        }
        
        void IEndBinding.ToGlobalContainer()
        {
            base.ToGlobalContainer(_dependency, _dependencyKey);
        }
    }
}