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
    public class IWatcherForm {
        public string Name { get; set; }
    }

    public class CreateWatcherPresenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ServiceConnection serviceConnection;

        public ICommand CreateWatcherCommand { get; set; }

        public IWatcherForm WatcherForm { get; set; } = new IWatcherForm { 
            Name = ""
        };

        private string _watcherName = "";
        public string WatcherName
        {
            get { return _watcherName; }
            set { 
                _watcherName = value;
                WatcherForm.Name = value;
                OnPropertyChanged(nameof(WatcherName)); 
            }
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public CreateWatcherPresenter()
        { 
            serviceConnection = ServiceConnection.GetInstance(
                delegate()
                {
                    OnPropertyChanged(nameof(serviceConnection));
                }
            );

            CreateWatcherCommand = new CreateWatcherCommand(serviceConnection);
        }
    }
}
