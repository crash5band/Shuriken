using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Shuriken.Commands
{
    public class RelayCommand : ICommand
    {
        private Action mExecute;
        private Func<bool> mCanExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action ex, Func<bool> canExec = null)
        {
            mExecute = ex;
            mCanExecute = canExec;
        }

        public bool CanExecute(object parameter)
        {
            return mExecute == null || mCanExecute == null ? true : mCanExecute();
        }

        public void Execute(object parameter)
        {
            mExecute();
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private Action<object> mExecute;
        private Func<bool> mCanExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> ex, Func<bool> canExec = null)
        {
            mExecute = ex;
            mCanExecute = canExec; 
        }

        public bool CanExecute(object parameter)
        {
            return mExecute == null || mCanExecute == null ? true : mCanExecute();
        }

        public void Execute(object parameter)
        {
            mExecute((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
