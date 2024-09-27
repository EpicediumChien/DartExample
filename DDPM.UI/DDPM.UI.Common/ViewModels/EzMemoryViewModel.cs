using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using VcpCore.Common;

namespace DDPM.UI.Common.ViewModels
{
    public class EzMemoryViewModel : ObservableObject
    {
        private HomeDevice _homeDevice;
        private readonly IDeviceManagerSA _deviceManagerSA;
        private ICommand? _listViewItemClickCommand;
        private ILog _log;
        //public IModuleOwner? ModuleOwner { get; set; }

        public Dictionary<string, List<EzMemoryPageData>> ezPages;
        public int _currentTotalPage = 0;// Control button Visibility.Collapsed 
        public int _currentPageIndex = 0;
        public string _currentDeviceModel = "EzMemory";

        public EzMemoryViewModel(HomeDevice homeDev)
        {
            _homeDevice = homeDev;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;
            _listViewItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);

            _splitItemEditCommand = new RelayCommand<SplitItem>(OnSplitItemEditCommand);
            Invoke_InitData();
        }

        private double _progressValue = 1;
        public double ProgressValue
        {
            get
            {
                return _progressValue;
            }
            set
            {
                _progressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }

        private int _currentAnimationPage;
        public int CurrentAnimationPage
        {
            get => _currentAnimationPage;
            set => SetProperty(ref _currentAnimationPage, value);
        }

        private string _mainText = string.Empty;
        public string MainText
        {
            get => _mainText;
            set => SetProperty(ref _mainText, value);
        }

        private string _subText = string.Empty;
        public string SubText
        {
            get => _subText;
            set => SetProperty(ref _subText, value);
        }

        public void OnListViewItemClicked(SplitItem spItem)
        {
            SelectedSplitItem = spItem;
        }
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

                }
                else
                {
                    //Selection none
                    SetProperty(ref _selectedSplitItem, value);
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

        #region Screen Orientation
        private bool _isVertical = false;
        public bool IsVertical
        {
            get => _isVertical;
            set => SetProperty(ref _isVertical, value);
        }
        #endregion

        public Dictionary<string, List<EzMemoryPageData>> GetEzPages()
        {
            return new Dictionary<string, List<EzMemoryPageData>>
            {
                { "EzMemory", new List<EzMemoryPageData>
                    {
                        new EzMemoryPageData { MainText = "Easy Memory", SubText = "Save different profiles and restore them manually, by scheduled time or at system start-up.\r\n\r\nBegin by assigning a name to your Easy Memory Profile and selecting a layout."},
                        new EzMemoryPageData { MainText = "Assign programs", SubText = "Assign applications/documents to windows or drag the application icon to the respective partition.\r\n\r\nNote: Easy Arrange Memory usability may vary according to application type and launch behavior."},
                        new EzMemoryPageData { MainText = "Launch options", SubText = "Select a launch type"}
                    }
                },

            };
        }
    }
    public class EzMemoryPageData
    {
        public string? MainText { get; set; }
        public string? SubText { get; set; }
    }

}
