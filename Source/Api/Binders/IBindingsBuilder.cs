namespace VoxCake.IoC
{
    public interface IBindingsBuilder
    {
        IBinding<TDependency> Bind<TDependency>();
        void Complete();
    }
}