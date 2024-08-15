using CommunityToolkit.Mvvm.Input;
using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Plugin.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.EzArrange
{
    /// <summary>
    /// Interaction logic for EzArrangeRightVierw.xaml
    /// </summary>
    public partial class EzArrangeRightVierw : UserControl
    {
        private HomeDevice _homeDevice;
        private IDeviceManagerSA _deviceManagerSA;
        private DDPM.UI.Common.ViewModels.EzArrangeViewModel _vm;
        private readonly DisplayViewModel _vmDisplay;
        private readonly IConsole _console;
        private readonly ILog _log;

        public EzArrangeRightVierw(DisplayViewModel vmDisplay)
        {
            Requires.NotNull(vmDisplay, nameof(vmDisplay));

            InitializeComponent();

            _vmDisplay = vmDisplay;
            _homeDevice = vmDisplay.SelectedHomeDevice;
            _console = vmDisplay.Console;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;

            if (_homeDevice.vmEzArrange == null)
            {
                _homeDevice.vmEzArrange = new DDPM.UI.Common.ViewModels.EzArrangeViewModel(_homeDevice);
            }
            _vm = _homeDevice.vmEzArrange;
            DataContext = _homeDevice.vmEzArrange;

            splitListView_Recent.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_4w.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_2w.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            //Set the ListView ClickCommand Handler
            //Set the ListView ClickCommand Handler
            // _vm.ListViewItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);

            splitListView_2w.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);
            splitListView_4w.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);

            InitRecentListView();
            InitListViewItems();
        }

        private void _deviceManagerSA_EAEditStarted1(object? sender, string e)
        {
            throw new NotImplementedException();
        }

        private void InitSplitListViews()
        {
            foreach (ISplitCtrl isp in ISplitCtrl.Splits_EA)
            {
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (HomeDevice.DeviceManagerSA != null)
            {
                //HomeDevice.DeviceManagerSA.SetEAFunctionEnabled(false);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //InitRecentListView();
            //InitListViewItems();
        }

        private void InitRecentListView()
        {
            ISplitCtrl? sp0A = ISplitCtrl.Create(0, 'A');
            if (sp0A != null)
                splitListView_Recent.AddItemToList(sp0A.UC);
        }

        private void InitListViewItems()
        {
            foreach (ISplitCtrl spCtrl in ISplitCtrl.Splits_EA)
            {
                SplitItem? spItem = null;
                ISplitCtrl newSplit = spCtrl.New();
                switch (newSplit.CellCount)
                {
                    case 2:
                        spItem = splitListView_2w.AddItemToList(newSplit.UC);
                        break;

                    case 4:
                        spItem = splitListView_4w.AddItemToList(newSplit.UC);
                        break;

                    default:
                        break;
                }
                if (spItem != null)
                {
                    spItem.IsEditEnabled = true;
                }
            }
        }

        private void OnListViewItemClicked(SplitItem spItem)
        {
            if (spItem.InnerContent is ISplitCtrl)
            {
                ISplitCtrl spCtrl = spItem.InnerContent as ISplitCtrl;
                _vm.SelectedSplitItem = spItem;
                _vm.SetWorkSplit(spCtrl.CellCount, spCtrl.SplitKey);

                //_deviceManagerSA.WriteEasyArrangeSettings()
            }
        }

        /// <summary>
        /// The event handler when the 'pencil' icon is clicked on the SplitItem
        /// </summary>
        /// <param name="spItem"></param>
        private void HandleSplitItemEditCommand(SplitItem spItem)
        {
            if (spItem.InnerContent is ISplitCtrl)
            {
                ISplitCtrl spCtrl = spItem.InnerContent as ISplitCtrl;
                //_vm.SelectedSplitItem = spItem;
                //_vm.SetWorkSplit(spCtrl.CellCount, spCtrl.SplitKey);

                if (_deviceManagerSA != null)
                {
                    //Robert_Lin, 2024-8-4 Use new interface
                    //OLD:
                    //_deviceManagerSA.RequestEditSplit(_homeDevice.MonitorInfo, spCtrl.CellCount, spCtrl.SplitKey,
                    //    spCtrl.FriendlyName, spCtrl.Settings);
                    //_deviceManagerSA.EAEditStarted += _deviceManagerSA_EAEditStarted;
                    //_deviceManagerSA.EAEditCompleted += _deviceManagerSA_EAEditCompleted;
                    //SplitJson spj = SplitJson.CreateFromSplitItem(spCtrl);
                    //_deviceManagerSA.RequestEditSplit(spj);
                    //
                    //NEW:
                    // 1 DDPM.UI call to EAEditCommand(monitorInfo, EAArgs)
                    // 2 DDPM.SA.EAPlugin will handle this command
                    //   If it's not available to handle (for example, it's already in Edit mode)
                    //   then will return false.
                    //   Others will invoke a UI thread to handle the command, and return true.
                    // 3 DDPM.SA (UI thread) will try the create a EditWindow to serve the EditCommand.
                    //   If it failed (for example, invalid argument values)
                    //      it will signal a 'EditStart<string>' event and the <string> contains the error message.
                    //   Otherwise, when it shows the EditWindow
                    //      it will signal a 'EditStart<string> event with <string> is empty ("").
                    //4 DDPM.UI get the EditStart' event and <sting> is empty, then DDPM.UI should be
                    //  minimize itselft (based on UI team's requirements), until a 'EditReturn' event.

                    //Prepare for the EAArgs
                    EAArgs args = new EAArgs()
                    {
                        Command = "EditCommnd",
                        CellCount = spCtrl.CellCount,
                        SplitKey = spCtrl.SplitKey,
                        CustomId = spItem.CustomId,
                        CustomName = spCtrl.FriendlyName,
                        CustomNames = new List<string>(),
                        Settings = spCtrl.Settings
                    };
#if ENABLE_CALL_SA

                    //Register a event handler for EditStarted event
                    _deviceManagerSA.EAEditStarted += _deviceManagerSA_EAEditStarted;
                    bool isSaAccepted = _deviceManagerSA.EAEditCommand(_homeDevice.MonitorInfo, args).Result;
                    if (!isSaAccepted)
                    {
                        //If DDPM.SA.EAPlugin cannot addcept the EditCommand, we will unregister the EditStarted
                        // handler, because, we will never receive this event from DDPM.SA.EAplugin
                        _deviceManagerSA.EAEditStarted -= _deviceManagerSA_EAEditStarted;
                        return;
                    }
#endif
                }
            }
        }

        private void _deviceManagerSA_EAEditCompleted(object? sender, string e)
        {
#if ENABLE_CALL_SA

            if (_deviceManagerSA != null)
            {
                _deviceManagerSA.EAEditCompleted -= _deviceManagerSA_EAEditCompleted;
                if (_console != null)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        _console.RaiseEvent("MainWindow.Activate", this, new EventManagerArgs());
                        //_console.RaiseEvent("MainWindow.Normal", this, new EventManagerArgs());
                        //_console.RaiseEvent("MainWindow.Show", this, new EventManagerArgs());
                    }
                    ));
                 }
            }
