namespace VoxCake.IoC
{
    public interface IRemovableDependency
    {
        /// <summary>
        /// Invoked on disposing of container or removing instance from container
        /// </summary>
        void OnRemove();
    }
}