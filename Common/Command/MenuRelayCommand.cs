using System;
using System.Windows.Input;

namespace icpms_client.Common.Command;

public class MenuRelayCommand(Action<object> execute, Func<object, bool>? canExecute = null) : ICommand
{
    private readonly Action<object> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

    public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter??true) ?? true;

    public void Execute(object? parameter)
    {
        _execute(parameter ?? new object());
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}