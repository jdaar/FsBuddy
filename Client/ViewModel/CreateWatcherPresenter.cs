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
    public class t_WatcherForm {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? InputPath { get; set; }
        public string? OutputPath { get; set; }
        public string? Filter { get; set; }
    }
    public class t_InputFile {
            public string? Name { get; set; }
            public bool? IsIncluded { get; set; }
    };

    public class CreateWatcherPresenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ServiceConnection serviceConnection;

        public ICommand CreateWatcherCommand { get; set; }

        public t_WatcherForm WatcherForm { get; set; } = new t_WatcherForm { 
            Name = "",
            InputPath = "",
            OutputPath = "",
            Filter = ""
        };

        public string WatcherName
        {
            get { return WatcherForm.Name ?? ""; }
            set { 
                WatcherForm.Name = value;
                OnPropertyChanged(nameof(WatcherName)); 
            }
        }
        public string WatcherFilter
        {
            get { return WatcherForm.Filter ?? ""; }
            set { 
                WatcherForm.Filter = value;
                OnPropertyChanged(nameof(WatcherFilter)); 
            }
        }

        public string WatcherInputPath 
        {
            get { return WatcherForm.InputPath ?? ""; }
            set { 
                WatcherForm.InputPath = value;
                OnPropertyChanged(nameof(WatcherInputPath)); 
            }
        }

        private List<t_InputFile> watcherInputPathItems;
        public List<t_InputFile> WatcherInputPathItems
        {
            get { return watcherInputPathItems; }
            set { 
                watcherInputPathItems = value;
                OnPropertyChanged(nameof(WatcherInputPathItems)); 
            }
        }

        public string WatcherOutputPath 
        {
            get { return WatcherForm.OutputPath ?? ""; }
            set { 
                WatcherForm.OutputPath = value;
                OnPropertyChanged(nameof(WatcherOutputPath)); 
            }
        }

        public void OnPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(WatcherInputPath) || propertyName == nameof(WatcherFilter))
            {
                List<t_InputFile> newInputPathItems = new();

                if (!Directory.Exists(WatcherInputPath))
                {
                    MessageBox.Show("Select a path that exists", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else {
                    string[] allFilePaths = Directory.GetFiles(
                                              WatcherInputPath,
                                              "*.*",
                                              SearchOption.TopDirectoryOnly);
                    string[] filteredFilePaths = Directory.GetFiles(
                                              WatcherInputPath,
                                              WatcherFilter,
                                              SearchOption.TopDirectoryOnly);

                    foreach (string file in allFilePaths)
                    {
                        if (filteredFilePaths.Contains(file))
                        {
                            newInputPathItems.Add(new t_InputFile
                            {
                                Name = file,
                                IsIncluded = true
                            });
                        } else
                        {
                            newInputPathItems.Add(new t_InputFile
                            {
                                Name = file,
                                IsIncluded = false
                            });
                        }
                    }

                    WatcherInputPathItems = newInputPathItems;
                } 
            }
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
