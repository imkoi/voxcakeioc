namespace VoxCake.IoC
{
    public interface IBinding
    {
        IEndBinding As<T>();
        IEndBinding As(object instance);
        
        IDirectBinding And<T>();
        IDirectBinding And(object instance);
        
        IEndBinding To<T>();
        IEndBinding To(object instance);
        
        void ToGlobalContainer();
    }
}