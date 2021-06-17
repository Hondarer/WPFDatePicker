using System;
using System.Windows.Input;
using WPFDatePicker.ViewModels;

namespace WPFDatePicker.Commands
{
    public class DelegateCommand : BindableBase, ICommand
    {
        private readonly Action<object> _execute;

        private readonly Func<object, bool> _canExecute;

        private readonly Func<object, object> _indexer;

        public DelegateCommand(Action<object> execute) : this(execute, o => true)
        {
        }

        public DelegateCommand(Action<object> execute, Func<object, object> indexer) : this(execute, o => true, indexer)
        {
        }

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute, Func<object, object> indexer)
        {
            _execute = execute;
            _canExecute = canExecute;
            _indexer = indexer;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return false;
            }

            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (_execute == null)
            {
                return;
            }

            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public static void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public object this[object index]
        {
            get
            {
                if (_indexer == null)
                {
                    return null;
                }

                return _indexer(index);
            }
        }
    }
}
