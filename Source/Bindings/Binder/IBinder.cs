namespace VoxCake.IoC
{
    public interface IBinder
    {
        IBinding Bind<T>();
        IBinding Bind(object instance);
    }
}