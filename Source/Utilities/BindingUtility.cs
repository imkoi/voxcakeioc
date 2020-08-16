namespace VoxCake.IoC.Utilities
{
    internal static class BindingUtility
    {
        internal static void InjectDependenciesToInstance(object[] dependencies, object instance)
        {
            var instanceType = instance.GetType();

            var constructorInfo = ConstructorInjector.GetConstructorWithDependencies(instanceType, dependencies);
            constructorInfo?.Invoke(instance, dependencies);
        }
    }
}