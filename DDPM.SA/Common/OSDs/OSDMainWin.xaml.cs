using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;

namespace DDPM.OSDs
{
    /// <summary>
    /// Interaction logic for OSDMainWin.xaml
    /// </summary>
    public partial class OSDMainWin : Window
    {
        ObservableCollection<OSDWinInfo> _osdWins = new ObservableCollection<OSDWinInfo>();

        public ObservableCollection<OSDWinInfo> OSDWins
        {
            get { return _osdWins; }
        }

        RelayCommand<string>? _osdWinClosed;
        public ICommand OSDWinClosed
        {
            get
            {
                if (null == _osdWinClosed)
                {
                    _osdWinClosed = new RelayCommand<string>(OnOSDWinClosed);
                }

                return _osdWinClosed;
            }
        }

        private void OnOSDWinClosed(string? guid)
        {
            OSDWinInfo? target = OSDWins.FirstOrDefault(x => x.GUID.Equals(guid));
            if (null != target)
                OSDWins.Remove(target);
        }

        public OSDMainWin()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }

        public void ShowWindow()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(ShowWindow);
                return;
            }
            Show();
        }

        public void AddShowOSDWinInfo(OSDWinInfo oSDWinInfo)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => OSDWins.Add(oSDWinInfo));
                return;
            }
        }
    }
}
