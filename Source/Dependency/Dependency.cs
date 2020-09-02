using System;

namespace VoxCake.IoC
{
    public class Dependency
    {
        public object Key => _key;
        public object Value => _value;
        public bool IsDirect => _isDirect;
        public bool IsInjectable => _isInjectable;

        private readonly object _key;
        private readonly object _value;

        private readonly bool _isDirect;
        private readonly bool _isInjectable;

        public Dependency(Type key, object value, bool isInjectable, bool isDirect)
        {
            _key = key;
            _value = value;
            _isDirect = isDirect;
            _isInjectable = isInjectable;
        }
    }
}