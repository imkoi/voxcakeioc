namespace VoxCake.IoC
{
    public interface IDirectBinding
    {
        IDirectBinding And<T>();
        IDirectBinding And(object instance);

        IEndBinding To<T>();
        IEndBinding To(object instance);
    }
}