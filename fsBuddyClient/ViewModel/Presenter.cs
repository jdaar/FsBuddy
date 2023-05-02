using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Model;
using System.Windows.Input;

namespace Client.ViewModel
{
    public class Presenter : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        public ServiceConnection serviceConnection = new ServiceConnection();

        public ICommand RefreshCommand { get; set; }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Presenter()
        {
            RefreshCommand = new RefreshCommand(serviceConnection);
        }
    }
}
