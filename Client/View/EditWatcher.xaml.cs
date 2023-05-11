using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Client.ViewModel;
using ConnectionInterface;

namespace Client.View
{
    /// <summary>
    /// Interaction logic for CreateWatcher.xaml
    /// </summary>
    public partial class EditWatcher : Window
    {
        public EditWatcher(int watcherId)
        {
            InitializeComponent();
            DataContext = new EditWatcherPresenter(watcherId);
        }

        private void CloseWindow(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
    }
}
