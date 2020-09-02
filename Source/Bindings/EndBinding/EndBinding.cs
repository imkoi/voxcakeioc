using System;
using System.Collections.Generic;

namespace VoxCake.IoC.Bindings
{
    internal class EndBinding : BaseBinding, IEndBinding
    {
        private readonly Dependency _dependency;
        private readonly Type _dependencyKey;

        internal EndBinding(Dictionary<Type, Dependency> localDependencies, Dictionary<Type, Dependency> globalDependencies,
            Dependency dependency, Type dependencyKey)
            : base(localDependencies, globalDependencies, null)
        {
            _dependency = dependency;
            _dependencyKey = dependencyKey;
        }
        
        void IEndBinding.ToGlobalContainer()
        {
            base.ToGlobalContainer(_dependencyKey, _dependency);
        }
    }
}