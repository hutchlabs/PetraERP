// FROM: http://stackoverflow.com/questions/6205472/mvvm-passing-eventargs-as-command-parameter

using System;
using System.Windows.Input;

namespace PetraERP.Shared.UI
{
    public class ActionCommand<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private Action<T> _action;

        public ActionCommand(Action<T> action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter) { return true; }

        public void Execute(object parameter)
        {
            if (_action != null)
            {
                var castParameter = (T)Convert.ChangeType(parameter, typeof(T));
                _action(castParameter);
            }
        }
    }
}
