using System.ComponentModel;

namespace FileFinder.ViewModel
{
    public abstract class  BaseViewModel : INotifyPropertyChanged
    {
        protected  BaseViewModel()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (null != this.PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
