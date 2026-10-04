using System.Collections.Generic;
using System.ComponentModel;

namespace Mo3RegUI.MvvmContract.Mvvm
{
    /// <summary>
    /// Minimal <see cref="INotifyPropertyChanged"/> base class for ViewModels.
    /// </summary>
    /// <remarks>
    /// Mo3RegUI is a single-file .NET Framework 4.0 application and must not take NuGet
    /// dependencies, so the MVVM primitives that a toolkit such as CommunityToolkit.Mvvm would
    /// normally provide are implemented here instead. <c>CallerMemberName</c> is deliberately not
    /// used because the attribute does not exist in .NET Framework 4.0; callers pass the property
    /// name explicitly through <c>nameof</c>.
    /// </remarks>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
