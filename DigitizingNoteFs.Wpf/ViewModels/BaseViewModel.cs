﻿using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DigitizingNoteFs.Wpf.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class RelayCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        public RelayCommand(Predicate<T> canExecute, Action<T> execute)
        {
            _canExecute = canExecute;
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public bool CanExecute(object? parameter)
        {
            ArgumentNullException.ThrowIfNull(parameter);
            try
            {
                return _canExecute == null || _canExecute((T)parameter);
            }
            catch
            {
                return true;
            }
        }

        public void Execute(object? parameter)
        {
            ArgumentNullException.ThrowIfNull(parameter);
            _execute(obj: (T)parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}