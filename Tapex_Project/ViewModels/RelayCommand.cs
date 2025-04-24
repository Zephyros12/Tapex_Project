using System;
using System.Windows.Input;

namespace Tapex_Project.ViewModels
{
    /// <summary>
    /// ICommand 구현체: MVVM에서 명령 바인딩용
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged;

        public void NotifyCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}