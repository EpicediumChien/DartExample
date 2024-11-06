using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.DdpmHomePlugin.ViewModels;
using Dell.Client.Framework.UX.WPF;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using VcpCore.Common;
using IDdpmHomePageViewModel = DDPM.UI.Plugin.DdpmHomePlugin.Interfaces.IDdpmHomePageViewModel;

namespace DDPM.UI.Plugin.DdpmHomePlugin
{
    /// <summary>
    /// Interaction logic for DdpmHomePage.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class DdpmHomePage : System.Windows.Controls.UserControl
    {
        private DdpmHomePageViewModel? _ddpmHomePageViewModel;

        public DdpmHomePage()
        {
            InitializeComponent();

            _ddpmHomePageViewModel = (DdpmHomePageViewModel?)(DdpmHomePlugin.PluginIoc?.GetService<IDdpmHomePageViewModel>());

            if (_ddpmHomePageViewModel != null)
            {
                base.DataContext = _ddpmHomePageViewModel;
                _ddpmHomePageViewModel.HomeDevicesChanged += _ddpmHomePageViewModel_HomeDevicesChanged;

                ////Robert_Lin, 2024-7-16 for engineer debug,
                //if (IniReadInt("DDPMDebug", "HomePage.ShowDeviceListViewToolbar", 0, @"C:\temp\DDPMDebug.txt") == 1)
                //{
                //    debugRwdParams.Visibility = Visibility.Visible;
                //}
                _ddpmHomePageViewModel.ShowConsentRequested += _ddpmHomePageViewModel_ShowConsent;
                _ddpmHomePageViewModel.ImportNotify += ImportNotifyEventHandler;
            }
        }

        private void ImportNotifyEventHandler(object sender, MonitorInfo mo)
        {
            Dispatcher.Invoke(() =>
            {
                Window parentWindow = Window.GetWindow(this);
                double windowLeft = 0;
                double windowTop = 0;
                ImportModalDialog modalDialog = new(mo.modelName, parentWindow.ActualWidth, parentWindow.ActualHeight - 40);
                if (parentWindow != null)
                {
                    modalDialog.Owner = parentWindow;
                    windowLeft = parentWindow.Left;
                    windowTop = parentWindow.Top + 40;
                }
                modalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                modalDialog.Left = windowLeft;
                modalDialog.Top = windowTop;
                modalDialog.ShowDialog();

                string localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dell");
                string path = localAppDataPath + "\\Dell Display and Peripheral Manager\\Export";
                string model = mo.modelName;//"U2724DE";
                string serviceTag = mo.edid.ServiceTag;
                string exportpath = path + "\\" + model + "_" + serviceTag + ".json";

                if (modalDialog.DialogResult != null && modalDialog.DialogResult == true)
                {
                    //For jason to do import
                    if (DdpmCommonHelper.DeviceManagerSA.DisplayImportSettings(mo, false, exportpath).Result)
                    {
                        //ignore next check for this model
                        if (modalDialog.isChecked)
                        {
                            //DdpmCommonHelper.DeviceManagerSA
                        }
                    }
                }
            });
        }

        //Unused
        private void deviceCollectionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            e.Handled = true;
            if (e.AddedItems == null)
                return;
            if (e.AddedItems.Count <= 0)
            {
                return;
            }

            ILog? log = DdpmHomePlugin.PluginIoc.GetService<ILog>();
            log?.Info("DdpmHomePage.DeviceSelectionChanged");

            if (e.AddedItems.Count <= 0)
                return;

            DeviceInfo? selDev = e.AddedItems[0] as DeviceInfo;
            if (selDev == null)
                return;

            DeviceInfo? selDev2 = _ddpmHomePageViewModel?.SelDevice;

            if (selDev == selDev2)
                return;

            //IPluginManager? pluginManager = DdpmHomePlugin.PluginIoc.GetService<IPluginManager>();
            //DdpmHomePlugin? homePlugin = pluginManager?.FindPluginByType<DdpmHomePlugin>();
            //homePlugin.Sel

            IDeviceInfo? devInfo = DdpmHomePlugin.PluginIoc.GetService<IDeviceInfo>();
            if (devInfo != null)
            {
            }

            IDdpmHomePageViewModel? vm = Get_ddpmHomePageViewModel();

            //(IDdpmHomePageViewModel)vm.SelDevice = (DeviceInfo)selDev;

            DdpmHomePageViewModel vm2 = (DdpmHomePageViewModel)DataContext;
            vm2.SelDevice = selDev;

            if (selDev == null)
                return;

            if (selDev.DeviceCategory == eDeviceCategory.Display)
            {
                IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                console?.ShowPluginById(DDPM.UI.Common.Constants.DisplayPluginId);
            }

            if (selDev.DeviceCategory == eDeviceCategory.KB)
            {
                IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                console?.ShowPluginById(DDPM.UI.Common.Constants.KeyboardPluginId);
            }

            if (selDev.DeviceCategory == eDeviceCategory.Mouse)
            {
                IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                console?.ShowPluginById(DDPM.UI.Common.Constants.MousePluginId);
            }
            */
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_ddpmHomePageViewModel != null)
            {
                //If the DdpmHomePageViewModel.PrepareMonitorInfos() was called,
                //then we can remove below assignment
                //_ddpmHomePageViewModel.HomeDevices = DdpmHomePlugin.HomeDevices;

                RefreshListViewItemWidth();
                //InitCollectionViews();
            }
            //Register DeviceChanged event 2024-6-20 move to Homeplugin
            //if (DdpmCommonHelper.DeviceManagerSA != null)
            //{
            //    DdpmCommonHelper.DeviceManagerSA.DeviceChanged += DeviceManagerSA_DeviceChanged;
            //}

