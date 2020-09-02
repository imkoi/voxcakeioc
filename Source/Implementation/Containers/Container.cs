using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoxCake.Common.CustomAwaiters;

namespace VoxCake.IoC
{
    public class Container : IContainer
    {
        public event Action<IBindingsBuilder> Bind;

        private Dictionary<Type, Dependency> _dependencies;
        private bool _isResolved;
        
        private CancellationTokenSource _cancellationTokenSource;

        public Container()
        {
            _dependencies = new Dictionary<Type, Dependency>();
        }
        
        public async Task ResolveDependencies(TaskOptions taskOptions)
        {
            try
            {
                if (!_isResolved)
                {
                    _cancellationTokenSource = taskOptions.cancellationTokenSource;
                    taskOptions.cancellationToken.ThrowIfCancellationRequested();

                    await GetBindings(taskOptions);

                    _isResolved = true;
                }
                else
                {
                    throw new Exception("Container was already resolved!");
                }
            }
            catch(Exception exception)
            {
                throw new Exception("Task ResolveDependencies was cancelled! \n" +
                                    $"{exception}");
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _dependencies.Clear();
        }

        private async Task GetBindings(TaskOptions taskOptions)
        {
            taskOptions.cancellationToken.ThrowIfCancellationRequested();
            
            var dependencyBinder = new BindingsBuilder(taskOptions.cancellationToken);
            Bind?.Invoke(dependencyBinder);

            var bindings = await dependencyBinder.GetBindings(taskOptions);
            
        }
    }
}