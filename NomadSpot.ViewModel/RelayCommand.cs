using System;
using System.Windows.Input;

namespace NomadSpot.ViewModel
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Action<object> _executeWithParam;
        private readonly Func<bool> _canExecute;
        private EventHandler _canExecuteChanged;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute    = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public RelayCommand(Action<object> execute, Func<bool> canExecute = null)
        {
            _executeWithParam = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute       = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add    => _canExecuteChanged += value;
            remove => _canExecuteChanged -= value;
        }

        public void RaiseCanExecuteChanged() =>
            _canExecuteChanged?.Invoke(this, EventArgs.Empty);

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter)
        {
            _execute?.Invoke();
            _executeWithParam?.Invoke(parameter);
        }
    }
}
