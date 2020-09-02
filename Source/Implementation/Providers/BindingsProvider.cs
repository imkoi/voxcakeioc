using System;
using System.Collections;
using System.Collections.Generic;
using VoxCake.IoC;

namespace VoxCake.IoCRefactored
{
    internal class BindingsProvider
    {
        private readonly Dictionary<Type, object> _bindings;
        
        internal BindingsProvider(Dictionary<Type, object> bindings)
        {
            _bindings = bindings;
        }

        public object TryGetBindingsForType(Type type)
        {
            if (_bindings.ContainsKey(type))
            {
                return _bindings[type];
            }

            return null;
        }
    }
}