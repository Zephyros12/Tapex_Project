using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tapex_Project.ViewModels
{
    /// <summary>
    /// INotifyPropertyChanged 기본 구현
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}