#endif
        }

        //DDPM.SA.EAPlugin notify us the result of our previous EditCommand request.
        //1 If e is empty => The EditCommand is accept and the EditWindow is opened.
        //2 If e is not empty => The EditCommand is faled to open the EditWindow, and return the error message in e.
        private void _deviceManagerSA_EAEditStarted(object? sender, string e)
        {
#if ENABLE_CALL_SA
            if (_deviceManagerSA != null)
            {
                //Unregister the event handler now, until the next EditCommand called
                _deviceManagerSA.EAEditStarted -= _deviceManagerSA_EAEditStarted;

                if (!String.IsNullOrEmpty(e)) //If fail to start editing
                {
                    _vm.LogInfo($"EAEditStarted event, fail to Start Edit: {e}");
                    return;
                }
                //Register next event 'EditReturn'
                _deviceManagerSA.EAEditReturn += _deviceManagerSA_EAEditReturn;

                if (_console !=null)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        //What option to DDPM.UI druing EditCommand is processing (when EditWindow is working)?
                        //
                        //_console.RaiseEvent("MainWindow.SetToBottomWindow", this, new EventManagerArgs());
                        _console.RaiseEvent("MainWindow.Minimize", this, new EventManagerArgs());
                        //_console.RaiseEvent("MainWindow.Hide", this, new EventManagerArgs());
                    }
                    ));
                }
            }
#endif
        }

        //Robert_Lin, 2024-8-4
        /// <summary>
        /// Handle the 'EditReturn' message from DDPM.SA.EAPlugin, after EasyArrange EditWindow has finished its job.
        /// We will need to check the EAArgs.Result to determine if user close the EditWindow by 'Save" or 'Cancel' button.
        /// </summary>
        /// <param name="sender">The EAPlugin</param>
        /// <param name="e">
        /// A EAArgs object, most of proparties will be the same with the EAArgs when we calling EAEditCommand().
        /// EAArgs.Result:
        ///   True=User close EditWindow by click 'Save' button. We can get 1) EAArgs.Settings=>New settings,
        ///        2) EAArgs.CustomName => the CustomName which is entered by user.
        ///   False=User close EditWindow by click 'Cancel'. No new settings/values are returned.
        /// </param>
        /// <exception cref="NotImplementedException"></exception>
        private void _deviceManagerSA_EAEditReturn(object? sender, EAArgs e)
        {
#if ENABLE_CALL_SA
            if (_deviceManagerSA != null)
            {
                _deviceManagerSA.EAEditReturn -= _deviceManagerSA_EAEditReturn;
            }
            //Restore DDPM.UI mainwindow.
            if (_console != null)
            {
                _console.RaiseEvent("MainWindow.Normal", this, new EventManagerArgs());
                //_console.RaiseEvent("MainWindow.Activate", this, new EventManagerArgs());
            }

            if (!e.Result)
            {
                return;
            }

            if (e.CustomId == 0)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    ISplitCtrl isp = ISplitCtrl.Create(e.CellCount, e.SplitKey);
                    if (isp != null)
                    {
                        isp.Settings = e.Settings;
                        isp.FriendlyName = e.CustomName;
                    }
                    SplitItem spItem = splitListView_Custom.AddItemToList(isp.UC);
                    spItem.CustomId = 1;
                }));
            }
#endif
        }
    }
}