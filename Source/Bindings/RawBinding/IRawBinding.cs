namespace VoxCake.IoC
{
    public interface IRawBinding
    {
        IRawBinding Raw<T>();
        IRawBinding Raw(object instance);

        IEndBinding To<T>();
        IEndBinding To(object instance);
    }
}