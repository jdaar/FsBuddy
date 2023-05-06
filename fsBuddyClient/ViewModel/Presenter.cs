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
using ConnectionInterface;

namespace Client.ViewModel
{
    public class Presenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ServiceConnection serviceConnection;

        public bool IsConnected {
            get {
                return serviceConnection.IsConnected;
            }
        }

        private List<Watcher> watchers;
        public List<Watcher> Watchers { 
            get
            {
                return watchers;
            }
            set 
            {
                watchers = value;
                OnPropertyChanged(nameof(Watchers));
            }
        }

        public ICommand RefreshCommand { get; set; }
        public ICommand SwitchConnectionStatusCommand { get; set; }
        public ICommand ShowCreateWatcherCommand { get; set; }
        public ICommand GetAllWatcherCommand { get; set; }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Presenter()
        { 
            serviceConnection = ServiceConnection.GetInstance(
                delegate()
                {
                    OnPropertyChanged(nameof(serviceConnection));
                    OnPropertyChanged(nameof(IsConnected));
                }
            );

            RefreshCommand = new RefreshCommand(serviceConnection);
            SwitchConnectionStatusCommand = new SwitchConnectionStatusCommand(serviceConnection);
            GetAllWatcherCommand = new GetAllWatcherCommand(serviceConnection, this);
            ShowCreateWatcherCommand = new ShowCreateWatcherCommand();
        }
    }
}
