using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Model;
using System.Windows.Input;
using System.Security.Policy;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections;
using Microsoft.Windows.Themes;
using System.Windows;

namespace Client.ViewModel
{
    public class Presenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ServiceConnection serviceConnection = new ServiceConnection();

        public ICommand RefreshCommand { get; set; }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Presenter()
        {
            RefreshCommand = new RefreshCommand(serviceConnection);
        }
    }
}
