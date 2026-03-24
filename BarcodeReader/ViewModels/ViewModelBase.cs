using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeReader.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {

        protected virtual bool SetProperty<T>(ref T member, T val,
                [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(member, val)) return false;

            member = val;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public virtual void Dispose() { }

    }
}
