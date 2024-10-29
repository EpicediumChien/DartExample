using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using VcpCore.Common;
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

        private ObservableCollection<InputSourceList> _inputsList = new ObservableCollection<InputSourceList>();

        public ObservableCollection<InputSourceList> InputsList
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
                if (value != null)
                {
                    Debug.WriteLine($"SetProperty SwitchInput1_Selected: {value.inputDisplayText}");
                    SetProperty(ref _switchInput1Selected, value);
                    OnPropertyChanged("SwitchInput1_Selected");
                }
            }
        }

        private InputSourceList _switchInput2Selected = new InputSourceList();

        public InputSourceList SwitchInput2_Selected
        {
            get => _switchInput2Selected;
            set
            {
                if (value != null)
                {
                    Debug.WriteLine($"SetProperty SwitchInput2_Selected: {value.inputDisplayText}");
                    SetProperty(ref _switchInput2Selected, value);
                    OnPropertyChanged("SwitchInput2_Selected");
                }
            }
        }

        private InputSourceList _FavoriteInputSelect = new InputSourceList();

        public InputSourceList FavoriteInput_Selected
        {
            get => _FavoriteInputSelect;
            set
            {
                if (value != null)
                {
                    Debug.WriteLine($"SetProperty FavoriteInput_Selected: {value.inputDisplayText}");
                    SetProperty(ref _FavoriteInputSelect, value);
                    OnPropertyChanged("FavoriteInput_Selected");

                }

            }
        }

        private int _FavoriteInput_Selected_Index = 0;
        public int FavoriteInput_Selected_Index
        {
            get
            {
                Debug.WriteLine($"get FavoriteInput_Selected_Index :{_FavoriteInput_Selected_Index}");
                return _FavoriteInput_Selected_Index;
            }
            set
            {
                if (value != null)
                {
                    Debug.WriteLine($"set FavoriteInput_Selected_Index: {value}");
                    SetProperty(ref _FavoriteInput_Selected_Index, value);
                    OnPropertyChanged("FavoriteInput_Selected_Index");

                }

            }
        }

        public Visibility PxPkeySettings_Visibility { get; set; } = Visibility.Collapsed;

        public void SaveHotkeySettings(InputSourceObj inputSourceObj, string inputNo)
        {
            var temp = DdpmCommonHelper.DeviceManagerSA.ReadCurrentHotkey(this.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo).Result;
            HotkeySettings curHotkey = temp.Item1;
            List<HotkeyData> list = temp.Item2;
            if (curHotkey != null && curHotkey.HotkeyInfo.Count > 0)
            {
                List<InputSourceObj> updateInputSourceList = new List<InputSourceObj>();
                switch (inputNo)
                {
                    //FavoriteInputSource
                    case "0":
                        HotkeyInfo? FavoriteHotkeyInfo = curHotkey.HotkeyInfo.Find(x => x.Job.Equals(HotkeyType.FavoriteInputSource));
                        if (FavoriteHotkeyInfo != null)//FavoriteHotkeyInfo.InputSource.Count > 0)
                        {
                            HotkeyData? hotkeyData1 = list?.SingleOrDefault(x => x.hotkeyType.Equals(HotkeyType.FavoriteInputSource));
                            if (hotkeyData1?.inputSource.Count == 0)
                            {
                                updateInputSourceList.Add(inputSourceObj);
                                FavoriteHotkeyInfo.InputSource = updateInputSourceList;
                                bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(this.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo, FavoriteHotkeyInfo).Result;
                            }
                            else
                            {
                                InputSourceList? inputSourceList = _inputsList.FirstOrDefault(x => x.inputDisplayText.Equals(hotkeyData1?.inputSource[0].Name));//FavoriteHotkeyInfo.InputSource[0].Name));
                                if (!inputSourceObj.Name.Equals(inputSourceList?.inputDisplayText))
                                {
                                    updateInputSourceList.Add(inputSourceObj);
                                    FavoriteHotkeyInfo.InputSource = updateInputSourceList;
                                    bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(this.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo, FavoriteHotkeyInfo).Result;
                                }
                            }

                        }
                        break;
                    case "1":
                        //switch input 1
                        HotkeyInfo? SwitchHotkeyInfo = curHotkey.HotkeyInfo.Find(x => x.Job.Equals(HotkeyType.SwitchInputSource));
                        if (SwitchHotkeyInfo != null)// SwitchHotkeyInfo.InputSource.Count > 0)
                        {
                            HotkeyData? hotkeyData2 = list.SingleOrDefault(x => x.hotkeyType.Equals(HotkeyType.SwitchInputSource));
                            InputSourceList? input1 = _inputsList.FirstOrDefault(x => x.inputDisplayText.Equals(hotkeyData2?.inputSource[0].Name));// SwitchHotkeyInfo.InputSource[0].Name));
                            InputSourceList? input2 = _inputsList.FirstOrDefault(x => x.inputDisplayText.Equals(hotkeyData2?.inputSource[1].Name));//SwitchHotkeyInfo.InputSource[1].Name));
                            if (!inputSourceObj.Name.Equals(input1?.inputDisplayText))
                            {
                                updateInputSourceList.Add(inputSourceObj);
                                updateInputSourceList.Add(new InputSourceObj(input2?.inputDisplayText));
                                SwitchHotkeyInfo.InputSource = updateInputSourceList;
                                bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(this.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo, SwitchHotkeyInfo).Result;
                            }
                        }
                        break;
                    case "2":
                        //switch input 2
                        HotkeyInfo? SwitchHotkeyInfo2 = curHotkey.HotkeyInfo.Find(x => x.Job.Equals(HotkeyType.SwitchInputSource));
                        if (SwitchHotkeyInfo2 != null)//SwitchHotkeyInfo2.InputSource.Count > 0)
                        {
                            HotkeyData? hotkeyData3 = list.SingleOrDefault(x => x.hotkeyType.Equals(HotkeyType.SwitchInputSource));
                            InputSourceList? input1 = _inputsList.FirstOrDefault(x => x.inputDisplayText.Equals(hotkeyData3?.inputSource[0].Name));//SwitchHotkeyInfo2.InputSource[0].Name));
                            InputSourceList? input2 = _inputsList.FirstOrDefault(x => x.inputDisplayText.Equals(hotkeyData3?.inputSource[1].Name));//SwitchHotkeyInfo2.InputSource[1].Name));

                            if (!inputSourceObj.Name.Equals(input2?.inputDisplayText))
                            {
                                updateInputSourceList.Add(new InputSourceObj(input1?.inputDisplayText));
                                updateInputSourceList.Add(inputSourceObj);
                                SwitchHotkeyInfo2.InputSource = updateInputSourceList;
                                bool saveSettings = DdpmCommonHelper.DeviceManagerSA.SaveHotkeySetting(this.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo, SwitchHotkeyInfo2).Result;
                            }
                        }
                        break;
                }




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
            IsBusy = true;
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try
            {
                //sender is the ‘bw’ object
                BackgroundWorker bwk = (BackgroundWorker)sender;
                inputList = new Dictionary<string, InputInfo>();
                //load hotkey setting
                var temp = DdpmCommonHelper.DeviceManagerSA.ReadCurrentHotkey(this.DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo).Result;
                HotkeySettings curHotkey = temp.Item1;
                List<HotkeyData> list = temp.Item2;
                //0708 error handling for non-EE support monitor
                try
                {
                    inputList = DdpmCommonHelper.DeviceManagerSA.GetInputSourcelist(DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo).Result;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"InputSource caused crash = {ex.Message}");
                    inputList = null;
                }
                if (inputList != null)
                {

                    ObservableCollection<InputSourceList> tmpInputsList = new ObservableCollection<InputSourceList>();
                    foreach (string item in inputList.Keys)
                    {
                        tmpInputsList.Add(new InputSourceList()
                        {
                            inputSource = item,
                            displayHotkeysModule = DisplayHotkeysModule,
                        });
                    }

                    _inputsList = tmpInputsList;

                    if (curHotkey != null && curHotkey.HotkeyInfo.Count > 0)
                    {
                        HotkeyInfo? hotkeyInfo = curHotkey.HotkeyInfo.Find(x => x.Job.Equals(HotkeyType.FavoriteInputSource));
                        if (hotkeyInfo != null && list != null && list.Count > 0)// hotkeyInfo.InputSource.Count > 0)
                        {
                            HotkeyData? hotkeyData = list.SingleOrDefault(x => x.hotkeyType.Equals(HotkeyType.FavoriteInputSource));
                            if (hotkeyData != null)
                            {
                                if (string.IsNullOrEmpty(hotkeyData.inputSource[0].Name))
                                {
                                    List<InputSourceObj> inputSourceObjs = hotkeyData.inputSource.Join(inputList.Values, a => a.Code, b => b.Code, (a, b) => new InputSourceObj()
                                    {
                                        Name = b.InputName,
                                        Code = a.Code,
                                    }).ToList();
                                    if (inputSourceObjs == null || list.Count == 0)
                                    {
                                        //hotkey.InputSource Count must not 0. 
                                        Debug.WriteLine($"FavoriteInputSource migration convert InputSource fail");
                                        //set default
                                        _FavoriteInputSelect = InputsList.SingleOrDefault(x => (x.inputSource == DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
                                        if(_FavoriteInputSelect != null) 
                                            SaveHotkeySettings(new InputSourceObj(_FavoriteInputSelect.inputDisplayText), "0");
                                    }
                                    else
                                    {
                                        _FavoriteInputSelect = InputsList.SingleOrDefault(x => x.inputDisplayText.Equals(inputSourceObjs[0].Name));// hotkeyInfo.InputSource[0].Name));
                                    }
                                }
                                else
                                {
                                    _FavoriteInputSelect = InputsList.SingleOrDefault(x => x.inputDisplayText.Equals(hotkeyData?.inputSource[0].Name));// hotkeyInfo.InputSource[0].Name));
                                    Debug.WriteLine($"FavoriteInput_Selected: {FavoriteInput_Selected?.inputDisplayText}");
                                    //_FavoriteInput_Selected_Index = 2;
                                }

                            }

                        }
                        else
                        {
                            _FavoriteInputSelect = InputsList.SingleOrDefault(x => (x.inputSource == DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
                            //_FavoriteInput_Selected_Index = 0;
                            if (_FavoriteInputSelect != null)
                                SaveHotkeySettings(new InputSourceObj(_FavoriteInputSelect.inputDisplayText), "0");
                        }
                    }
                    else
                    {
                        _FavoriteInputSelect = InputsList.SingleOrDefault(x => (x.inputSource == DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
                        if (_FavoriteInputSelect != null)
                            SaveHotkeySettings(new InputSourceObj(_FavoriteInputSelect.inputDisplayText), "0");
                    }
                    //switch input
                    if (curHotkey != null && curHotkey.HotkeyInfo.Count > 0)
                    {
                        HotkeyInfo? hotkeyInfo = curHotkey.HotkeyInfo.Find(x => x.Job.Equals(HotkeyType.SwitchInputSource));
                        if (hotkeyInfo != null && list != null && list.Count > 0)//hotkeyInfo.InputSource.Count > 0)
                        {
                            HotkeyData? hotkeyData2 = list.SingleOrDefault(x => x.hotkeyType.Equals(HotkeyType.SwitchInputSource));
                            if (hotkeyData2 != null)
                            {
                                if (string.IsNullOrEmpty(hotkeyData2.inputSource[0].Name))
                                {
                                    List<InputSourceObj> inputSourceObjs = hotkeyData2.inputSource.Join(inputList.Values, a => a.Code, b => b.Code, (a, b) => new InputSourceObj()
                                    {
                                        Name = b.InputName,
                                        Code = a.Code,
                                    }).ToList();
                                    if (inputSourceObjs == null || list.Count == 0)
                                    {
                                        //hotkey.InputSource Count must not 0. 
                                        Debug.WriteLine($"SwitchInputSource migration convert InputSource fail");
                                        //set default
                                        _switchInput1Selected = InputsList.SingleOrDefault(x => (x.inputSource == DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
                                        if (_switchInput1Selected != null)
                                            _switchInput2Selected = _inputsList.Where(x => x.inputDisplayText != _switchInput1Selected.inputDisplayText).First();
                                        if(_switchInput1Selected != null && _switchInput2Selected != null)
                                        {
                                            SaveHotkeySettings(new InputSourceObj(_switchInput1Selected.inputDisplayText), "1");
                                            SaveHotkeySettings(new InputSourceObj(_switchInput2Selected.inputDisplayText), "2");
                                        }
                                    }
                                    else
                                    {
                                        _switchInput1Selected = InputsList.SingleOrDefault(x => x.inputDisplayText.Equals(inputSourceObjs[0].Name));//hotkeyInfo.InputSource[0].Name));
                                        _switchInput2Selected = InputsList.SingleOrDefault(x => x.inputDisplayText.Equals(inputSourceObjs[1].Name));//hotkeyInfo.InputSource[1].Name));
                                    }
                                }
                                else
                                {
                                    _switchInput1Selected = InputsList.SingleOrDefault(x => x.inputDisplayText.Equals(hotkeyData2?.inputSource[0].Name));//hotkeyInfo.InputSource[0].Name));
                                    _switchInput2Selected = InputsList.SingleOrDefault(x => x.inputDisplayText.Equals(hotkeyData2?.inputSource[1].Name));//hotkeyInfo.InputSource[1].Name));
                                }
                            }
                        }
                        else
                        {
                            if (FavoriteInput_Selected != null)
                            {
                                _switchInput1Selected = InputsList.SingleOrDefault(x => (x.inputSource == DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
                                if (_switchInput1Selected != null)
                                    _switchInput2Selected = _inputsList.Where(x => x.inputDisplayText != _switchInput1Selected.inputDisplayText).First();
                                if (_switchInput1Selected != null && _switchInput2Selected != null)
                                {
                                    SaveHotkeySettings(new InputSourceObj(_switchInput1Selected.inputDisplayText), "1");
                                    SaveHotkeySettings(new InputSourceObj(_switchInput2Selected.inputDisplayText), "2");
                                }
                            }
                        }
                    }
                    else
                    {
                        if (FavoriteInput_Selected != null)
                        {
                            _switchInput1Selected = InputsList.SingleOrDefault(x => (x.inputSource == DisplayHotkeysModule.SelectedHomeDevice.MonitorInfo.inputSource));
                            if (_switchInput1Selected != null)
                                _switchInput2Selected = InputsList.Where(x => x.inputDisplayText != _switchInput1Selected.inputDisplayText).First();
                            if (_switchInput1Selected != null && _switchInput2Selected != null)
                            {
                                SaveHotkeySettings(new InputSourceObj(_switchInput1Selected.inputDisplayText), "1");
                                SaveHotkeySettings(new InputSourceObj(_switchInput2Selected.inputDisplayText), "2");
                            }
                        }
                    }
                }
                else
                    return;//temp solution 0708
                OnPropertyChanged("InputsList");
                OnPropertyChanged("FavoriteInput_Selected");
                OnPropertyChanged("SwitchInput1_Selected");
                OnPropertyChanged("SwitchInput2_Selected");
                //OnPropertyChanged("FavoriteInput_Selected_Index");
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
                else
                {
                    ToggleInputSourceKey = "None";
                    FavoriteInputSourceKey = "None";
                    SwitchInputSourceKey = "None";
                    SwapPIPPBPInputSourceKey = "None";
                    ChangePIPPositionKey = "None";
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
            IsBusy = false;
            OnPropertyChanged("IsBusy");
            Debug.WriteLine("InputSource-->Hotkey tab data refresh done");
        }
        #region UI Enable Flags

        private bool _isBusy = false;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion UI Enable Flags
    }
}