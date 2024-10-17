
using DDPM.SA.Common;
using DDPM.SA.Common.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Interfaces;
using VcpCore.Common;

namespace DDPM.EABroker
{
    public class EABroker
    {
        private AwsWindow _awsWindow;
        private EAEditWindow _editWindow;
        private SaveCustomWindow _saveCustomWindow;
        private InfoWindow _infoWindow;
        private readonly IAgent _agent;
        private ILog? _log = null;
        private readonly IDeviceManagerSA _deviceManagerSA;
        private readonly IEasyArrangeService _easyArrangeService;
        private readonly ArrangeVM _vm = new ArrangeVM();

        public EABroker(IAgent agent, IDeviceManagerSA deviceManager, IDisplayService displayService, IEasyArrangeService easyArrangeService)
        {
            _agent = agent;
            _log = agent.CreateLogger("EABroker",typeof(EABroker));
            _deviceManagerSA = deviceManager;
            _easyArrangeService = easyArrangeService;

            _vm.InitInterfaces(agent, _log, deviceManager, displayService, easyArrangeService);
            WriteLog("EABroker is constructed.");
        }

        public void Start()
        {
            WriteLog("@EABroker.Start()");
            //Init InfoWindow, EAEditWindow, SaveCustomWindow 
            InitAllWindows(); 
            //Init EAWorkWindows
            _vm.InitWorkWindows();
            //Init AwsWindow
            _vm.InitAwsWindow();
        }

        public void Stop()
        {

        }

        private void WriteLog(string msg, Exception? e=null)
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

        public ArrangeVM VM { get { return _vm; } }
        public EAEditWindow EditWindow { get { return _editWindow; } }

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

        public void SetWorkSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings = null)
        {
            EAWorkWindow? workWindow = _vm.FindWorkWindowByMonitor(monitorInfo);
            if (workWindow != null)
            {
                workWindow.SetWorkingSplit(cellCount, splitKey, settings);
            }
        }

        /*
        public EventHandler<string>? EditSave;

        public EventHandler<string>? EditCancel;

        private void saveCustomWidow_CancelButtonClick(object sender, string e)
        {

            if (EditReturn != null)
            {
                EAArgs retArgs = new EAArgs(_eaArgs);
                retArgs.Result = false;
                retArgs.Command = "EditReturn";
                retArgs.Message = "User cancel the editing.";
                EditReturn(this, retArgs);
            }
            _editWindow.InvokeClose();
            _saveCustomWindow.Hide();

            _vmArrange.IsWorkUIEnabled = true;
        }

        private void saveCustomWidow_SaveButtonClick(object sender, string e)
        {
            //if (EditCompleted != null)
            //    EditCompleted(this, "");

            if (EditReturn != null)
            {
                EAArgs retArgs = new EAArgs(_eaArgs);
                retArgs.Result = true;

                retArgs.Settings = _editWindow.GetSettings();
                retArgs.CustomName = e;
                retArgs.Command = "EditReturn";
                EditReturn(this, retArgs);
            }
            _editWindow.InvokeClose();
            _saveCustomWindow.Hide();
            _vmArrange.IsWorkUIEnabled = true;
        }
        */
    }

}
