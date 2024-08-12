using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace DDPM.UI.Module.DisplayHotkeys
{

    public class InputSourceList
    {
        public string inputSource = String.Empty;
        public DisplayHotkeysModule displayHotkeysModule { get; set; }
        public string inputDisplayText
        {
            get
            {
                return inputSource;
            }
        }
    }

    public class DisplayHotkeysViewModel : ObservableObject
    {
        public IModuleOwner? ModuleOwner { get; set; }
        public DisplayHotkeysModule DisplayHotkeysModule { get; set; }
        public Dictionary<string, InputInfo> inputList { get; set; }

        private string _toggleInputSourceKey = "None";
        public string ToggleInputSourceKey
        {
            get => _toggleInputSourceKey;
            set
            {
                SetProperty(ref _toggleInputSourceKey, value);
                OnPropertyChanged("ToggleInputSourceKey");
            }
        }

        private string _favoriteInputSourceKey = "None";
        public string FavoriteInputSourceKey
        {
            get => _favoriteInputSourceKey;
            set
            {
                SetProperty(ref _favoriteInputSourceKey, value);
                OnPropertyChanged("FavoriteInputSourceKey");
            }
        }
        private string _SwitchInputSourceKey = "None";
        public string SwitchInputSourceKey
        {
            get => _SwitchInputSourceKey;
            set
            {
                SetProperty(ref _SwitchInputSourceKey, value);
                OnPropertyChanged("SwitchInputSourceKey");
            }
        }
        private string _changePIPPositionKey = "None";
        public string ChangePIPPositionKey
        {
            get => _changePIPPositionKey;
            set
            {
                SetProperty(ref _changePIPPositionKey, value);
                OnPropertyChanged("ChangePIPPositionKey");
            }
        }
        private string _swapPIPPBPInputSourceKey = "None";
        public string SwapPIPPBPInputSourceKey
        {
            get => _swapPIPPBPInputSourceKey;
            set
            {
                SetProperty(ref _swapPIPPBPInputSourceKey, value);
                OnPropertyChanged("SwapPIPPBPInputSourceKey");
            }
        }



        private List<InputSourceList> _inputsList = new List<InputSourceList>();
        public List<InputSourceList> InputsList
        {
            get => _inputsList;
            set
            {
                SetProperty(ref _inputsList, value);
                OnPropertyChanged("InputsList");
            }
                
        }



        private InputSourceList _switchInput1Selected = new InputSourceList();
        public InputSourceList SwitchInput1_Selected
        {
            get => _switchInput1Selected;
            set
            {
                SetProperty(ref _switchInput1Selected, value);
                OnPropertyChanged("SwitchInput1_Selected");
                //bool b = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo, "Input Select", _selectInput.inputSource).Result;
            }
        }

        private InputSourceList _switchInput2Selected = new InputSourceList();
        public InputSourceList SwitchInput2_Selected
        {
            get => _switchInput2Selected;
            set
            {
                SetProperty(ref _switchInput2Selected, value);
                OnPropertyChanged("SwitchInput2_Selected");
            }
        }

        private InputSourceList _FavoriteInputSelect = new InputSourceList();
        public InputSourceList FavoriteInput_Selected
        {
            get => _FavoriteInputSelect;
            set
            {
                SetProperty(ref _FavoriteInputSelect, value);
                OnPropertyChanged("FavoriteInput_Selected");
            }
        }

        public void Invoke_RefreshData()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshData;
            bw.RunWorkerAsync(); //myArg is the optional argument
        }
        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try 
            {
                //sender is the ‘bw’ object
                BackgroundWorker bwk = (BackgroundWorker)sender;
                InputsList.Clear();
                inputList = new Dictionary<string, InputInfo>();
                //load hotkey setting
                HotkeySettings curHotkey = DdpmCommonHelper.DeviceManagerSA.ReadCurrentHotkey(this.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.edid).Result;
                //0708 error handling for non-EE support monitor
                try
                {
                    inputList = DdpmCommonHelper.DeviceManagerSA.GetInputSourcelist(DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo).Result;
                }
                catch(Exception ex)
                {
                    Debug.WriteLine($"InputSource caused crash = {ex.Message}");
                    inputList = null;
                }
                if (inputList != null)
                {
                    foreach (string item in inputList.Keys)
                    {
                        _inputsList.Add(new InputSourceList()
                        {
                            inputSource = item,
                            displayHotkeysModule = DisplayHotkeysModule,
                        });
                    }
                    InputsList = _inputsList;
                    if (curHotkey!=null && curHotkey.HotkeyInfo.Count > 0)
                    {
                        HotkeyInfo? hotkeyInfo = curHotkey.HotkeyInfo.Find(x => x.Job.Equals(HotkeyType.FavoriteInputSource));
                        if (hotkeyInfo != null && hotkeyInfo.InputSource.Count > 0) 
                        {
                            FavoriteInput_Selected =  _inputsList.Find(x => x.inputDisplayText.Equals(hotkeyInfo.InputSource[0].Name));
                        }
                        else
                        {
                            FavoriteInput_Selected = _inputsList.Find(x => (x.inputSource == DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
                        }
                    }
                    else
                    {
                        FavoriteInput_Selected = _inputsList.Find(x => (x.inputSource == DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
                    }
                    //switch input
                    if (curHotkey != null && curHotkey.HotkeyInfo.Count > 0)
                    {
                        HotkeyInfo? hotkeyInfo = curHotkey.HotkeyInfo.Find(x => x.Job.Equals(HotkeyType.SwitchInputSource));
                        if (hotkeyInfo != null && hotkeyInfo.InputSource.Count > 0)
                        {
                            SwitchInput1_Selected = _inputsList.Find(x => x.inputDisplayText.Equals(hotkeyInfo.InputSource[0].Name));
                            SwitchInput2_Selected = _inputsList.Find(x => x.inputDisplayText.Equals(hotkeyInfo.InputSource[1].Name));

                        }
                        else
                        {
                            if (FavoriteInput_Selected != null)
                            {
                                SwitchInput1_Selected = _inputsList.Find(x => (x.inputSource == DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
                                if (SwitchInput1_Selected != null)
                                    SwitchInput2_Selected = _inputsList.Where(x => x.inputDisplayText != _switchInput1Selected.inputDisplayText).First();
                            }
                        }
                    }
                    else
                    {
                        if (FavoriteInput_Selected != null)
                        {
                            SwitchInput1_Selected = _inputsList.Find(x => (x.inputSource == DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
                            if (SwitchInput1_Selected != null)
                                SwitchInput2_Selected = _inputsList.Where(x => x.inputDisplayText != _switchInput1Selected.inputDisplayText).First();
                        }
                    }                  
                }

                
                string swHortcutText = string.Empty;
                if (curHotkey.HotkeyInfo.Count > 0)
                {

                    foreach (var hotkeyInfo in curHotkey.HotkeyInfo)
                    {
                        List<VirtualKey> hotkeys = hotkeyInfo.Hotkey;
                        switch (hotkeyInfo.Job)
                        {
                            case HotkeyType.ToggleInputSource:
                                KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                                hotkeys.Clear();
                                ToggleInputSourceKey = swHortcutText;
                                break;
                            case HotkeyType.FavoriteInputSource:
                                KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                                hotkeys.Clear();
                                FavoriteInputSourceKey = swHortcutText;
                                break;
                            case HotkeyType.SwitchInputSource:
                                KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                                hotkeys.Clear();
                                SwitchInputSourceKey = swHortcutText;
                                break;
                            case HotkeyType.SwapIputPIPPBP:
                                KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                                hotkeys.Clear();
                                SwapPIPPBPInputSourceKey = swHortcutText;
                                break;
                            case HotkeyType.ChangePIPPosition:
                                KeysHelper.ReSetHotKeyText(ref swHortcutText, ref hotkeys);
                                hotkeys.Clear();
                                ChangePIPPositionKey = swHortcutText;
                                break;
                        }
                    }
                }
            }
            catch (Exception)
            {

                ;
            }

        }
        private void RunWorkerCompleted_RefreshData(object sender, RunWorkerCompletedEventArgs e)
        {
            //Handling the result and final process
            Debug.WriteLine("InputSource-->Hotkey tab data refresh done");
        }
    }
}
