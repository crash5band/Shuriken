using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Shuriken.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public string DisplayName { get; set; }

        /// <summary>
        /// FontAwesome icon codes
        /// </summary>
        public string IconCode { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
