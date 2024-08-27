#define ENABLE_CALL_SA
//Robert_Lin, 2024-8-14, comment out the #define line if you would like to disable calling to Subagent EAPlugin

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System.ComponentModel;
using System.Windows.Input;
using VcpCore.Common;

namespace DDPM.UI.Common.ViewModels
{
    public class EzArrangeViewModel : ObservableObject
    {
        private HomeDevice _homeDevice;
        private readonly IDeviceManagerSA _deviceManagerSA;
        private ICommand? _listViewItemClickCommand;
        private ILog _log;

        public EzArrangeViewModel(HomeDevice homeDev)
        {
            _homeDevice = homeDev;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;

            //Any item in SplitListView is clicked, will notify to below handler
            //Default handler, but currently it will be handled by RightView
            _listViewItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);

            _splitItemEditCommand = new RelayCommand<SplitItem>(OnSplitItemEditCommand);
            Invoke_InitData();
        }

        private bool _isEaFunctionEnabled;

        public bool IsEaFunctionEnabled
        {
            get => _isEaFunctionEnabled;
            set
            {
                bool res = _deviceManagerSA.SetEAFunctionEnabled(value).Result;
                if (res)
                {
                    _isEaFunctionEnabled = value;
                }
            }
        }

        #region Init Data

        private void Invoke_InitData()
        {
            BackgroundWorker bw = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_InitData;
            bw.RunWorkerCompleted += RunWorkerCompleted_InitData;

            bw.RunWorkerAsync();
        }

        private void DoWork_InitData(object? sender, DoWorkEventArgs e)
        {
#if ENABLE_CALL_SA
            ObjGetVCP ret = _deviceManagerSA.GetEAFunctionEnabled().Result;
            if (ret.result)
                _isEaFunctionEnabled = (bool)ret.value;
#endif
            e.Result = "OK";
        }

        private void RunWorkerCompleted_InitData(object sender, RunWorkerCompletedEventArgs e)
        {
            //If BackgroundWorker. WorkerSupportsCancellation is true, and you set e.Cancel=true in DoWorker
            if (e.Cancelled)
            {
                return;
            }
            if (e.Error != null)
            {
                //The message is e.Error.Message
                return;
            }
            //
            if (e.Result == null)
            {
                //In case that you never set value to e-Result
                //Log?.Info("** RefreshData abnormal stopped unknown reason.");
            }
            else
            {
                //Log?.Info($"** RefreshData result: {e.Result}");

                if (e.Result == "OK")
                {
                    //RefreshPbpSplitListView();
                    //Result is passed.
                    //UI_SetSelectedSplitItemByCurPxpMode();
                }
                else
                {
                    //Result is failed.
                }
            }
        }

        #endregion Init Data

        #region Get/Set to SA

        public void SetWorkSplit(int cellCount, char splitKey, List<double>? settings = null)
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += delegate
            {
#if ENABLE_CALL_SA
                if (_deviceManagerSA != null)
                {
                    bool res = _deviceManagerSA.SetEAWrokSplit(_homeDevice.MonitorInfo, cellCount, splitKey, settings).Result;
                }
#endif
            };
            bw.RunWorkerCompleted += delegate
            {
                //IsBusy = false;
            };
            //IsBusy = true;
            bw.RunWorkerAsync();
        }

        #endregion Get/Set to SA

        #region ListViewItem Click Commands

        public ICommand? ListViewItemClickCommand
        {
            get => _listViewItemClickCommand;
            set => SetProperty(ref _listViewItemClickCommand, value);
        }

        public void OnListViewItemClicked(SplitItem spItem)
        {
            SelectedSplitItem = spItem;
        }

        #endregion ListViewItem Click Commands

        #region Selected Item

        private SplitItem? _selectedSplitItem;

        public SplitItem? SelectedSplitItem
        {
            get => _selectedSplitItem;
            set
            {
                //Unselected origainl selection
                if (_selectedSplitItem != null)
                {
                    //If selected item is the same (no changed)
                    if (_selectedSplitItem == value)
                        return;
                    //Selection changed => unselection origial item
                    _selectedSplitItem.IsSelected = false;
                    if (_selectedSplitItem.Buddy != null)
                    {
                        _selectedSplitItem.Buddy.IsSelected = false;
                    }
                }
                if (value != null)
                {
                    SetProperty(ref _selectedSplitItem, value);
                    _selectedSplitItem.IsSelected = true;
                    if (_selectedSplitItem.Buddy != null)
                        _selectedSplitItem.Buddy.IsSelected = true;

                    //OnPropertyChanged("IsPipItemSelected");
                }
                else
                {
                    //Selection none
                    SetProperty(ref _selectedSplitItem, value);
                    //OnPropertyChanged("IsPipItemSelected");
                }
            }
        }

        #endregion Selected Item

        #region SplitItem Edit Command

        private ICommand? _splitItemEditCommand;

        public ICommand? SplitItemEditCommand
        {
            get => _splitItemEditCommand;
            set => SetProperty(ref _splitItemEditCommand, value);
        }

        public void OnSplitItemEditCommand(SplitItem splitItem)
        {
            //Not handled here, it will be handled by HandleSplitItemEditCommand() in RightView
        }

        #endregion SplitItem Edit Command

        #region Log

        public void CreateLog(IConsole console, string logName)
        {
            _log = console.CreateLog(logName);
        }

        public void LogInfo(string message)
        {
            if (_log != null)
            {
                _log.Info(message);
            }
        }

        #endregion Log
    }
}