            //Reference to [https://stackoverflow.com/questions/27729881/which-event-fires-after-all-items-are-loaded-and-shown-in-a-listview]
            //To get into RenderingDone() when UI is render done.
            Dispatcher.BeginInvoke(new Action(RenderingDone), System.Windows.Threading.DispatcherPriority.ContextIdle, null);
        }

        //private void DeviceManagerSA_DeviceChanged(object? sender, SA.Common.DeviceChangedEventArgs e)
        //{
        //    //_ = Task.Run(RefreshHomeDeviceListAsync);
        //}

        //Robert_Lin, 2024-6-26, fix SAST issue: [Bug] Return 'Task' instead
        //This method should be unused, rename the method, and add the suggest solution.
        //It can be removed any time.
        //OLD Code:
        //  private async void RefreshHomeDeviceListAsync()
        //NEW Code:
        /*private async Task RefreshHomeDeviceListAsync_Unused()
        {
            //if (_ddpmHomePageViewModel != null)
            //{
            //    //Dispatcher.Invoke(() =>
            //    //{
            //    //    _ddpmHomePageViewModel.HomeDevices.Clear();
            //    //});

            //    if (DdpmCommonHelper.DeviceManagerSA != null)
            //    {
            //        List<MonitorInfo> monitorInfos = await DdpmCommonHelper.DeviceManagerSA.GetMonitors();

            //        DeviceHelper deviceHelper = await DdpmCommonHelper.DeviceManagerSA.GetDevices();
            //        List<DeviceInfo> deviceInfos = new List<DeviceInfo>();
            //        if ((deviceHelper != null) && (deviceHelper.deviceInfo != null))
            //        {
            //            deviceInfos = deviceHelper.deviceInfo;
            //        }

            //        _ddpmHomePageViewModel.ResetDevices();
            //        _ddpmHomePageViewModel.PrepareMonitorInfos(monitorInfos);
            //        _ddpmHomePageViewModel.PrepareDeviceInfos(deviceInfos);
            //    }

            //}
        }*/

        #region RWD HomeDevices

        //v1.03 2024-6-22 Robert_Lin, 4 items per row first
        // ItemCount = 1~3, use the same rule with v1.02
        // If ItemCount >= 4, Arrange 4 item per row first
        // Calculate methods: CalculateItemWidthV3_xxxx
        private const double minWidth = 280;

        private const double minGap = 32;

        //v1.02 2024-5-18 Robert_lin
        // No maxWidth limitation (that means maxWidth can be removed)
        // The ItemsPerRow can be determined by the Breakpoints (bkpt?)
        private const double maxWidth = 500;

        //private const double minWidth = 280;
        //private const double minGap = 32;
        private double ratioItemView = (640.000 / 730.000);

        //The height of BatteryIndicator
        private const double cyBatteryIndicator = 40.000;

        //The break-points
        //bkpt2: if (cxView<bkpt2) then ItemsPerRow=1
        //bkpt3: if (xView<bkpt3) then ItemsPerRow<=2
        private const double bkpt2 = minWidth * 2 + minGap * 3; //656

        private const double bkpt3 = minWidth * 3 + minGap * 4; //968
        private const double bkpt4 = minWidth * 4 + minGap * 5; //1280
        private const double bkpt5 = minWidth * 5 + minGap * 6;

        private double _screenScale = 1.000; //Refresh in RefreshListViewItemWidth()

        private void RefreshListViewItemWidth()
        {
            double cxView = HomeDevicesListView.ActualWidth;
            double cyView = HomeDevicesListView.ActualHeight;

            //Robert_Lin, 2024-8-7, skip refresh if Homepage is not displayed (cxView==0)
            if ((cxView == 0) || (cyView == 0))
                return;

            var primaryScreenScalingRatio = Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth;

            DpiScale dpiScale = VisualTreeHelper.GetDpi(this);
            double scale = dpiScale.PixelsPerDip;
            _screenScale = dpiScale.PixelsPerDip;

            //if (DdpmCommonHelper.IsMainWindowAtPrimaryScreen)
            //{
            //    scale = 1.000;
            //}

            //cxView /= scale;
            //cyView /= scale;

            if (_ddpmHomePageViewModel != null)
            {
                double newWidth = minWidth;
                //double newHeight = 290;

                if (_ddpmHomePageViewModel.HomeDevices.Count == 1)
                {
                    newWidth = CalculateItemWidthV3_ItemsPerRow1(cxView, cyView);
                }
                else if (_ddpmHomePageViewModel.HomeDevices.Count == 2)
                {
                    if (cxView < bkpt2) //656
                        newWidth = CalculateItemWidthV3_ItemsPerRow1(cxView, cyView);
                    else
                        newWidth = CalculateItemWidthV3_ItemsPerRow2(cxView, cyView);

                    //Robert_Lin debug, force small icon to test ConnectionHoverView
                    //newWidth = minWidth - 100;
                }
                else if (_ddpmHomePageViewModel.HomeDevices.Count == 3)
                {
                    if (cxView < bkpt2) //656
                        newWidth = CalculateItemWidthV3_ItemsPerRow1(cxView, cyView);
                    else if (cxView < bkpt3) //968
                        newWidth = CalculateItemWidthV3_ItemsPerRow2(cxView, cyView);
                    else
                        newWidth = CalculateItemWidthV3_ItemsPerRow3(cxView, cyView);
                }
                else if (_ddpmHomePageViewModel.HomeDevices.Count == 4)
                {
                    if (cxView < bkpt2) //656
                        newWidth = CalculateItemWidthV3_ItemsPerRow1(cxView, cyView);
                    else if (cxView < bkpt3) //968
                        newWidth = CalculateItemWidthV3_ItemsPerRow2(cxView, cyView);
                    else if (cxView < bkpt4) //1280
                        newWidth = CalculateItemWidthV3_ItemsPerRow3(cxView, cyView);
                    else
                        newWidth = CalculateItemWidthV3_ItemsPerRow4(cxView, cyView);
                }
                //2024-6-23, HomePage RWD, 4 items per row first, so never > 4 items/row
                //else if (_ddpmHomePageViewModel.HomeDevices.Count == 5)
                //{
                //    if (cxView < bkpt2)
                //        newWidth = CalculateItemWidth_ItemsPerRow1(cxView, cyView);
                //    else if (cxView < bkpt3)
                //        newWidth = CalculateItemWidth_ItemsPerRow2(cxView, cyView);
                //    else if (cxView < bkpt4)
                //        newWidth = CalculateItemWidth_ItemsPerRow3(cxView, cyView);
                //    else if (cxView < bkpt5)
                //        newWidth = CalculateItemWidth_ItemsPerRowN(cxView, cyView, 5);
                //    else
                //        newWidth = CalculateItemWidth_ItemsPerRowN(cxView, cyView, 6);
                //}
                else //ItemCount > 4
                {
                    if (cxView < bkpt2)
                        newWidth = CalculateItemWidthV3_ItemsPerRow1(cxView, cyView);
                    else if (cxView < bkpt3)
                        newWidth = CalculateItemWidthV3_ItemsPerRow2(cxView, cyView);
                    else if (cxView < bkpt4)
                        newWidth = CalculateItemWidthV3_ItemsPerRow3(cxView, cyView);
                    else //2024-6-23, Robert_Lin, RWD 4 item per row first
                    {
                        //If ItemCount>4, and cxView>=bkpt4(1280), will show 4 items/row
                        newWidth = CalculateItemWidthV3_ItemsPerRow4(cxView, cyView);
                    }
                    //else if (cxView < bkpt5)
                    //    newWidth = CalculateItemWidth_ItemsPerRowN(cxView, cyView, 5);
                    //    else
                    //    {
                    //        for (int n = 6; n <= _ddpmHomePageViewModel.HomeDevices.Count; n++)
                    //        {
                    //            if (cxView < (minWidth * n) + minGap * (n + 1))
                    //            {
                    //                newWidth = CalculateItemWidth_ItemsPerRowN(cxView, cyView, n);
                    //                break;
                    //            }
                    //        }
                    //    }
                    //}
                }

                //If MainWindow is in Primary screen,  we need to div by scale.
                //Otherwise (not primary screen), we don't need (by reset scale to 1)
                if (!DdpmCommonHelper.IsMainWindowAtPrimaryScreen)
                    scale = 1.0000;

                Dispatcher.Invoke(new Action(() =>
                {
                    DataContext = null;
                    foreach (HomeDevice dev in _ddpmHomePageViewModel.HomeDevices)
                    {
                        dev.NormalWidth = newWidth / scale;
                    }
                    _ddpmHomePageViewModel.cxItem = newWidth;
                    DataContext = _ddpmHomePageViewModel;
                }));
            }
        }

        private double CalculateItemWidthV3_ItemsPerRow1(double cxView, double cyView)
        {
            //Robert_Lin, 2024-10-1 Special for huge monitor (4K)
            //When screen resolution is very large, the ratio to gap to batteryIndicator is very large
            //
            double gapRatio = 1;
            if (cxView >= 2200)
            {
                gapRatio = 2.0;
            }
            //Robert_Lin, 2024-10-1 Special for huge monitor (4K)
            double hugeReduce = 0;
            if (cxView >= 2200)
            {
                //hugeReduce = 100;
            }
            //sizeView = min (cxView, cyView)
            double sizeView = Math.Min(cxView, cyView - cyBatteryIndicator * 2 * gapRatio - hugeReduce);

            //Calculate the sizeItem
            double sizeItem = sizeView - (minGap * 2 * gapRatio); //sizeView * ratioItemView;

            //But the sizeItem must >= minWidth
            if (sizeItem < minWidth)
                return minWidth;

            return sizeItem;
        }

        private double CalculateItemWidthV3_ItemsPerRow2(double cxView, double cyView)
        {
            //Robert_Lin, 2024-10-1 Special for huge monitor (4K)
            //When screen resolution is very large, the ratio to gap to batteryIndicator is very large
            //
            double gapRatio = 1;
            if (cxView >= 2200)
            {
                gapRatio = 2.0;
            }
            double cxItem = (cxView - (minGap * 3.000 * gapRatio)) / 2.000;
            double cyItem = (cyView - (minGap * 2.000 * gapRatio));
            double sizeItem = Math.Min(cxItem, cyItem - cyBatteryIndicator * 2 * gapRatio);
            return sizeItem;
        }

        private double CalculateItemWidthV3_ItemsPerRow3(double cxView, double cyView)
        {
            //Robert_Lin, 2024-10-1 Special for huge monitor (4K)
            double hugeReduce = 0;
            if (cxView >= 2200)
            {
                hugeReduce = 100;
            }
            double cxItem = (cxView - (minGap * 4.000)) / 3.000;
            double cyItem = (cyView - (minGap * 2.000));
            double sizeItem = Math.Min(cxItem, cyItem - cyBatteryIndicator * 2 - hugeReduce * 3);
            return sizeItem;
        }

        private double CalculateItemWidthV3_ItemsPerRow4(double cxView, double cyView)
        {
            //Add margin in cxItem to avoid internal margin
            double cxItem = (cxView - (minGap * 7.000)) / 4.000;
            double cyItem = (cyView - (minGap * 2.000));
            double sizeItem = Math.Min(cxItem, cyItem - cyBatteryIndicator * 2);
            return sizeItem;
        }

        //Robert_Lin, 2024-8-7 Unused code, can be removed
        private double CalculateItemWidth_ItemsPerRow1(double cxView, double cyView)
        {
            //sizeView = min (cxView, cyView)
            double sizeView = Math.Min(cxView, cyView - cyBatteryIndicator * 2);

            //Calculate the sizeItem
            double sizeItem = sizeView - (minGap * 2); //sizeView * ratioItemView;

            //But the sizeItem must >= minWidth
            if (sizeItem < minWidth)
                return minWidth;

            return sizeItem;
        }

        //Robert_Lin, 2024-8-7 Unused code, can be removed
        private double CalculateItemWidth_ItemsPerRow2(double cxView, double cyView)
        {
            double cxItem = (cxView - (minGap * 3.000)) / 2.000;
            double cyItem = (cyView - (minGap * 2.000));
            double sizeItem = Math.Min(cxItem, cyItem - cyBatteryIndicator * 2);
            return sizeItem;
            /*
            //sizeView = min (cxView, cyView)
            double sizeView = Math.Min(cxView, cyView);

            //Calculate the sizeItem
            double sizeItem = sizeView * ratioItemView;

            if (cxView > (maxWidth * 2 + minGap * 3))
            {
                return maxWidth;
            }

            if (cxView > bkpt2) //656
            {
                double cxItem = (cxView - (minGap * 3)) / 2;
                //Consider cyView
                if (cyView < cxItem)
                {
                    if (cyView > minWidth)
                        return cyView;
                    return minWidth;
                }
                return cxItem;
            }

            return minWidth;*/
        }

        //Robert_Lin, 2024-8-7 Unused code, can be removed
        private double CalculateItemWidth_ItemsPerRow3(double cxView, double cyView)
        {
            double cxItem = (cxView - (minGap * 4.000)) / 3.000;
            double cyItem = (cyView - (minGap * 2.000));
            double sizeItem = Math.Min(cxItem, cyItem - cyBatteryIndicator * 2);
            return sizeItem;

            /*
            if (cxView > (maxWidth * 3 + minGap * 4)) //1628
                return maxWidth;

            if (cxView > bkpt3) //968
            {
                double cxItem = (cxView - (minGap * 4)) / 3;
                //Consider cyView
                if (cyView < cxItem)
                {
                    if (cyView > minWidth)
                        return cyView;
                    return minWidth;
                }
                return cxItem;
            }
            return minWidth;*/
        }

        //Robert_Lin, 2024-8-7 Unused code, can be removed
        private double CalculateItemWidth_ItemsPerRow4(double cxView, double cyView)
        {
            double cxItem = (cxView - (minGap * 5.000)) / 4.000;
            double cyItem = (cyView - (minGap * 2.000));
            double sizeItem = Math.Min(cxItem, cyItem - cyBatteryIndicator * 2);
            return sizeItem;

            /*
            if (cxView > (maxWidth * 4 + minGap * 5)) //1628
                return maxWidth;

            if (cxView > bkpt4) //968
            {
                double cxItem = (cxView - (minGap * 5)) / 4;
                //Consider cyView
                if (cyView < cxItem)
                {
                    if (cyView > minWidth)
                        return cyView;
                    return minWidth;
                }
                return cxItem;
            }
            return minWidth;*/
        }

        //Robert_Lin, 2024-8-7 Unused code, can be removed
        private double CalculateItemWidth_ItemsPerRowN(double cxView, double cyView, int n)
        {
            double cxItem = (cxView - (minGap * (n + 1))) / n;
            double cyItem = (cyView - (minGap * 2.000));
            double sizeItem = Math.Min(cxItem, cyItem - cyBatteryIndicator * 2);
            return sizeItem;

            /*
            if (n < 1)
                return maxWidth;

            if (cxView > (maxWidth * n + minGap * (n + 1)))
                return maxWidth;

            if (cxView > (minWidth * n + minGap * (n + 1)))
            {
                double cxItem = (cxView - (minGap * (n + 1))) / n;
                if (cyView < cxItem)
                {
                    if (cyView > minWidth)
                        return cyView;
                    return minWidth;
                }
                return cxItem;
            }
            return minWidth;*/
        }

        private void rootUserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            RefreshListViewItemWidth();
        }

        #endregion RWD HomeDevices

        #region HomeDevice Selection and Navigate to Landing Page

        //Mouse left button down
        private void devListViewItemRoot_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Grid)
            {
                //e.Handled = true;

                Grid grid = sender as Grid;
                if (grid.DataContext != null)
                {
                    HomeDevice? dev = grid.DataContext as HomeDevice;

                    //Robert_Lin, 2024-8-8 Move below code segment into NavigateToDeviceLandingPage()
                    //
                    if (dev != null)
                    {
                        NavigateToDeviceLandingPage(dev);
                    }

                    /*
                    if (_ddpmHomePageViewModel != null)
                    {
                        _ddpmHomePageViewModel.SelectedHomeDevice = dev;
                    }
                    //DdpmHomePlugin.SelectedHomeDevice = dev;

                    //Robert_Lin, 2024-8-6, append instanceNo:
                    // If (InstanceNo==0) No instaneNo => "{ID}"
                    // If (InstanceNo!=0) has instaneNo => "{ID}-{InstanceNo}"
                    string instanceNo = "";
                    if (dev.InstanceNo != 0)
                        instanceNo = $"-{dev.InstanceNo}";

                    if (dev?.DeviceCategory == eDeviceCategory.Display)
                    {
                        //Check if it's fake device
                        if (dev.MonitorInfo == null)
                            return;

                        IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                        console?.ShowPluginById(DDPM.UI.Common.Constants.DisplayPluginId);

                        //IConsole? myConsole = DdpmCommonHelper.MyConsole;
                        //myConsole?.ShowPluginById(Constants.DisplayPluginId);
                        return;
                    }
                    if (dev?.DeviceCategory == eDeviceCategory.KB)
                    {
                        //Check if it's fake device
                        if (dev.DeviceInfo == null)
                            return;

                        //IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                        //console?.ShowPluginById(DDPM.UI.Common.Constants.KeyboardPluginId);
                        IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                        _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.KeyboardPluginId, dev.DeviceInfo.ID.ToString()+instanceNo);
                    }
          if(dev?.DeviceCategory == eDeviceCategory.Mouse) {
            //Check if it's fake device
            if(dev.DeviceInfo == null)
              return;

            //IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
            //console?.ShowPluginById(DDPM.UI.Common.Constants.MousePluginId);
            IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
            _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.MousePluginId, dev.DeviceInfo.ID.ToString() + instanceNo);
          }
          // 240722 Added by Hess to show Pen landing page
          if(dev?.DeviceCategory == eDeviceCategory.Pen) {
            if(dev.DeviceInfo == null)
              return;
            IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
            _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.PenPluginId, dev.DeviceInfo.ID.ToString() + instanceNo);
          }
          // jim add 20240613
          if(dev?.DeviceCategory == eDeviceCategory.Webcam)
                    {
                        //Check if it's fake device
                        if (dev.DeviceInfo == null)
                            return;

                        //IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                        //console?.ShowPluginById(DDPM.UI.Common.Constants.MousePluginId);
                        IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                        _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.WebCameraPluginId, dev.DeviceInfo.ID.ToString() + instanceNo);
                    }
                    //0614 Bruce 新增Dock UI
                    if (dev?.DeviceCategory == eDeviceCategory.Dock)
                    {
                        //Check if it's fake device
                        if (dev.DeviceInfo == null)
                            return;

                        //IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                        //console?.ShowPluginById(DDPM.UI.Common.Constants.MousePluginId);
                        IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                        _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.DockPluginId, dev.DeviceInfo.ID.ToString());
                    }
                    //0619 Wayn 新增HeatSet UI
                    if (dev?.DeviceCategory == eDeviceCategory.Headset)
                    {
                        //Check if it's fake device
                        if (dev.DeviceInfo == null)
                            return;

                        //IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                        //console?.ShowPluginById(DDPM.UI.Common.Constants.MousePluginId);
                        IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                        _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.HeadsetPluginId, dev.DeviceInfo.ID.ToString() + instanceNo);
                    }
                    //0726 Wayn 新增Soundbar UI
                    if (dev?.DeviceCategory == eDeviceCategory.Soundbar)
                    {
                        //Check if it's fake device
                        if (dev.DeviceInfo == null)
                            return;

                        //IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                        //console?.ShowPluginById(DDPM.UI.Common.Constants.MousePluginId);
                        IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                        _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.SoundBarPluginId, dev.DeviceInfo.ID.ToString() + instanceNo);
                    } */
                }
            }
        }

        //Keyboard Enter down
        private void HomeDevicesListView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (e.OriginalSource == null)
                    return;
                System.Windows.Controls.ListViewItem lvItem = (System.Windows.Controls.ListViewItem)e.OriginalSource;
                if (lvItem == null)
                    return;
                if (lvItem.DataContext == null)
                    return;
                HomeDevice homeDevice = lvItem.DataContext as HomeDevice;
                if (homeDevice == null)
                    return;
                e.Handled = true;
                NavigateToDeviceLandingPage(homeDevice);
            }
        }

        private void NavigateToDeviceLandingPage(HomeDevice selectedHomeDevice)
        {
            //Store the SelectedHomeDevice for Plugins usage
            if (_ddpmHomePageViewModel != null)
            {
                _ddpmHomePageViewModel.SelectedHomeDevice = selectedHomeDevice;
            }

            //Prepare InstanceNo for Peripherals,
            //will provide it from ShwPluginById(pluginId, deviceId+InstanceNo)
            //Robert_Lin, 2024-8-6, append instanceNo:
            // If (InstanceNo==0) No instaneNo => "{ID}"
            // If (InstanceNo!=0) has instaneNo => "{ID}-{InstanceNo}"
            string instanceNo = "";
            if (selectedHomeDevice.InstanceNo != 0)
                instanceNo = $"-{selectedHomeDevice.InstanceNo}";

            //Load Plugin depened on DeviceCategory
            //
            if (selectedHomeDevice?.DeviceCategory == eDeviceCategory.Display)
            {
                IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                console?.ShowPluginById(DDPM.UI.Common.Constants.DisplayPluginId);
                return;
            }
            if (selectedHomeDevice?.DeviceCategory == eDeviceCategory.KB)
            {
                ////Check if it's fake device
                if (selectedHomeDevice.DeviceInfo == null)
                    return;

                IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.KeyboardPluginId, selectedHomeDevice.DeviceInfo.ID.ToString() + instanceNo);
            }
            if (selectedHomeDevice?.DeviceCategory == eDeviceCategory.Mouse)
            {
                //Check if it's fake device
                if (selectedHomeDevice.DeviceInfo == null)
                    return;

                IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.MousePluginId, selectedHomeDevice.DeviceInfo.ID.ToString() + instanceNo);
            }
            // 240722 Added by Hess to show Pen landing page
            if (selectedHomeDevice?.DeviceCategory == eDeviceCategory.Pen)
            {
                if (selectedHomeDevice.DeviceInfo == null)
                    return;
                IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.PenPluginId, selectedHomeDevice.DeviceInfo.ID.ToString() + instanceNo);
            }
            // jim add 20240613
            if (selectedHomeDevice?.DeviceCategory == eDeviceCategory.Webcam)
            {
                //Check if it's fake device
                if (selectedHomeDevice.DeviceInfo == null)
                    return;

                IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.WebCameraPluginId, selectedHomeDevice.DeviceInfo.ID.ToString() + instanceNo);
            }
            //0614 Bruce 新增Dock UI
            if (selectedHomeDevice?.DeviceCategory == eDeviceCategory.Dock)
            {
                //Check if it's fake device
                if (selectedHomeDevice.DeviceInfo == null)
                    return;

                IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.DockPluginId, selectedHomeDevice.DeviceInfo.ID.ToString());
            }
            //0619 Wayn 新增HeatSet UI
            if (selectedHomeDevice?.DeviceCategory == eDeviceCategory.Headset)
            {
                //Check if it's fake device
                if (selectedHomeDevice.DeviceInfo == null)
                    return;

                IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.HeadsetPluginId, selectedHomeDevice.DeviceInfo.ID.ToString() + instanceNo);
            }
            //0726 Wayn 新增Soundbar UI
            if (selectedHomeDevice?.DeviceCategory == eDeviceCategory.Soundbar)
            {
                //Check if it's fake device
                if (selectedHomeDevice.DeviceInfo == null)
                    return;

                IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.SoundBarPluginId, selectedHomeDevice.DeviceInfo.ID.ToString() + instanceNo);
            }
        }

        #endregion HomeDevice Selection and Navigate to Landing Page

        #region Debug - RWD self testing

        private MonitorInfo GetFakeMonitorInfo()
        {
            MonitorInfo info = new MonitorInfo();
            info.AliasDeviceName = "Fake Monitor";
            info.inputSource = "Internal";
            info.CapabilityString = "";
            info.FwVersion = "1.0";
            info.DDCisON = false;
            info.DisplayName = @"\\\\.\\DISPLAY2";
            info.Index = 1;
            info.IsDellMonitor = false;
            info.edid = new VcpCore.Common.EDID();
            info.edid.Month = 6;
            info.edid.Year = 2024;
            info.edid.SerialNumber = "A12345";
            info.edid.EdidVersion = "V1.4";
            info.edid.ManufactureID = "LGD";
            info.edid.ServiceTag = "ABCDE";
            info.edid.ModelName = "INTER";
            info.edid.Size = 12;
            info.edid.Week = 2;
            info.edid.VideoInputType = "digital singal";
            return info;
        }

        private void addItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_ddpmHomePageViewModel != null)
            {
                int idx = _ddpmHomePageViewModel.HomeDevices.Count;
                MonitorInfo mi = GetFakeMonitorInfo();
                mi.Index = idx;
                _ddpmHomePageViewModel.PrepareMonitorInfos(new List<MonitorInfo> { mi });
            }

            //HomeDevice demo = new HomeDevice()
            //{
            //    DeviceCategory = eDeviceCategory.KB,
            //    DeviceName = $"Demo {id}",
            //    DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png")
            //};
            //_ddpmHomePageViewModel.AddDemoHomeDevice(demo);
            RefreshListViewItemWidth();
        }

        private void addDisplay_Click(object sender, RoutedEventArgs e)
        {
            if (_ddpmHomePageViewModel != null)
            {
                int idx = _ddpmHomePageViewModel.HomeDevices.Count;
                MonitorInfo mi = GetFakeMonitorInfo();
                mi.Index = idx;
                _ddpmHomePageViewModel.PrepareMonitorInfos(new List<MonitorInfo> { mi });
            }
            //int id = _ddpmHomePageViewModel.HomeDevices.Count;
            //HomeDevice demo = new HomeDevice()
            //{
            //    DeviceCategory = eDeviceCategory.Display,
            //    DeviceName = $"Demo {id}",
            //    DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_Display.png")
            //};
            //_ddpmHomePageViewModel.AddDemoHomeDevice(demo);
            RefreshListViewItemWidth();
            if (_ddpmHomePageViewModel != null)
                _ddpmHomePageViewModel.RefreshCollectionView();
        }

        private void addKb_Click(object sender, RoutedEventArgs e)
        {
            int id = _ddpmHomePageViewModel.HomeDevices.Count;

            HomeDevice demo = new HomeDevice()
            {
                DeviceCategory = eDeviceCategory.KB,
                DeviceName = $"Demo {id}",
                DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png")
            };
            _ddpmHomePageViewModel.AddDemoHomeDevice(demo);
            RefreshListViewItemWidth();
        }

        private void addMouse_Click(object sender, RoutedEventArgs e)
        {
            int id = _ddpmHomePageViewModel.HomeDevices.Count;

            HomeDevice demo = new HomeDevice()
            {
                DeviceCategory = eDeviceCategory.Mouse,
                DeviceName = $"Demo {id}",
                DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_Mouse.png")
            };
            _ddpmHomePageViewModel.AddDemoHomeDevice(demo);
            RefreshListViewItemWidth();
        }

        #endregion Debug - RWD self testing

        #region CollectionView for HomeDevice Sort and grouping

        private void InitCollectionViews()
        {
            //Robert_lin, 2024-7-10, unused, use DdpmHomePageViewModel.RefreshCollectionView() instead.
            //if (_ddpmHomePageViewModel != null)
            //{
            //    ListCollectionView colView = (ListCollectionView)CollectionViewSource.GetDefaultView(_ddpmHomePageViewModel.HomeDevices);
            //    colView.CustomSort = new HomeDeviceSorter();
            //}
        }

        #endregion CollectionView for HomeDevice Sort and grouping

        #region HomeDevices Changed event handler

        //Robert_Lin, 2024-6-24, when HomeDevices changed, we need to recalculate the item width,
        //that it, rearranged with RWD rule.
        private void _ddpmHomePageViewModel_HomeDevicesChanged(object? sender, EventArgs e)
        {
            RefreshListViewItemWidth();
            //InitCollectionViews();
        }

        #endregion HomeDevices Changed event handler

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_ddpmHomePageViewModel != null)
                _ddpmHomePageViewModel.RaiseShowAddDevicePlugin();
        }

        #region Read/Write INI file

        //Robert_Lin 2024-7-5 copy from VCPCorePlugin.cs, shared with other projects
        public static int IniReadInt(string sec, string key, int def, string pathName)
        {
            return _GetPrivateProfileInt(sec, key, def, pathName);
        }

        //Usage: int value=GetPrivateProfileInt("sectionName", "key", 3, @"C:\temp\a.ini");
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetPrivateProfileInt(string section, string key, int def, string filePath);
        private static int _GetPrivateProfileInt(string section, string key, int def, string filePath)
        {
            return GetPrivateProfileInt(section, key, def, filePath);
        }

        //Uage:
        // //allocate string buffer, for large string you can allocate 4096 chars.
        // StringBuilder sb1=new StringBuilder(255);
        // int charsRet=GetPrivateProfileString("secName","key","defValue",sb1,sb1.Capacity,@"C:\temp\a.ini");
        // string result=sb1.ToString();
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        public static int _GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath)
        {
            return GetPrivateProfileString(section, key, def, retVal, size, filePath);
        }
        #endregion Read/Write INI file

        #region Connection Hover View

        private ConnectionHoverView? _connHoverView = null;

        private void batteryIndicator_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //sender is BatteryIndicator
            if (sender == null)
                return;
            BatteryIndicator bi = (BatteryIndicator)sender;

            //ViewModel of BatteryIndicator is HomeDevice
            if (bi.DataContext == null)
                return;
            HomeDevice homeDevice = bi.DataContext as HomeDevice;

            //If the device is Display
            if (homeDevice.DeviceCategory == eDeviceCategory.Display)
                return;

            if (homeDevice.DeviceInfo == null)
                return;
            DeviceInfo di = homeDevice.DeviceInfo as DeviceInfo;

            homeDevice.IsConnectionHoverViewShow = true;

            //if (_connHoverView == null)
            //{
            //    _connHoverView = new ConnectionHoverView();
            //    connHover.Content = _connHoverView;
            //}
            //connHover.Visibility = Visibility.Visible;
        }

        private void batteryIndicator_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //sender is BatteryIndicator
            if (sender == null)
                return;
            BatteryIndicator bi = (BatteryIndicator)sender;

            //ViewModel of BatteryIndicator is HomeDevice
            if (bi.DataContext == null)
                return;
            HomeDevice homeDevice = bi.DataContext as HomeDevice;

            if (homeDevice == null)
                return;

            //If the device is Display
            if (homeDevice.DeviceCategory == eDeviceCategory.Display)
                return;

            if (homeDevice.DeviceInfo == null)
                return;
            DeviceInfo di = homeDevice.DeviceInfo as DeviceInfo;

            homeDevice.IsConnectionHoverViewShow = false;
            //connHover.Visibility = Visibility.Collapsed;
        }

        #endregion Connection Hover View

        private void RenderingDone()
        {
            RefreshListViewItemWidth();

            System.Windows.Threading.DispatcherTimer dispTimer = new System.Windows.Threading.DispatcherTimer();
            dispTimer.Tick += delegate
            {
                dispTimer.Stop();
                RefreshListViewItemWidth();
            };
            dispTimer.Interval = new TimeSpan(200);
            dispTimer.Start();
        }

        private void _ddpmHomePageViewModel_ShowConsent(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() => {
                Window parentWindow = Window.GetWindow(this);
                double windowLeft = 0;
                double windowTop = 0;
                ConsentModalDialog modalDialog = new(parentWindow.ActualWidth, parentWindow.ActualHeight - 40);
                if (parentWindow != null)
                {
                    modalDialog.Owner = parentWindow;
                    windowLeft = parentWindow.Left;
                    windowTop = parentWindow.Top + 40;
                }
                modalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                modalDialog.Left = windowLeft;
                modalDialog.Top = windowTop;
                var _globalSettings = DdpmCommonHelper.DeviceManagerSA!.GetGlobalSettingParam().Result;
                if (modalDialog.ShowDialog()!.Value)
                {
                    _ = DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_EnableTelemetryConsent(true).Result;
                }
                else
                {
                    _ = DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_EnableTelemetryConsent(false).Result;
                }
            });
        }
    }
}