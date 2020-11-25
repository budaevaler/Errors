using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Errors.Core
{
    public class RelayCommand : ICommand
    {
        private Action<object> _execute;
        private Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public bool CanExecute(object parameter) { return this._canExecute == null || this._canExecute(parameter); }

        public void Execute(object parameter)
        {
            try { this._execute(parameter); }
            catch (Exception e)
            {
                //ExceptionMethods.ThrowExceptionJson(e, "Test");
                var nl = Environment.NewLine;
                MessageBox.Show($"An error was thrown!{nl}{nl}" +
                                $"Error message:{nl}{e.Message}");
            }
        }
    }
}
