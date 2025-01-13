using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VcpCore.Common;
using Rectangle = System.Drawing.Rectangle;
using System.Windows;
using System.Reflection;
using static System.Windows.Forms.AxHost;
using Dell.Client.Framework.Common;
using System.Windows.Media.Media3D;

namespace DDPM.EABroker
{
    //Robert_Lin, 2024-12-6, Unused. The usage from EzMemoryPlugin, will be removed.
    public class EzMemLauncher_Unused
    {
        #region Private Members
        private string _lastError = string.Empty;
        private int _stage = 0;
        private MonitorInfo? _mi;
        private Screen? _screen = null;
        private ISplitCtrl? _splitCtrl;
        private int _cellBorderCount = 0;
        private EzMemLauncherWindow? _emLauncherWindow;
        private bool _isReadyToArrange = false;
        #endregion

        #region Static Members
        public static IDeviceManagerSA? DeviceManagerSA { get; set; } = null;
        public static ILog? Log { get; set; } = null;

        #endregion
        public EzMemLauncher_Unused()
        {
            
        }

        public string LastError => _lastError;

        public int LaunchStart(MonitorInfo mi, int eaId)
        {
            _isReadyToArrange = false;
            if (DeviceManagerSA == null)
            {
                _lastError = "IDeviceManagerSA is not ready for used.";
                return -1;
            }
            _mi = mi;

            //Invalidate monitorInfo
            //Determine the screen to be arranged
            _screen = Screen.AllScreens.FirstOrDefault(x => x.DeviceName.Equals(mi.DisplayName, StringComparison.OrdinalIgnoreCase));
            if (_screen == null)
            {
                _lastError = $"Cannot find a Screen for the Monito({mi.modelName},{mi.edid.ServiceTag},{mi.DisplayName})";
                return -2;
            }

            _mi = mi;

            //Validate eaId
            if (eaId >= EAEMConstants.EAID_FirstCustom)
            {
                //Load EA CustomList from User settings
                SplitJson[] customList = DeviceManagerSA.ReadEACustomList().Result;

                //Find the Custom layout by EAID
                int idxCustom = Array.FindIndex(customList, x => x.EAID == eaId);
                if (idxCustom < 0)
                {
                    _lastError = $"EAID({eaId}) is not a valid Custom Layout";
                    return -4;
                }
                _cellBorderCount = (int)customList[idxCustom].Settings[0];
            }
            else
            {
                int cellBorderCount = GetCellBorderCountFromEAID(eaId);
                if (cellBorderCount <= 0)
                {
                    _lastError = $"Invalid EAID ({eaId})";
                    return -4;
                }
                _cellBorderCount = cellBorderCount;
            }
            //Invoke a STA thread to continue for UI process
            Thread thread = new Thread(() =>
            {
                STA_LaunchStart(mi, eaId);

                System.Windows.Threading.Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            //Return the count of cellObj
            return _cellBorderCount;

        }

        private void STA_LaunchStart(MonitorInfo mi, int eaId)
        {
            if (_screen == null)
                return;

            //Determine the screen to be arranged
            //Screen? screen = Screen.AllScreens.FirstOrDefault(x => x.DeviceName.Equals(mi.DisplayName, StringComparison.OrdinalIgnoreCase));
            //if (screen == null)
            //{
            //    _lastError = $"Cannot find a Screen for the Monito({mi.modelName},{mi.edid.ServiceTag},{mi.DisplayName})";
            //    return -2;
            //}


            //Stage 2 - Determine the Rect of the Window (Sceen)
            //Robert_Lin, 2024-12-6, use the method in CommonFunctions
            double screenScale = CommonFunctions.GetDpiX();
            //double screenScale = 1.000;
            //var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            //if (dpiXProperty != null)
            //{
            //    var varX = (int)dpiXProperty.GetValue(null, null);
            //    double dpiX = (double)varX / (double)96;
            //    if (dpiX >= 1.0000)
            //        screenScale = dpiX;
            //}

            Rect rcScreen = new Rect();
            rcScreen.X = _screen.WorkingArea.Left / screenScale;
            rcScreen.Y = _screen.WorkingArea.Top / screenScale;
            rcScreen.Width = _screen.WorkingArea.Width / screenScale;
            rcScreen.Height = _screen.WorkingArea.Height / screenScale;

            _stage = 2;

            //Create Window and move the the screen
            //_emLauncherWindow = new EzMemLauncherWindow();

            //The eaId is belong to a custom layout
            if (eaId >= EAEMConstants.EAID_FirstCustom)
            {
                //Load EA CustomList from User settings
                SplitJson[] customList = DeviceManagerSA.ReadEACustomList().Result;

                //Find the Custom layout by EAID
                int idxCustom = Array.FindIndex(customList, x => x.EAID == eaId);
                if (idxCustom < 0)
                {
                    _lastError = $"EAID({eaId}) is not a valid Custom Layout";
                    return ;
                }

                //Create the SplitCtrl
                int cellCount = customList[idxCustom].CellCount;
                char splitKey = customList[idxCustom].SplitKey;

                if (customList[idxCustom].IsOverlapLayout)
                {
                    if (customList[idxCustom].Settings == null)
                    {
                        _lastError = $"No setting in the overlap Custom Layout";
                        return ;
                    }
                    if (customList[idxCustom].Settings.Count <= 4)
                    {
                        _lastError = $"No setting is empty in the overlap Custom Layout";
                        return;
                    }
                    SplitCtrl0B sp0B = new SplitCtrl0B();
                    _splitCtrl = sp0B;
                    _splitCtrl.Settings = new List<double>(customList[idxCustom].Settings);
                    _cellBorderCount = (int)customList[idxCustom].Settings[0];
                    //sp0B.ApplySettingsToCellList(rcScreen);
                }
                else
                {
                    _splitCtrl = ISplitCtrl.Create(cellCount, splitKey);
                    if (_splitCtrl == null)
                    {
                        _lastError = $"Invalid settings of custom layout ({cellCount}{splitKey})";
                        return ;
                    }
                    _splitCtrl.Settings = new List<double>(customList[idxCustom].Settings);
                    _cellBorderCount = _splitCtrl.CellList.Count;
                }
            }
            else
            {
                //A preset layout EAID [1~48]
                _splitCtrl = ISplitCtrl.Create(eaId);
                if (_splitCtrl == null)
                {
                    _lastError = $"Invalid EAID({eaId}) of preset layout.";
                    return;
                }
                _cellBorderCount = _splitCtrl.CellList.Count;
            }

            // [sonarqube] this subsequent code is never executed.
            //if (_splitCtrl == null)
            //{
            //    _lastError = "Fail to create SplitCtrl";
            //    return ;
            //}

            _emLauncherWindow.ShowForEzMemLauncher(mi, _splitCtrl);
            _isReadyToArrange = true;

        }

        //eaid should be [1~49]
        private int GetCellBorderCountFromEAID(int eaid)
        {
            //EAID      CellBorder Count
            //[1~4]     2
            //[5~13]    3
            //[14~19]   4
            //[20~28]   5
            //[29~38]   6
            //[39~42]   7
            //[43~46]   9
            //[47]      8
            //[48]      12
            //[49]      8
            if (eaid < 1)
                return 0;
            if (eaid <= 4)
                return 2;
            if (eaid <= 13)
                return 3;
            if (eaid <= 13)
                return 3;
            if (eaid <= 19)
                return 4;
            if (eaid <= 28)
                return 5;
            if (eaid <= 38)
                return 6;
            if (eaid <= 42)
                return 7;
            if (eaid <= 46)
                return 9;
            if (eaid <= 47)
                return 8;
            if (eaid <= 48)
                return 12;
            if (eaid <= 49)
                return 8;
            return 0;
        }


        public void ArrangeWindow(IntPtr hWnd, int idxCell)
        {
            if (_emLauncherWindow != null && _isReadyToArrange)
            {
                //_emLauncherWindow.ArrangeWindow(hWnd, idxCell);
            }
        }

        public void LaunchEnd()
        {
            if (_emLauncherWindow != null)
            {
                _emLauncherWindow.Dispatcher_Close();
                _emLauncherWindow = null;
                _splitCtrl = null;
            }
        }
    }

}
