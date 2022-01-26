using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Shuriken.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public string DisplayName { get; set; }

        /// <summary>
        /// FontAwesome icon code
        /// </summary>
        public string IconCode { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName, object before, object after)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
