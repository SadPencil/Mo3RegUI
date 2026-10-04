using System;
using System.Windows.Input;

namespace Mo3RegUI.MVVMContract.Mvvm
{
    /// <summary>
    /// An <see cref="ICommand"/> whose <see cref="CanExecuteChanged"/> can be raised by the
    /// ViewModel. Commands are the only way the View is allowed to ask the ViewModel to do
    /// something.
    /// </summary>
    public interface IRelayCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Parameterless command implementation. There is no generic variant because the application
    /// has no command that needs a parameter.
    /// </summary>
    public class RelayCommand : IRelayCommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => this.canExecute?.Invoke() ?? true;

        public void Execute(object parameter) => this.execute();

        public void RaiseCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
