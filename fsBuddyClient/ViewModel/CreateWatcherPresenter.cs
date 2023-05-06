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
    public class WatcherForm {
        public string Name { get; set; }
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public string Filter { get; set; }
    }

    public class CreateWatcherPresenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ServiceConnection serviceConnection;

        public ICommand CreateWatcherCommand { get; set; }

        public WatcherForm WatcherForm { get; set; } = new WatcherForm { 
            Name = "",
            InputPath = "",
            OutputPath = "",
            Filter = ""
        };

        public string WatcherName
        {
            get { return WatcherForm.Name; }
            set { 
                WatcherForm.Name = value;
                OnPropertyChanged(nameof(WatcherName)); 
            }
        }
        public string WatcherFilter
        {
            get { return WatcherForm.Filter; }
            set { 
                WatcherForm.Filter = value;
                OnPropertyChanged(nameof(WatcherFilter)); 
            }
        }

        public string WatcherInputPath 
        {
            get { return WatcherForm.InputPath; }
            set { 
                WatcherForm.InputPath = value;
                OnPropertyChanged(nameof(WatcherInputPath)); 
            }
        }

        public string WatcherOutputPath 
        {
            get { return WatcherForm.OutputPath; }
            set { 
                WatcherForm.OutputPath = value;
                OnPropertyChanged(nameof(WatcherOutputPath)); 
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
