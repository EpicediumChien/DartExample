using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.DdpmHomePlugin.ViewModels;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using VcpCore.Common;
using IDdpmHomePageViewModel = DDPM.UI.Plugin.DdpmHomePlugin.Interfaces.IDdpmHomePageViewModel;
using DDPM.SA.Common.Settings;
using System.Diagnostics;
using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using DDPM.UI.Common.UserControls;
using System.Collections.ObjectModel;

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
                _ddpmHomePageViewModel.HomeDevicesChanged -= _ddpmHomePageViewModel_HomeDevicesChanged;
                _ddpmHomePageViewModel.HomeDevicesChanged += _ddpmHomePageViewModel_HomeDevicesChanged;

                //Robert_Lin, 2025-1-7, the DDPMDebug.txt solution will be removed, use DevSettings instaed.
                //NEW:
                if (DevSettings.DdpmHomeShowDeviceListViewToolbar())
                //OLD:
                ////Robert_Lin, 2024-7-16 for engineer debug,
                //if (DDPM.UI.Common.User32.IniReadInt("DDPMDebug", "HomePage.ShowDeviceListViewToolbar", 0, @"C:\temp\DDPMDebug.txt") == 1)
                {
                    UIDebugPanel.Visibility = Visibility.Visible;
                }
                DdpmCommonHelper.BitmapImageUpdated -= _DDPMThemeChange;
                DdpmCommonHelper.BitmapImageUpdated += _DDPMThemeChange;

                AttachImportNotification();
            }
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // TODO Change the UXControls:UXTextBlock
            }), System.Windows.Threading.DispatcherPriority.Background);
            if (DdpmCommonHelper.UIDebugModeFlag)
            {
                UIDebugPanel.Visibility = Visibility.Visible;
            }
            this.MinWidth = System.Windows.Application.Current.MainWindow.MinWidth;
            //System.Windows.Application.Current.MainWindow.StateChanged -= MainWindow_StateChanged;
            //System.Windows.Application.Current.MainWindow.StateChanged += MainWindow_StateChanged;
        }

        //private void MainWindow_StateChanged(object? sender, EventArgs e)
        //{
        //    Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        RefreshListViewItemWidth();
        //    }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        //}

        private void ImportNotifyEventHandler(object sender, MonitorInfo mo)
        {
            try
            {
                if (mo != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        List<Task> tasks = new List<Task>();
                        bool isSameModelFlag = false;
                        string localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dell");
                        string path = localAppDataPath + "\\Dell Display and Peripheral Manager\\Export";
                        string model = mo.modelName;//"U2724DE";
                        string exportpath = path + "\\" + model + ".json";
                        string displayProfilePath = $"{localAppDataPath}\\Dell Display and Peripheral Manager\\Display\\{model}.json";

                        DDPMImpExpSettings ImpExpSettings = new DDPMImpExpSettings();
                        //ImpExpSettings = DdpmCommonHelper.DeviceManagerSA.ReadImportSettingsFile(displayProfilePath).Result;

                        Task<DDPMImpExpSettings> readExp = DdpmCommonHelper.DeviceManagerSA.ReadImportSettingsFile(displayProfilePath);
                        Task<bool> readSameModel = DdpmCommonHelper.DeviceManagerSA.ReadSameModelAutoApplySameModelFlag(displayProfilePath, model);
                        tasks.Add(readExp);
                        tasks.Add(readSameModel);
                        Task.WhenAll(tasks).Wait();
                        ImpExpSettings = readExp.Result;
                        isSameModelFlag = readSameModel.Result;

                        ImpExpSettings = DdpmCommonHelper.DeviceManagerSA.ReadImportSettingsFile(exportpath).Result;

                        if (ImpExpSettings != null &&
                            ImpExpSettings.MonitorSettings != null &&
                            ImpExpSettings.MonitorSettings.ServiceTag != mo.edid.ServiceTag)
                        {
                            //if (ImpExpSettings.MonitorSettings.ImpExpSettings.SameModel)
                            if (isSameModelFlag)
                            {
                                DdpmCommonHelper.DeviceManagerSA.DisplayImportSettings(mo, true, exportpath).Wait();
                            }
                            else
                            {
                                Window parentWindow = System.Windows.Application.Current.MainWindow;
                                double windowLeft = 0;
                                double windowTop = 0;
                                double actualWidth = 0;
                                double actualHeight = 0;
                                if (parentWindow == null)
                                {
                                    DdpmCommonHelper.WriteUILog($"[DdpmHomePlugin] Error cannot get MainWindow value", memberName: nameof(parentWindow));
                                    return;
                                }
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

                                if (modalDialog.DialogResult != null &&
                                    modalDialog.DialogResult == true &&
                                    (int)DdpmCommonHelper.DeviceManagerSA.DisplayImportSettings(mo, true, exportpath).Result > 0 && //For jason to do import
                                    modalDialog.isChecked) //ignore next check for this model
                                {
                                    DdpmCommonHelper.DeviceManagerSA.SetSameModel(mo, true);
                                }
                            }
                        }

                    });
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[DdpmHomePage.] throws exception: {ex.Message}; StackTrace: {ex.StackTrace}", log_type: DdpmCommonHelper.log_type.error, memberName: nameof(ImportNotifyEventHandler));
                return;
            }
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

            //Determine whether current screen is small resolution
            Window mainWindow = System.Windows.Application.Current.MainWindow;
            IntPtr hMainWnd = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;
            Screen screenNow = Screen.FromHandle(hMainWnd);
            if (_ddpmHomePageViewModel != null)
            {
                _ddpmHomePageViewModel.IsSmallScreenResolution = screenNow.Bounds.Width < 1050;
            }

            //Reference to [https://stackoverflow.com/questions/27729881/which-event-fires-after-all-items-are-loaded-and-shown-in-a-listview]
            //To get into RenderingDone() when UI is render done.
            Dispatcher.BeginInvoke(new Action(RenderingDone), System.Windows.Threading.DispatcherPriority.ContextIdle, null);
        }

        #region RWD HomeDevices

        //v1.03 2024-6-22 Robert_Lin, 4 items per row first
        // ItemCount = 1~3, use the same rule with v1.02
        // If ItemCount >= 4, Arrange 4 item per row first
        // Calculate methods: CalculateItemWidthV3_xxxx
        private const double minWidth = 250;

        private const double minGap = 40;

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
        private const double bkpt2 = minWidth * 2 + minGap * 3; //620

        private const double bkpt3 = minWidth * 3 + minGap * 4; //810
        private const double bkpt4 = minWidth * 4 + minGap * 5; //1200

        private double _screenScale = 1.000; //Refresh in RefreshListViewItemWidth()
        private int refreshCounter = 1;

        private void RefreshListViewItemWidth()
        {
            #region Wait for device ready
            double cxView = this.ActualWidth;
            double cyView = this.ActualHeight;
#if DEBUG
            Debug.WriteLine($"Actual Width: {cxView}, Actual Height: {cyView}");
            Debug.WriteLine($"{_ddpmHomePageViewModel.HomeDevices.Count} HomeDevices need to show.");
#endif
            //Robert_Lin, 2024-8-7, skip refresh if Homepage is not displayed (cxView==0)
            System.Windows.Threading.DispatcherTimer dispTimer = new System.Windows.Threading.DispatcherTimer();
            //Add a retry after 300 msec
            dispTimer.Tick += delegate
            {
#if DEBUG
                Debug.WriteLine($"Refreshing count: {refreshCounter}");
#endif
                refreshCounter++;
                RefreshListViewItemWidth();
            };
            dispTimer.Interval = new TimeSpan(300);
            if (this.GetType().Name == "DdpmHomePage"
                && dispTimer.IsEnabled == false)
            {
                dispTimer.Start();
            }
            #endregion

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
                // MinWidth = 330
                if (_ddpmHomePageViewModel.HomeDevices.Count == 1)
                {
                    newWidth = CalculateItemWidthV3_ItemsPerRow1(cxView, cyView);
                }
                // MinWidth = 620
                else if (_ddpmHomePageViewModel.HomeDevices.Count == 2)
                {
                    newWidth = CalculateItemWidthV3_ItemsPerRow2(cxView, cyView);
                    //Robert_Lin debug, force small icon to test ConnectionHoverView
                    //newWidth = minWidth - 100;
                }
                // MinWidth = 910
                else if (_ddpmHomePageViewModel.HomeDevices.Count == 3)
                {
                    newWidth = CalculateItemWidthV3_ItemsPerRow3(cxView, cyView);
                }
                // MinWidth = 620*
                else if (_ddpmHomePageViewModel.HomeDevices.Count == 4)
                {
                    if (cxView < bkpt4) //1200
                        newWidth = CalculateItemWidthV3_ItemsPerRow2(cxView, cyView, _ddpmHomePageViewModel.HomeDevices.Count);
                    else
                        newWidth = CalculateItemWidthV3_ItemsPerRow4(cxView, cyView);
                }
                else if (_ddpmHomePageViewModel.HomeDevices.Count < 7)
                {
                    newWidth = CalculateItemWidthV3_ItemsPerRow3(cxView, cyView, _ddpmHomePageViewModel.HomeDevices.Count);
                }
                //2024-6-23, HomePage RWD, 4 items per row first, so never > 4 items/row
                //ItemCount > 4
                // MinWidth = 910 (3 items)
                else
                {
                    if (cxView < bkpt4)
                    {
                        newWidth = CalculateItemWidthV3_ItemsPerRow3(cxView, cyView, _ddpmHomePageViewModel.HomeDevices.Count);
                    }
                    else //2024-6-23, Robert_Lin, RWD 4 item per row first
                    {
                        //If ItemCount>4, and cxView>=bkpt4(1280), will show 4 items/row
                        newWidth = CalculateItemWidthV3_ItemsPerRow4(cxView, cyView, _ddpmHomePageViewModel.HomeDevices.Count);
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

                if (newWidth > maxWidth)
                {
                    newWidth = maxWidth;
                }
                else if (newWidth < minWidth)
                {
                    newWidth = minWidth;
                }
                // realWidth for HomeDevice.ItemWidth will scale 1.16
                double realWidth = newWidth / 1.16;

                //If MainWindow is in Primary screen,  we need to div by scale.
                //Otherwise (not primary screen), we don't need (by reset scale to 1)
                //if (!DdpmCommonHelper.IsMainWindowAtPrimaryScreen)
                //    scale = 1.0000;

                Dispatcher.Invoke(new Action(() =>
                {
                    DataContext = null;
                    // Confirmed to remove scale from Robert
                    foreach (HomeDevice dev in _ddpmHomePageViewModel.HomeDevices)
                    {
                        dev.NormalWidth = Math.Floor(realWidth * 100) / 100; // prevent border to wrap device by decimal
                    }
                    _ddpmHomePageViewModel.cxItem = realWidth;
                    DataContext = _ddpmHomePageViewModel;
                }));
            }
            dispTimer.Stop();
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

            return justifyMinMaxWidth(sizeItem);
        }

        private double CalculateItemWidthV3_ItemsPerRow2(double cxView, double cyView, double devCount = 2)
        {
            double rowCount = devCount / 2;
            //Robert_Lin, 2024-10-1 Special for huge monitor (4K)
            //When screen resolution is very large, the ratio to gap to batteryIndicator is very large
            //
            double gapRatio = 1;
            if (cxView >= 2200)
            {
                DdpmCommonHelper.WriteUILog($"[CalculateItemWidthV3_ItemsPerRow2] Screen resolution >= 2200 trigger gapRatio*2.");
                gapRatio = 2.0;
            }
            double cxItem = (cxView - minGap * 3.000 * gapRatio) / 2.000;
            double cyItem = (cyView - minGap * gapRatio) / rowCount;
            double sizeItem = Math.Min(cxItem, cyItem - cyBatteryIndicator * 2 * gapRatio);
            if(_ddpmHomePageViewModel != null)
                _ddpmHomePageViewModel.OrderingWidth = justifyMinMaxWidth(sizeItem) * 2 + minGap * 3;
            // 1.16 for view item size
            return sizeItem;
        }

        private double CalculateItemWidthV3_ItemsPerRow3(double cxView, double cyView, double devCount = 3)
        {
            double rowCount = devCount / 3;
            rowCount += devCount % 3 == 0 ? 0 : 1;

            //Robert_Lin, 2024-10-1 Special for huge monitor (4K)
            double hugeReduce = 0;
            if (cxView >= 2200)
            {
                DdpmCommonHelper.WriteUILog($"[CalculateItemWidthV3_ItemsPerRow3] Screen resolution >= 2200 trigger hugeReduce -300px.");
                hugeReduce = 100;
            }
            double cxItem = (cxView - minGap * 4.000) / 3.000;
            double cyItem = (cyView - minGap) / rowCount;
            double sizeItem = Math.Min(cxItem, cyItem - cyBatteryIndicator * 2 - hugeReduce * 3);

            if (_ddpmHomePageViewModel != null)
                _ddpmHomePageViewModel.OrderingWidth = justifyMinMaxWidth(sizeItem) * 3 + minGap * 4;
            return sizeItem;
        }

        private double CalculateItemWidthV3_ItemsPerRow4(double cxView, double cyView, double devCount = 4)
        {
            double rowCount = devCount / 4;
            rowCount += devCount % 4 == 0 ? 0 : 1;

            //Add margin in cxItem to avoid internal margin
            double cxItem = (cxView - (minGap * 5.000)) / 4.000;
            double cyItem = (cyView - minGap) / rowCount;
            double sizeItem = Math.Min(cxItem, cyItem - cyBatteryIndicator * 2);
            if (_ddpmHomePageViewModel != null)
                _ddpmHomePageViewModel.OrderingWidth = justifyMinMaxWidth(sizeItem) * 4 + minGap * 5;
            return sizeItem;
        }

        private void rootUserControl_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            RefreshListViewItemWidth();
        }

        private double justifyMinMaxWidth(double originalWidth)
        {
            if (originalWidth < 250)
                return 250;
            if (originalWidth > 500)
                return 500;
            return originalWidth;
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
                        //Robert_Lin 2025-3-27 to log all user's action (mouse click)
                        _ddpmHomePageViewModel?.Log.Info($"HomeDevice is clicked, Category=[{dev.DeviceCategory}], DisplayName=[{dev.DisplayName}]");
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
                _ddpmHomePageViewModel?.Log.Info("Calling to ShowPluginById(DisplayPluginId)");
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
                DdpmCommonHelper.WriteUILog($"Homepage Show Keyboard Landing Page timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
                _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.KeyboardPluginId, selectedHomeDevice.DeviceInfo.ID.ToString() + instanceNo);
            }
            if (selectedHomeDevice?.DeviceCategory == eDeviceCategory.Mouse)
            {
                //Check if it's fake device
                if (selectedHomeDevice.DeviceInfo == null)
                    return;

                IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                DdpmCommonHelper.WriteUILog($"Homepage Show Mouse Landing Page timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
                _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.MousePluginId, selectedHomeDevice.DeviceInfo.ID.ToString() + instanceNo);
            }
            // 240722 Added by Hess to show Pen landing page
            if (selectedHomeDevice?.DeviceCategory == eDeviceCategory.Pen)
            {
                if (selectedHomeDevice.DeviceInfo == null)
                    return;
                IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                DdpmCommonHelper.WriteUILog($"Homepage Show Pen Landing Page timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
                _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.PenPluginId, selectedHomeDevice.DeviceInfo.ID.ToString() + instanceNo);
            }
            // jim add 20240613
            if (selectedHomeDevice?.DeviceCategory == eDeviceCategory.Webcam)
            {
                //Check if it's fake device
                if (selectedHomeDevice.DeviceInfo == null)
                    return;

                IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                DdpmCommonHelper.WriteUILog($"Homepage Show Webcam Landing Page timestamp: {DateTime.Now:hh:mm:ss.ffffff}");
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
            //20250313 Wayn 新增AirAudio UI
            if (selectedHomeDevice?.DeviceCategory == eDeviceCategory.AirAudio)
            {
                //Check if it's fake device
                if (selectedHomeDevice.DeviceInfo == null)
                    return;

                IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.AirAudioPluginId, selectedHomeDevice.DeviceInfo.ID.ToString() + instanceNo);
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
            //0211 Bruce 新增Bootloader UI
            if (selectedHomeDevice?.DeviceCategory == eDeviceCategory.Bootloader)
            {
                //Check if it's fake device
                if (selectedHomeDevice.DeviceInfo == null)
                    return;

                IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.BootloaderPluginId, selectedHomeDevice.DeviceInfo.ID.ToString());
            }
            //0415 Wayne 新增RTKHUB UI
            if (selectedHomeDevice?.DeviceCategory == eDeviceCategory.RtkHub)
            {
                //Check if it's fake device
                if (selectedHomeDevice.DeviceInfo == null)
                    return;

                IShowPluginManager? _showPluginManager = DdpmHomePlugin.PluginIoc.GetService<IShowPluginManager>();
                _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.RtkHubPluginId, selectedHomeDevice.DeviceInfo.ID.ToString());
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
                _ddpmHomePageViewModel.AddFakeMonitorToListView();
                //int idx = _ddpmHomePageViewModel.HomeDevices.Count;
                //MonitorInfo mi = GetFakeMonitorInfo();
                //mi.Index = idx;
                //_ddpmHomePageViewModel.PrepareMonitorInfos(new List<MonitorInfo> { mi });
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
                DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_KB900.png"),
                DeviceInfo = new DeviceInfo()
                {
                    Name = "KB900"
                }
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
                DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_Mouse.png"),
                DeviceInfo = new DeviceInfo()
                {
                    Name = "Mouse"
                }
            };
            _ddpmHomePageViewModel.AddDemoHomeDevice(demo);
            RefreshListViewItemWidth();
        }
        private void addAirAudio_Click(object sender, RoutedEventArgs e)
        {
            int id = _ddpmHomePageViewModel.HomeDevices.Count;

            HomeDevice demo = new HomeDevice()
            {
                DeviceCategory = eDeviceCategory.AirAudio,
                DeviceName = $"Demo {id}",
                DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/SB725.png"),
                DeviceInfo = new DeviceInfo()
                {
                    Name = "AirAudio"
                }
            };
            _ddpmHomePageViewModel.AddDemoHomeDevice(demo);
            RefreshListViewItemWidth();
        }
        private void addAllTest_Click(object sender, RoutedEventArgs e)
        {
            AddAll();
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

        #region Read/Write INI file - Move to DDPM.UI.Common/User32.cs, can be removed
        /*
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
        */
        #endregion Read/Write INI file

        #region Connection Hover View

        //private ConnectionHoverView? _connHoverView = null;

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

            //Robert_Lin, 2024-11-29 for test BatteryIndicator LightMode
            //bi.ChangeToNextBatteryStatus();
        }

        #endregion Connection Hover View

        private void RenderingDone()
        {
            if (_ddpmHomePageViewModel != null)
            {
                _ddpmHomePageViewModel.Log.Info($"DdpmHomePage.RenderingDown() is called. MemoryUsage={DdpmCommonHelper.GetProcessMemoryUsageMB():F2} MB");
            }
            //RefreshListViewItemWidth();

            //System.Windows.Threading.DispatcherTimer dispTimer = new System.Windows.Threading.DispatcherTimer();
            //dispTimer.Tick += delegate
            //{
            //    dispTimer.Stop();
            //    RefreshListViewItemWidth();
            //};
            //dispTimer.Interval = new TimeSpan(200);
            //dispTimer.Start();
        }

        [Obsolete("This method is obsolete. Moved to WalkThroughPage.xaml before app walk through.")]
        private void _ddpmHomePageViewModel_ShowConsent(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
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
                    //_ = DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_EnableTelemetryConsent(true).Result;
                    DdpmCommonHelper.Set_GlobalSettings(DdpmCommonHelper.GlobalSettingsType.Consent, true);
                }
                else
                {
                    //_ = DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_EnableTelemetryConsent(false).Result;
                    DdpmCommonHelper.Set_GlobalSettings(DdpmCommonHelper.GlobalSettingsType.Consent, false);
                }
            });
        }

        private void Pairing(object sender, System.Windows.Input.StylusDownEventArgs e)
        {
            if (sender is System.Windows.Controls.ListView listView)
            {
                //var listView = sender as ListView;             
                // Find the ItemsPresenter (or the ScrollViewer that hosts it)
                var scrollViewer = FindVisualChild<ScrollViewer>(listView);
                if (scrollViewer == null)
                    return;

                // Get the bounds of the ScrollViewer (occupied area)
                var scrollViewerPosition = scrollViewer.TransformToAncestor(listView).Transform(new System.Windows.Point(0, 0));
                var scrollViewerBounds = new Rect(scrollViewerPosition, new System.Windows.Size(scrollViewer.ActualWidth, scrollViewer.ActualHeight));

                // Get the position of the Stylusvar stylusPosition = e.GetPosition(listView);             // Check if the StylusDown occurred outside the ItemsPanel areaif (!scrollViewerBounds.Contains(stylusPosition)) { MessageBox.Show("StylusDown occurred outside the ItemsPanel!"); } else { MessageBox.Show("StylusDown occurred inside the ItemsPanel."); } e.Handled = true; // Mark the event as handled            }
                //var listView = sender as System.Windows.Controls.ListView;
                // Get the bounds of the ScrollViewer (occupied area)
                // var scrollViewerPosition = scrollViewer.TransformToAncestor(listView)                                                    .Transform(new Point(0, 0));             var scrollViewerBounds = new Rect(scrollViewerPosition,                                               new Size(scrollViewer.ActualWidth, scrollViewer.ActualHeight));             // Get the position of the Stylus
                var stylusPosition = e.GetPosition(listView);
                // Check if the StylusDown occurred outside the ItemsPanel area
                if (!scrollViewerBounds.Contains(stylusPosition))
                {
                    MessageModalDialog messageModalDialog;
                    Window parentWindow = Window.GetWindow(this);
                    if (_ddpmHomePageViewModel!.IsPandoraPaired)
                    {
                        messageModalDialog = new(Strings.Error, Strings.PenAlreadyPaired, Strings.Cancel);
                        if (parentWindow != null)
                        {
                            messageModalDialog.Owner = parentWindow;
                        }
                        messageModalDialog.ShowDialog();
                        return;
                    }
                    messageModalDialog = new(Strings.PairYourPen, Strings.PairYourPenMessage, Strings.No, Strings.Yes);
                    if (parentWindow != null)
                    {
                        messageModalDialog.Owner = parentWindow;
                    }
                    if (messageModalDialog.ShowDialog()!.Value)
                    {
                        DdpmCommonHelper.DeviceManagerSA!.PairingPen();
                    }
                }
                else
                {

                }
                e.Handled = true;
            }
        }

        private static T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T t)
                    return t;

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        private void AddAll()
        {
            _ddpmHomePageViewModel?.AddFakeMonitorToListView();
            int id = _ddpmHomePageViewModel?.HomeDevices.Count ?? 0;
            HomeDevice demo = new HomeDevice()
            {
                DeviceCategory = eDeviceCategory.KB,
                DeviceName = $"Demo {id}",
                //DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/KB900.png"),
                DeviceInfo = new DeviceInfo()
                {
                    Name = "KB900",
                    ModelNumber = "KB900"
                }
            };
            _ddpmHomePageViewModel?.AddDemoHomeDevice(demo);
            id++;
            demo = new HomeDevice()
            {
                DeviceCategory = eDeviceCategory.Mouse,
                DeviceName = $"Demo {id}",
                //DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/MS300.png"),
                DeviceInfo = new DeviceInfo()
                {
                    Name = "MS300",
                    ModelNumber = "MS300"
                }
            };
            _ddpmHomePageViewModel?.AddDemoHomeDevice(demo);
            id++;
            demo = new HomeDevice()
            {
                DeviceCategory = eDeviceCategory.Soundbar,
                DeviceName = $"Demo {id}",
                //DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/SB522A.png"),
                DeviceInfo = new DeviceInfo()
                {
                    Name = "SB522A",
                    ModelNumber = "SB522A"
                }
            };
            _ddpmHomePageViewModel?.AddDemoHomeDevice(demo);
            id++;
            demo = new HomeDevice()
            {
                DeviceCategory = eDeviceCategory.Headset,
                DeviceName = $"Demo {id}",
                //DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/WH3024.png"),
                DeviceInfo = new DeviceInfo()
                {
                    Name = "WH3024",
                    ModelNumber = "WH3024"
                }
            };
            _ddpmHomePageViewModel?.AddDemoHomeDevice(demo);
            id++;
            demo = new HomeDevice()
            {
                DeviceCategory = eDeviceCategory.AirAudio,
                DeviceName = $"Demo {id}",
                //DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/WH3024.png"),
                DeviceInfo = new DeviceInfo()
                {
                    Name = "SB725",
                    ModelNumber = "SB725"
                }
            };
            _ddpmHomePageViewModel?.AddDemoHomeDevice(demo);
            id++;
            demo = new HomeDevice()
            {
                DeviceCategory = eDeviceCategory.Webcam,
                DeviceName = $"Demo {id}",
                //DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/WB3023.png"),
                DeviceInfo = new DeviceInfo()
                {
                    Name = "WB3023",
                    ModelNumber = "WB3023"
                }
            };
            _ddpmHomePageViewModel?.AddDemoHomeDevice(demo);
            id++;
            demo = new HomeDevice()
            {
                DeviceCategory = eDeviceCategory.Pen,
                DeviceName = $"Demo {id}",
                //DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/PN5122W.png"),
                DeviceInfo = new DeviceInfo()
                {
                    Name = "PN5122W",
                    ModelNumber = "PN5122W"
                }
            };
            _ddpmHomePageViewModel?.AddDemoHomeDevice(demo);
        }

        private void AttachImportNotification()
        {
            _ddpmHomePageViewModel.ImportNotify -= ImportNotifyEventHandler;
            if (_ddpmHomePageViewModel != null && _ddpmHomePageViewModel.ImportNotify == null
                || !_ddpmHomePageViewModel.ImportNotify.GetInvocationList().Any(e => e.Method.Name == nameof(ImportNotifyEventHandler)))
            {
                _ddpmHomePageViewModel.ImportNotify += ImportNotifyEventHandler;
            }
        }

        private UXFlyout? _flyout1 = null;

        private void UXButton_Click(object sender, RoutedEventArgs e)
        {
            if (_flyout1 == null)
            {
                VbarItem1 vbarItem = new VbarItem1()
                {
                    Index = 0,
                    //Text = mg.GroupName, //Robert_Lin,2024-7-26, GroupName is ID used to identify a Group
                    Text = "VbarItem1 Text",      // VbarText is the display string on VbarItem
                    //IconTemplate = mg.IconTemplate
                    IconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.DisplaySettings)
                };
                UXButton uxBtn = (UXButton)sender;
                _flyout1 = new UXFlyout();
                _flyout1.PlacementTarget = uxBtn;
                _flyout1.Child = vbarItem;
                _flyout1.Placement = PlacementArea.Bottom;
                _flyout1.Margin = new Thickness(0, 5, 0, 0);
                _flyout1.IsOpen = true;
            }
            else
            {
                _flyout1.IsOpen = !_flyout1.IsOpen;
            }
        }

        private void _DDPMThemeChange(OSThemeEnum oSThemeEnum)
        {
            ObservableCollection<HomeDevice>? homeDevices = _ddpmHomePageViewModel?.HomeDevices;
            if (homeDevices != null && homeDevices.Count > 0)
            {
                foreach (HomeDevice homeDevice in homeDevices)
                {
                    if (homeDevice.MonitorInfo != null)
                    { 
                        homeDevice.FetchMonitorImage();
                    }

                    if (homeDevice.DeviceInfo != null)
                    {
                        homeDevice.FetchPeripheralDeviceImage();
                    }
                }
            }
        }

        ~DdpmHomePage()
        {
            _ddpmHomePageViewModel.ImportNotify -= ImportNotifyEventHandler;
        }

        #region Unused
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
        #endregion

        //private UXFlyout? _flyout1 = null;
        //private void UXButton_Click(object sender, RoutedEventArgs e)
        //{
        //    IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
        //    console?.ShowPluginById(DDPM.UI.Common.Constants.ExitAppPluginId);

            //if (_flyout1 == null)
            //{
            //    VbarItem1 vbarItem = new VbarItem1()
            //    {
            //        Index = 0,
            //        //Text = mg.GroupName, //Robert_Lin,2024-7-26, GroupName is ID used to identify a Group
            //        Text = "VbarItem1 Text",      // VbarText is the display string on VbarItem
            //        //IconTemplate = mg.IconTemplate
            //        IconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.DisplaySettings)
            //    };
            //    UXButton uxBtn = (UXButton)sender;
            //    _flyout1 = new UXFlyout();
            //    _flyout1.PlacementTarget = uxBtn;
            //    _flyout1.Child = vbarItem;
            //    _flyout1.Placement = PlacementArea.Bottom;
            //    _flyout1.Margin = new Thickness(0, 5, 0, 0);
            //    _flyout1.IsOpen = true;
            //}
            //else
            //{
            //    _flyout1.IsOpen = !_flyout1.IsOpen;
            //}


        //}
    }
}