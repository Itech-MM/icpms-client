namespace icpms_client.Common.Command;

using System;
using System.Windows.Input;

public class CommonRelayCommand<T>(Action<T> execute, Predicate<T>? canExecute = null) : ICommand
{
    private readonly Action<T> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

    public bool CanExecute(object? parameter)
    {
        return canExecute == null || parameter is T parameter1 && canExecute(parameter1);
    }

    public void Execute(object? parameter)
    {
        if (parameter is T param)
        {
            _execute(param);
        }
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
