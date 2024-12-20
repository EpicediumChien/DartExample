
using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Interfaces;
using System.ComponentModel;
using VcpCore.Common;

namespace DDPM.EABroker
{
    public class EABroker
    {
        #region Private members
        private bool _isEaBrokerStarted = false;
        private AwsWindow _awsWindow;
        private EAEditWindow _editWindow;
        private SaveCustomWindow _saveCustomWindow;
        private InfoWindow _infoWindow;
        private readonly IAgent _agent;
        private ILog? _log = null;
        private readonly IDeviceManagerSA _deviceManagerSA;
        private readonly IEasyArrangeService _easyArrangeService;
        private readonly ArrangeVM _vm = new ArrangeVM();
        private readonly ISettingsManagerDev _settingsManager;
        #endregion  Private members

        #region Public Properties
        public ArrangeVM VM { get { return _vm; } }
        public eEARunningStates RunningState { get; set; } = eEARunningStates.NotAvailable;
        #endregion  Public Properties

        #region ctor
        public EABroker(IAgent agent, IDeviceManagerSA deviceManager, IDisplayService displayService, IEasyArrangeService easyArrangeService, ISettingsManagerDev settingsManager)
        {
            _agent = agent;
            _log = agent.CreateLogger("EABroker", typeof(EABroker));
            _deviceManagerSA = deviceManager;
            _easyArrangeService = easyArrangeService;

            _vm.InitInterfaces(agent, _log, deviceManager, displayService, easyArrangeService, settingsManager);
            WriteLog("EABroker is constructed.");
            _settingsManager = settingsManager;
        }
        #endregion ctor

