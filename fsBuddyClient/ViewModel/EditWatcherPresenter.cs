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
    public class EditWatcherPresenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ServiceConnection serviceConnection;

        public ICommand EditWatcherCommand { get; set; }

        public int WatcherId { get; set; }

        public t_WatcherForm watcherForm = new t_WatcherForm { 
            Name = "",
            InputPath = "",
            OutputPath = "",
            Filter = ""
        };
        public t_WatcherForm WatcherForm { 
            get { return watcherForm; } 
            set {
                watcherForm = value;
                OnPropertyChanged(nameof(WatcherForm));
                OnPropertyChanged(nameof(WatcherName)); 
                OnPropertyChanged(nameof(WatcherFilter)); 
                OnPropertyChanged(nameof(WatcherInputPath)); 
                OnPropertyChanged(nameof(WatcherOutputPath)); 
                OnPropertyChanged(nameof(WatcherInputPathItems)); 
            } 
        }

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

        private async Task PopulateWatcherForm()
        {
            var request = new PipeRequest
            {
                Command = t_PipeCommand.GET_WATCHER,
                Payload = new PipeRequestPayload
                {
                    WatcherId = WatcherId
                }
            };

            var response = await serviceConnection.SendPipeRequest(request);

            var watcher = response?.Payload?.Watchers?.First();

            if (watcher == null)
            {
                return;
            }

            WatcherForm = new t_WatcherForm
            {
                Name = watcher.Name,
                InputPath = watcher.InputPath,
                OutputPath = watcher.OutputPath,
                Filter = watcher.SearchPattern
            };
        }

        public EditWatcherPresenter(int watcherId)
        { 
            serviceConnection = ServiceConnection.GetInstance(
                delegate()
                {
                    OnPropertyChanged(nameof(serviceConnection));
                }
            );

            WatcherId = watcherId;
            EditWatcherCommand = new CreateWatcherCommand(serviceConnection);
            Task.Run(PopulateWatcherForm);
        }
    }
}
