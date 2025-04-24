using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using Dell.TechHub.Sdk.Common.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
            {
                OSDWins.Remove(target);
                if (target.OSDType_Op.Equals(OSDType_Op.CloseAll))
                {
                    _ = Task.Run(async () =>
                     {
                         await Task.Delay(1000);
                         foreach (OSDWinInfo info in OSDWins)
                         {
                             switch (info.OSDType_Device)
                             {
                                 case OSDType_Device.CapsLockOn:
                                     break;
                                 case OSDType_Device.CapsLockOff:
                                     break;
                                 case OSDType_Device.ScrollLockOn:
                                     break;
                                 case OSDType_Device.ScrollLockOff:
                                     break;
                                 case OSDType_Device.NumLockOn:
                                     break;
                                 case OSDType_Device.NumLockOff:
                                     break;
                                 default:
                                     info.IsFadeOut = true;
                                     break;
                             }
                         }
                     });
                }
            }
            if (OSDWins.Count == 0)
            {
                CloseWindow();
            }
        }

        public OSDMainWin()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this.WindowState = WindowState.Maximized;
            /*this.Left = 1;
            this.Top = 1;
            this.Width = Screen.PrimaryScreen!.WorkingArea.Width;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;*/
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
        public void adjustOSDWin((double, double, double, double) args)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() =>
                {
                    this.Top = args.Item1 + 1;
                    this.Left = args.Item2 + 1;
                    this.Width = args.Item3 - 2;
                    this.Height = args.Item4 - 2;
                });
                return;
            }
            this.Top = args.Item1 + 1;
            this.Left = args.Item2 + 1;
            this.Width = args.Item3 - 2;
            this.Height = args.Item4 - 2;

        }
        public void AddShowOSDWinInfo(OSDWinInfo oSDWinInfo)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => OSDWins.Add(oSDWinInfo));
                return;
            }
            OSDWins.Add(oSDWinInfo);
        }

        public void RemoveShowOSDWinInfo(OSDWinInfo oSDWinInfo)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => OSDWins.Remove(oSDWinInfo));
                return;
            }
            OSDWins.Remove(oSDWinInfo);
        }

        public void CloseWindow()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(CloseWindow);
                return;
            }
            Close();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }
    }
}
