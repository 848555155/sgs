using System.Windows.Input;

namespace Sanguosha.UI.Controls;

public class SimpleRelayCommand : ViewModelBase, ICommand
{
    #region Fields

    public Action<object> Executor { get; }


    private bool _canExecute;

    public bool CanExecuteStatus
    {
        get => _canExecute;
        set
        {
            if (_canExecute == value) return;
            _canExecute = value;
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }

    #endregion // Fields

    #region Constructors

    public SimpleRelayCommand(Action<object> execute)
    {
        ArgumentNullException.ThrowIfNull(execute);

        Executor = execute;
    }

    #endregion // Constructors

    #region ICommand Members

    public virtual bool CanExecute(object parameter)
    {
        return CanExecuteStatus;
    }

    public event EventHandler CanExecuteChanged;

    public virtual void Execute(object parameter)
    {
        Executor(parameter);
    }

    #endregion // ICommand Members
}