        #region Starting
        public void Start()
        {
            WriteLog("@EABroker.Start()");
            //Init InfoWindow, EAEditWindow, SaveCustomWindow 
            InitAllWindows();
            //Init EAWorkWindows
            _vm.InitWorkWindows();
            //Init AwsWindow
            _vm.InitAwsWindow();
            _isEaBrokerStarted = true;
            RunningState = eEARunningStates.Waiting;
        }
        private void InitAllWindows()
        {
            int added = 0;
            Thread thread = new Thread(() =>
            {
                /*
                if (_editWindow == null)
                {
                    try
                    {
                        WriteLog("Before new EAEditWindow");
                        _editWindow = new EAEditWindow(_log);
                        WriteLog("After new EAEditWindow");
                        //_editWindow will show in _editWindow.ShowAndEdit()
                        //_editWindow.Show();
                    }
                    catch (Exception exA)
                    {
                        WriteLog("EXCEPTION when new EAEditWindow()", exA);
                    }
                }
                if (_saveCustomWindow == null)
                {
                    try
                    {
                        WriteLog("Before new SaveCustomWindow");
                        _saveCustomWindow = new SaveCustomWindow();
                        WriteLog("After new SaveCustomWindow");
                        if (_editWindow != null)
                        {
                            WriteLog("Setting up SaveCustomWindow");
                            _saveCustomWindow.Owner = _editWindow;
                            _saveCustomWindow.CancelButtonClick += saveCustomWidow_CancelButtonClick;
                            _saveCustomWindow.SaveButtonClick += saveCustomWidow_SaveButtonClick;
                            WriteLog("Setting up SaveCustomWindow - done");
                        }
                        //_saveCustomWindow.Show();
                    }
                    catch (Exception exS)
                    {
                        WriteLog("EXCEPTION when new SaveCustomWindow", exS);
                    }

                }
                */
                if (_infoWindow == null)
                {
                    try
                    {
                        WriteLog("Before new InfoWindow");
                        _infoWindow = new InfoWindow(_vm);
                        WriteLog("After new InfoWindow");
                        _infoWindow.Show();

                        _vm.InitScreenIdWindows();
                    }
                    catch (Exception exIn)
                    {
                        WriteLog("EXCEPTION when new InfoWindow", exIn);
                    }
                }

                added++;

                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            while (added <= 0)
            {
                Thread.Sleep(10);
            }
        }
        #endregion

        #region Exiting
        public void Stop()
        {
            RunningState = eEARunningStates.NotAvailable;
        }
        #endregion

        #region Log
        private void WriteLog(string msg, Exception? e = null)
        {
            if (_vm != null)
                _vm.WriteLog(msg, e);
            else
            {
                if (_log != null)
                {
                    if (e == null)
                    {
                        _log.Info(msg);
                    }
                    else
                    {
                        _log.Error(e, msg);
                    }
                }
            }
        }
        #endregion


        //public EAEditWindow EditWindow { get { return _editWindow; } }


        public void SetWorkSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings = null)
        {
            EAWorkWindow? workWindow = _vm.FindWorkWindowByMonitor(monitorInfo);
            if (workWindow != null)
            {
                workWindow.SetWorkingSplit(cellCount, splitKey, settings);
            }
        }

        public bool NotifyEASelectedLayoutChanged(MonitorInfo monitorInfo, SplitJson spJson)
        {
            return _vm.NotifyEASelectedLayoutChanged(monitorInfo, spJson);
        }

        /// <summary>
        /// Called from EAPlugin, when SettingsManager.SettingReadyEvent is triggered.
        /// </summary>
        public void NotifySettingsManagerIsInitializedDone()
        {
            if (_vm != null)
                _vm.ReloadEzSettingsFromUserSettingsFile();
        }

        public void TestForRobert_EzArrange()
        {
            MonitorInfo moinfo = new MonitorInfo();
            _deviceManagerSA.CheckEAIDExit( moinfo, 0);
            _deviceManagerSA.DeleteEAID(moinfo, 0);
        }

        public void Handle_DisplaySettingsChanged(bool isInit=false)
        {
            if (_vm != null)
            {
                _vm.WriteLog("@EABroker.Handle_DisplaySettingsChanged()");

                //Cancel EditProcess 
                if (RunningState == eEARunningStates.Edit)
                {
                }

                //Check for Span across multiple monitors
                //
                //1 Save original settings
                bool orgSpanEnabled = _vm.IsSpanEnabled;
                bool newSpanEnabled = orgSpanEnabled;
 
                //2 Refresh settings
                _vm.DetectSpanCondition();
                //3 Check if changed
                newSpanEnabled = _vm.IsSpanEnabled;

                //4 Notify to UI if it's changed
                if (newSpanEnabled != orgSpanEnabled)
                {
                    if (_deviceManagerSA != null)
                    {
                        EAArgs eAArgs = new EAArgs();
                        eAArgs.Command = EAEMConstants.EACommand_SetIsSpanEnabled;
                        eAArgs.Result = newSpanEnabled;
                        _deviceManagerSA.SendEANotify(eAArgs);
                    }
                }

                _vm.RefreshWorkWindows(isInit);
            }
        }

        public void Handle_AllInfoMonitorChanged(bool isInit = false)
        {
            if (_vm != null)
            {
                _vm.WriteLog("@EABroker.Handle_DisplaySettingsChanged()");
                //Check for Span across multiple monitors
                //
                //1 Save original settings
                bool orgSpanEnabled = _vm.IsSpanEnabled;                

                //2 Refresh settings
                _vm.DetectSpanCondition();
                //3 Check if changed
                bool newSpanEnabled = _vm.IsSpanEnabled;

                //4 Notify to UI if it's changed
                if (newSpanEnabled != orgSpanEnabled)
                {
                    if (_deviceManagerSA != null)
                    {
                        EAArgs eAArgs = new EAArgs();
                        eAArgs.Command = EAEMConstants.EACommand_SetIsSpanEnabled;
                        eAArgs.Result = newSpanEnabled;
                        _deviceManagerSA.SendEANotify(eAArgs);
                    }
                }

                _vm.RefreshWorkWindows(isInit);
            }
        }
        public void NotifySelectedMonitorChanged()
        {
            _vm.NotifySelectedMonitorChanged();
        }

        public bool STA_LaunchAndArrangeAppsWithEzArrange(Dictionary<String, Bind_AddFullPage_AppCollectionData> sortApps, MonitorInfo moInfo, int eaId)
        {
            if (_deviceManagerSA == null)
            {
                WriteLog("@STA_LaunchAndArrangeAppsWithEzArrange(), _deviceManagerSA is null.");
                return false;
            }

            //Phase A. Determine the WorkingArea of the arrange
            Rectangle workingArea = Rectangle.Empty;
            // 1 If it's under SpanScreen working mode
            if (_vm.IsSpanScreenWorking)
            {
                //Check if the moInfo is included in the SpanScreen
                if (_vm.SpanScreen.IsExistScreen(moInfo.DisplayName))
                {
                    //The WorkingArea is the SpanScren rect
                    workingArea = _vm.SpanScreen.WorkingArea;
                }
            }
            //2 (not) in SpanScreen workng mode, then use the workingArea of moInfo
            if (workingArea.IsEmpty)
            {
                //Get the screen from moInfo
                Screen? scr = Screen.AllScreens.FirstOrDefault(x => x.DeviceName == moInfo.DisplayName);
                if (scr == null)
                {
                    WriteLog("LaunchAndArrangeAppsWithEzArrange ERROR: the Monitor is not a present screen.");
                    return false;
                }
                workingArea = scr.WorkingArea;
            }
            bool _isVertical = workingArea.Width < workingArea.Height;
            //Phase B. Create EA Layout and determine the cellBorderCount
            ISplitCtrl? ispLayout = null;
            int cellBorderCount = 0;

            //B1 Check if it's a custom layout (EAID=[1000~1004])
            if (eaId >= EAEMConstants.EAID_FirstCustom)
            {
                //B2 Load EA CustomList from User settings
                SplitJson[] customList = _deviceManagerSA.ReadEACustomList().Result;
                //B3 if CustomList is empty, then return error
                if (customList == null || customList.Length == 0)
                {
                    WriteLog("LaunchAndArrangeAppsWithEzArrange ERROR: saved custom list is empty.");
                    return false;
                }
                //B4 Find the Custom layout by EAID
                int idxCustom = Array.FindIndex(customList, x => x.EAID == eaId);
                if (idxCustom < 0)
                {
                    WriteLog($"LaunchAndArrangeAppsWithEzArrange ERROR: EAID({eaId}) not found in saved custom list.");
                    return false;
                }

                //B5 We do not support Overlap layout
                if (customList[idxCustom].IsOverlapLayout)
                {
                    WriteLog($"LaunchAndArrangeAppsWithEzArrange ERROR: Layout (EAID={eaId}) is overlap which is not supported.");
                    return false;
                }

                //B5 Create the ISplitCtrl, and determine the cellBorderCount
                int cellCount = customList[idxCustom].CellCount;
                char splitKey = customList[idxCustom].SplitKey;
                ispLayout = ISplitCtrl.Create(cellCount, splitKey);
                if (ispLayout == null)
                {
                    WriteLog($"LaunchAndArrangeAppsWithEzArrange ERROR: Invalid ISplit parameters ({cellCount}{splitKey}) in custom list.");
                    return false;
                }
                if (customList[idxCustom].Settings == null)
                {
                    WriteLog($"LaunchAndArrangeAppsWithEzArrange ERROR: ISplit({cellCount}{splitKey}) Settings is null in saved custom list.");
                    return false;
                }
                //Copy Settings
                ispLayout.Settings = new List<double>(customList[idxCustom].Settings);
            }
            else //Preset layout EAID=[1~49]
            {
                ispLayout = ISplitCtrl.Create(eaId);
                if (ispLayout == null)
                {
                    WriteLog($"LaunchAndArrangeAppsWithEzArrange ERROR: Invalid EAID ({eaId}) for preset layout.");
                    return false;
                }
            }
            ispLayout.IsVertical = _isVertical;
            cellBorderCount = ispLayout.CellList.Count;
            int appCount = sortApps.Count;
            int arrangeCount = Math.Min(cellBorderCount, appCount);
            WriteLog($"LaunchAndArrangeAppsWithEzArrange: Layout={ispLayout.CtrlClass} CellBorderCount={cellBorderCount}, AppCount={appCount} => ArrangeCount={arrangeCount}");

            //Phase C. Show EzMemLauncherWindow
            EzMemLauncherWindow emWin = new EzMemLauncherWindow(ispLayout, workingArea, arrangeCount, _log);
            emWin.LayoutReady += delegate
            {
                //Phase D. 
                //var sortedApps = sortApps.OrderBy(x => x.Key).Select(x => x.Value).ToList();
                //int idxCell = 0;
                //for (int i=0; i< arrangeCount; i++)
                //{
                //    var sortedApps = sortApps.OrderBy(x => i).Select(x => x.Value).ToList();
                //    var app = sortedApps[i];
                //    Task.Delay(500);
                //    emWin.LaunchAndArrange(app, i);

                //}

                int idxCell = 0;
                emWin.LaunchAndArrange(sortApps, idxCell++, VM);
            };

            emWin.ArrangeDone += delegate
            {
                emWin.Close();
                VM.IsWorkUIEnabled = true;
            };
            emWin.Show();

            VM.IsWorkUIEnabled = false;
            return true;
        }

    }

}
