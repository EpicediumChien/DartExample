
using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Interfaces;
using DDPM.Win32Lib;
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
        //Derek 2025/04/01
        //private AwsWindow? _awsWindow = null;
        //private EAEditWindow? _editWindow = null;
        //private SaveCustomWindow? _saveCustomWindow = null;
        private InfoWindow? _infoWindow = null;
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
            _settingsManager = settingsManager;

            WriteLog("EABroker is constructed.");
        }
        #endregion ctor

        #region Starting
        public void Start()
        {
            WriteLog("@EABroker.Start()");
            //Init InfoWindow, EAEditWindow, SaveCustomWindow 
            //InitAllWindows(); //Derek Change function name to InitInfoWindow
            InitInfoWindow();
            //Init EAWorkWindows
            _vm.InitWorkWindows();
            //Init AwsWindow
            _vm.InitAwsWindow();
            _isEaBrokerStarted = true;
            RunningState = eEARunningStates.Waiting;
        }
        private void InitInfoWindow()
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

                        //Robert_Lin 2025-3-19, the ScreenIdWindows are not used in DDPM v2.0.1
                        //Remoarked below, don't create them.
                        //_vm.InitScreenIdWindows();
                        if (_vm!=null)
                        {
                            _vm.RefreshScreenScale();
                            ISplitCtrl.ScreenScale = _vm.ScreenScale;
                        }
                    }
                    catch (Exception exIn)
                    {
                        WriteLog("EXCEPTION when new InfoWindow", exIn);
                    }
                }

                added++;

                //Robert_Lin 2025-3-19, InfoWindow.Window_Closing() will call InvokeShutdown() to exit from Run() loop.
                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            while (added <= 0)
            {
                Task.Delay(10).Wait();
            }
        }
        #endregion

        #region Exiting
        //Reverse function of EABroker.Start()
        public void Stop()
        {
            //Reverse of _vm.InitWorkWindows(), _vm.InitAwsWindow()
            if (_vm != null)
                _vm.Exit();

            //Reverse of InitAllWindows()
            if (_infoWindow != null)
            {
                _infoWindow.Close();
                _infoWindow = null;
            }

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
            {
                WriteLog($"[EABroker] ReloadEzSettingsFromUserSettingsFile by NotifySettingsManagerIsInitializedDone"); //add for debug
                _vm.ReloadEzSettingsFromUserSettingsFile();
            }
        }

        //Derek 2025/0401 due to reference = 0
        //public void TestForRobert_EzArrange()
        //{
        //    MonitorInfo moinfo = new MonitorInfo();

        //    _ = _deviceManagerSA.CheckEAIDExit( moinfo, 0);
        //    _ = _deviceManagerSA.DeleteEAID(moinfo, 0);
        //}

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
 
                //2 Refresh settings
                _vm.DetectSpanCondition();
                //3 Check if changed
                bool newSpanEnabled = _vm.IsSpanEnabled;

                //4 Notify to UI if it's changed
                if (newSpanEnabled != orgSpanEnabled && _deviceManagerSA != null)
                {
                    EAArgs eAArgs = new EAArgs();
                    eAArgs.Command = EAEMConstants.EACommand_SetIsSpanEnabled;
                    eAArgs.Result = newSpanEnabled;
                    _ = _deviceManagerSA.SendEANotify(eAArgs);
                }

                _vm.WriteLog("@EABroker RefreshWorkWindows call from Handle_DisplaySettingsChanged()");
                _vm.RefreshWorkWindows(isInit);
            }
        }

        public void Handle_AllInfoMonitorChanged(bool isInit = false)
        {
            if (_vm != null)
            {
                _vm.WriteLog("@EABroker.Handle_AllInfoMonitorChanged()");
                //Check for Span across multiple monitors
                //
                //1 Save original settings
                bool orgSpanEnabled = _vm.IsSpanEnabled;                

                //2 Refresh settings
                _vm.DetectSpanCondition();
                //3 Check if changed
                bool newSpanEnabled = _vm.IsSpanEnabled;

                //4 Notify to UI if it's changed
                if (newSpanEnabled != orgSpanEnabled &&
                    _deviceManagerSA != null)
                {
                    EAArgs eAArgs = new EAArgs();
                    eAArgs.Command = EAEMConstants.EACommand_SetIsSpanEnabled;
                    eAArgs.Result = newSpanEnabled;
                    _ = _deviceManagerSA.SendEANotify(eAArgs);
                }

                WriteLog($"[ArrangeVM] RefreshWorkWindows call from Handle_AllInfoMonitorChanged");
                _vm.RefreshWorkWindows(isInit);
            }
        }
        public void NotifySelectedMonitorChanged()
        {
            _vm.NotifySelectedMonitorChanged();
        }
        
        //Derek 2025/03/31
        private void ExitUIThread()
        {
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }

        public bool STA_LaunchAndArrangeAppsWithEzArrange(Dictionary<String, Bind_AddFullPage_AppCollectionData> sortApps, 
                                                            MonitorInfo moInfo, int eaId)
        {
            if (_deviceManagerSA == null)
            {
                WriteLog("@STA_LaunchAndArrangeAppsWithEzArrange(), _deviceManagerSA is null.");

                return false;
            }


            //Phase A. Determine the WorkingArea of the arrange
            Rectangle workingArea = Rectangle.Empty;
            //targetScreen: the target screen to be arranged. it's null when IsSpanScreenWorking is true.
            Screen? targetScreen = null;
            // 1 If it's under SpanScreen working mode
            if (_vm.IsSpanScreenWorking)
            {
                //Check if the moInfo is included in the SpanScreen
                if (_vm.SpanScreen.IsExistScreen(moInfo.DisplayName))
                {
                    //The WorkingArea is the SpanScren rect
                    workingArea = _vm.SpanScreen.WorkingArea;
                    WriteLog($"@LaunchAndArrange, SpanScreenEnabled=True, IncludeTargetMonitor=Yes, WorkingArea={CommonFunctions.FormatRectangle(workingArea)}");
                }
                else
                {
                    WriteLog("@LaunchAndArrange, SpanScreenEnabled=True, IncludeTargetMonitor=No");
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
                targetScreen = scr;
                workingArea = scr.WorkingArea;
                WriteLog($"@LaunchAndArrange, SpanScreenEnabled={_vm.IsSpanScreenWorking}, WorkingArea={CommonFunctions.FormatRectangle(workingArea)}");
            }

            bool _isVertical = workingArea.Width < workingArea.Height;
            WriteLog($"@LaunchAndArrange, IsVertical={_isVertical}");
            
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
            //WriteLog($"LaunchAndArrangeAppsWithEzArrange: Layout={ispLayout.CtrlClass} CellBorderCount={cellBorderCount}, AppCount={appCount} => ArrangeCount={arrangeCount}");

            //Robert_Lin, 2025-1-6 Added
            // Before LaunchAppAndArrange starting, minimize all top-level Windows which is inside
            // target screen.
            //1 Get all top-level window handles
            List<IntPtr> hWnds_TopLevel = Win32.GetAltTabWindows();

            //For-loop to find the Window inside target screen, and minimize it
            foreach (IntPtr hWnd in hWnds_TopLevel)
            {
                //Get WindowText, used only for debug time
                string wndText = Win32._GetWindowText(hWnd);

                //Check if the hWnd is inside target screen
                bool isWndInsideTargetScreen = false;
                Screen screenOfhWnd = Screen.FromHandle(hWnd);
                string deviceNmaeOfHwnd = screenOfhWnd.DeviceName;

                if(_vm.IsSpanScreenWorking)
                {
                    foreach(EAScreen eaScr in _vm.SpanScreen.eaScreens)
                    {
                        if (eaScr.FormsScreen.DeviceName.Equals(deviceNmaeOfHwnd))
                        {
                            isWndInsideTargetScreen = true;
                        }
                    }
                }
                else
                {
                    if (targetScreen != null && 
                        targetScreen.DeviceName.Equals(deviceNmaeOfHwnd))
                    {
                        isWndInsideTargetScreen = true;
                    }
                }
                if (!isWndInsideTargetScreen)
                    continue;

                //Minimized this window
                Win32._ShowWindow(hWnd, Win32.ShowWindowCommands.Minimize);
            }


            //
            ///////////////////////

            //Phase C. Show EzMemLauncherWindow
            EzMemLauncherWindow emWin = new EzMemLauncherWindow(ispLayout, workingArea, arrangeCount, VM, _deviceManagerSA);

            //Wayn's v1
            ///*
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
            //*/
            //Robert's v2
            //emWin.LayoutReady += delegate
            //{
            //    //Phase D. 
            //    var sortedApps = sortApps.OrderBy(x => x.Key).Select(x => x.Value).ToList();
            //    int idxCell = 0;
            //    for (int i = 0; i < arrangeCount; i++)
            //    {
            //        //var sortedApps = sortApps.OrderBy(x => i).Select(x => x.Value).ToList();
            //        var app = sortedApps[i];
            //        //Task.Delay(500);
            //        emWin.LaunchAndArrange_v2(app, i, _vm);

            //    }

            //    //int idxCell = 0;
            //    //emWin.LaunchAndArrange(sortApps, idxCell++, VM);
            //};


            emWin.ArrangeDone += delegate
            {
                emWin.Close();
                //VM.IsWorkUIEnabled = true;

                //Robert_Lin, 2025-1-6, After EAM Arrange Done, will chnage cureent selected layout
                //Based on DDM behavior
                if (_easyArrangeService != null)
                {
                    _vm.WriteLog($"@LaunchAndArrangeAppsWithEzArrange. Arrange done, will SetSelectedLayout to EAID={eaId}");
                    _easyArrangeService.SetEASelectedLayout(moInfo, eaId);
                }

                //Derek 2025/03/31
                ExitUIThread();
            };
            emWin.Show();

            //VM.IsWorkUIEnabled = false;
            return true;
        }

    }

}
