namespace VoxCake.IoC
{
    public interface IRegistrableDependency
    {
        /// <summary>
        /// Invoked after all injections
        /// </summary>
        void OnRegister();
    }
}