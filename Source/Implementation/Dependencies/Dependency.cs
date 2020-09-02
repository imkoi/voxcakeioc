using System;

namespace VoxCake.IoC
{
    public class Dependency
    {
        public Type key;
        public object value;
        public bool isRegistrable;
        public bool isUnregistrable;

        public IRegistrableDependency registrable;
        public IUnregistrableDependency unregistrable;
    }
}