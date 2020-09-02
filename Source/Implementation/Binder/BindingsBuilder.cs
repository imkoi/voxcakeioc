using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VoxCake.Common.CustomAwaiters;

namespace VoxCake.IoC
{
    internal class BindingsBuilder : IBindingsBuilder
    {
        private readonly Dictionary<Type, object> _bindings;
        private readonly TaskCompletionSource<bool> _completionSource;
        
        private bool _isBindingCompleted;
        
        internal BindingsBuilder(CancellationToken cancellationToken)
        {
            _bindings = new Dictionary<Type, object>();
            _completionSource = new TaskCompletionSource<bool>(cancellationToken);
        }

        public IBinding<TDependency> Bind<TDependency>()
        {
            if (!_isBindingCompleted)
            {
                return new Binding<TDependency>();
            }
            
            throw new Exception("Cannot bind dependency because binding process is completed!");
        }

        public void Complete()
        {
            if (!_isBindingCompleted)
            {
                _completionSource.SetResult(true);
                _isBindingCompleted = true;
            }
            else
            {
                throw new Exception("Cannot complete bindings because its already completed!");
            }
        }
        
        internal async Task<Dictionary<Type, object>> GetBindings(TaskOptions taskOptions)
        {
            taskOptions.cancellationToken.ThrowIfCancellationRequested();
            
            await _completionSource.Task;

            return _bindings;
        }
    }
}