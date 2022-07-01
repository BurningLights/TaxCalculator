using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TaxCalculator.ViewModels
{
    internal class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetPropertyValue<T>(ref T member, T newValue, [CallerMemberName] string propertyName = "")
        {
            if (Equals(member, newValue))
            {
                return false;
            }

            member = newValue;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}
