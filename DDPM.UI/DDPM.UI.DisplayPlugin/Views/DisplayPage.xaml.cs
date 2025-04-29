#define USE_VBARITEM1

using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Interfaces.ViewModels;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Common.ViewModels;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.Brightness;
using DDPM.UI.Module.Color;
using DDPM.UI.Module.DisplayHotkeys;
using DDPM.UI.Module.DisplayOthers;
using DDPM.UI.Module.DisplayProperties;
using DDPM.UI.Module.DisplayWebcam;
using DDPM.UI.Module.EzArrange;
using DDPM.UI.Module.EzMemory;
using DDPM.UI.Module.EzSettings;
using DDPM.UI.Module.Gaming;
using DDPM.UI.Module.GamingVisionEngine;
using DDPM.UI.Module.InputSource;
using DDPM.UI.Module.Kvm;
using DDPM.UI.Module.PipPbp;
using DDPM.UI.Module.WebCameraCapture;
using DDPM.UI.Module.WebCameraColorImage;
using DDPM.UI.Module.WebCameraSettings;
using DDPM.UI.Plugin.Common.ViewModels;
using DDPM.UI.Plugin.DisplayPlugin.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using DDPM.UI.Resources.Helper;
using DDPM.UI.Module.MonitorAudio;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using VcpCore.Common;

namespace DDPM.UI.Plugin.DisplayPlugin.Views
{
    /// <summary>
    /// Interaction logic for DisplayPage.xaml
    /// </summary>
    public partial class DisplayPage : UserControl, IDisposable
    {
        #region Private members
        private IDisplayPageViewModel? _ivm;
        private ILog? _log;

        //private IDevicePageViewModel? _idevPageVm;
        private readonly DisplayViewModel _vmDisplay;

        private WebCameraViewModel? _webCameraViewModel;

        private DisplayWebcamModule? _displayWebCamModule;

        private readonly IDeviceManagerSA? _deviceManagerSA;
        private bool _isDisposed = false;

        private ModuleGroup moduleGroup;
        #endregion

        #region Init

        public DisplayPage()
        {
            InitializeComponent();

            _log = DisplayPlugin.PluginIoc?.GetService<ILog>();
            _log?.Debug("DisplayPage.ctor()");

            _vmDisplay = DisplayPlugin.PluginIoc?.GetService<IDisplayViewModel>() as DisplayViewModel;
            _webCameraViewModel = (WebCameraViewModel?)DisplayPlugin.PluginIoc.GetService<IPeripheralViewModel>();
            _deviceManagerSA = DisplayPlugin.PluginIoc?.GetService<IDeviceManagerSA>();

            _ivm = DisplayPlugin.PluginIoc?.GetService<IDisplayPageViewModel>();
            if (_ivm != null)
            {
                //_idevPageVm = (IDevicePageViewModel)_ivm;
                _ivm.Reset();
                DataContext = _ivm;
#if USE_VBARITEM1
                _ivm.VbarItemClickCommand = new RelayCommand<VbarItem1>(OnVbarItemClicked);
#else
                _ivm.VbarItemClickCommand = new RelayCommand<VbarItem>(OnVbarItemClicked);
#endif

                basePage.SetHomeDevices(_ivm.HomeDevices);
                basePage.SetSelectedHomeDevice(_ivm.SelectedHomeDevice);
                DdpmCommonHelper.ModuleOwner = basePage.ViewModel;
            }

            //Must be eailer  then UserControl_Loaded()
            // basePage.SetModuleGroupList(BuildModuleGroups());
            basePage.SetLeftFrameWidth(660);

            UserControl defLeftView = new DisplayDefaultLeftView();
            if (_ivm != null)
            {
                defLeftView.DataContext = _ivm.SelectedHomeDevice;
                basePage.SetDefaultLeftView(defLeftView);
                basePage.LeftArrowClick += OnLeftArrowClick;
            }

            //LeftFrame.Content = GetDefaultLeftView();
            //Robert_Lin,2024-7-26, Unused, move to DdpmHomePlugin
            //if (DdpmCommonHelper.MyConsole != null)
            //    DdpmCommonHelper.MyConsole.RegisterForEvent("AddDeviceClicked", OnAddDeviceClicked);

            //Robert_Lin, 2024-8-6, Duplicate to DisplayPlugin to fix bug (not PIMS)
            // When stay in Homepage (never let DisplayPage loaded, so the DDC_CI Status changed event is not been
            // registered), then user change DDC/CI status from monitor's OSD. DDPM.SA will sent DDC DC status changed
            // event but DDPM.UI will not handled.
            //
            if (_deviceManagerSA != null)
            {
                _deviceManagerSA.DDCCIStatuschanged += _deviceManagerSA_DDCCIStatuschanged;
                _deviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;

                //[Dean 1001]for hotkey to set current selected display device to SA
                if (_ivm != null &&
                    _ivm.SelectedHomeDevice != null &&
                    _ivm.SelectedHomeDevice.MonitorInfo != null)
                    _deviceManagerSA.SetLastSelectedMonitorFromUI(_ivm.SelectedHomeDevice.MonitorInfo);
            }

            basePage.SetModuleGroupList(BuildModuleGroups());

            HomeDevice? homeDev = GetSelectedHomeDevice();
            if (_ivm != null && homeDev != null)
            {
                _ivm.SelectedHomeDevice.IsRestoreBtnVisible = Visibility.Visible;
                if (!homeDev.MonitorInfo.DDCisON)
                {
                    HandleDdcCiOnOffEvent(homeDev.MonitorInfo, homeDev.MonitorInfo.DDCisON);
                }
            }
        }

        //Move into Exit region
        //~DisplayPage()
        //{
        //    if (_deviceManagerSA != null)
        //    {
        //        _deviceManagerSA.DDCCIStatuschanged -= _deviceManagerSA_DDCCIStatuschanged;
        //        _deviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
        //    }
        //}

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_log != null)
            {
                _log.Info("DisplayPage.UserControl_Loaded");
            }
            //basePage.SetModuleGroupList(BuildModuleGroups());

            //HomeDevice? homeDev = GetSelectedHomeDevice();
            //if (homeDev != null)
            //{
            //    _ivm.SelectedHomeDevice.IsRestoreBtnVisible = Visibility.Visible;
            //    if (!homeDev.MonitorInfo.DDCisON)
            //    {
            //        HandleDdcCiOnOffEvent(homeDev.MonitorInfo, homeDev.MonitorInfo.DDCisON);
            //    }
            //}

            ApplyockStatusFromSettingsFile();
            //basePage.SetLockModuleGroup(Constants.GroupName_EasyArrange, true);
            //basePage.SetLockModuleGroup(Constants.GroupName_InputSource, true);
        }

        //Only for init (entering Landing page)
        private void ApplyockStatusFromSettingsFile()
        {
            DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();// DeviceManagerSA.ReloadAppConfigData().Result;
            if (data == null)
                return;
            if (data.UserSettings == null)
                return;

            bool isLocked = data.LockSettings.Lock_Display_ActiveInputSource;
            basePage.SetLockModuleGroup(Constants.GroupName_InputSource, isLocked);
            isLocked = data.LockSettings.Lock_Display_EasyArrangeLayout;
            basePage.SetLockModuleGroup(Constants.GroupName_EasyArrange, isLocked);
        }

        #endregion Init

        #region Init for Modules

        /// <summary>
        /// Base on specified monitor's capabiliies to build the Vbar items, and headers/modules
        /// </summary>
        private List<ModuleGroup> BuildModuleGroups()
        {
            Stopwatch sw = new Stopwatch();
            _log?.Info("Enter DisplayPage.BuildModuleGroups()");

            List<ModuleGroup> groups = new List<ModuleGroup>();
            moduleGroup = new();

            // --------- Robert_Lin 2024-8-28 -----
            // The support capability check in BuildModuleGroup() is removed.
            // All Groups and Modules (headers) will be added into ModuleGroups.
            // When entering LandingPage and when monitor device changed from ComboBox
            // ModuleOwner (DeviceBasePageViewModel) will show/hide the Groups/Headers 
            // depended on the capabilities of the selected monitor.
            // ------------------------------------------------------------
            /*
            // Debug to show selected modules only
            // Robert_Lin 2024-5-27 use isDebugModule flag to switch the release|debug capabilities
            bool isDebugModule = false; //Please set to false in release build

            ModuleCapabilities moduleCapabilities;

            //string str = _ivm.SelectedHomeDevice.MonitorInfo.edid.ModelName.Substring(4, 1);

            //0708 fix USBKVM exception on specific monitor
            //bool? support = _ivm?.SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("EE");
            bool? support = _ivm?.SelectedHomeDevice.HasCapability_KVM;

            //Robert_Lin, 2024-8-4, use the HomeDevice method
            //OLD:
            //bool? support_contrast = _ivm?.SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("12");
            //bool? SupportGaming = _ivm?.SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("F5");
            //bool? SupportVision = _ivm?.SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("EC");
            //NEW:
            bool? support_contrast = _ivm.SelectedHomeDevice.HasCapability_Contrast;
            bool? SupportGaming = _ivm.SelectedHomeDevice.HasCapability_Gaming;
            bool? SupportVision = _ivm.SelectedHomeDevice.HasCapability_VisionEngine;

            if (isDebugModule)
            {
                //Only the Modules to be debugged will be enabled
                moduleCapabilities = new ModuleCapabilities()
                {
                    BrightnessContrast = true,
                    Color = true,
                    DisplayProperties = true,

                    InputSource = true,
                    PipPbp = true,
                    DisplayHotkeys = true,

                    EzArrange = true,
                    EzMemeory = true,
                    EzSettings = true,

                    Gaming = true,
                    VisionEngine = true,

                    Kvm = true,

                    DisplayOthers = true,
                };
            }
            else
            {
                // Enable all Modules
                moduleCapabilities = new ModuleCapabilities()
                {
                    BrightnessContrast = true,
                    Color = true,
                    DisplayProperties = true,

                    InputSource = true,
                    PipPbp = true,
                    DisplayHotkeys = true,

                    EzArrange = true,
                    EzMemeory = true,
                    EzSettings = true,

                    Gaming = SupportGaming.HasValue ? SupportGaming.Value : false,
                    VisionEngine = SupportVision.HasValue ? SupportVision.Value : false,

                    //Robert_Lin, 2024-8-4, use HomeDevice method
                    //OLD:
                    //Kvm = ((support == null || support == false) && (DdpmCommonHelper.DeviceManagerSA.isNKVMSupportMonitor(_ivm?.SelectedHomeDevice.MonitorInfo).Result == false)) ? false : true,
                    //NEW:
                    Kvm = ((support == null || support == false) && (_ivm.SelectedHomeDevice.HasCapability_NetworkKvm == false)) ? false : true,

                    DisplayOthers = true,
                };
            }
            */

            string lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. ";

            //Robert_Lin, 2024-7-26, for ModuleGroup:
            //  GroupName is the Id used to identify a ModuelGroup
            //  VbarText is the display string on VbarItem control

            //Group[0] Display Settings
            //         Header[0] Brightness/Contrast,   BrightnessModule
            //         Header[1] Color,                 ColorModule
            //         Header[2] Display Properties,    DisplayPropertiesModule
            moduleGroup = new ModuleGroup()
            {
                GroupName = Constants.GroupName_DisplaySettings, // "DisplaySettings",
                VbarText = Strings.VbarText_DisplaySettings,
                IconTemplate = (ControlTemplate)this.TryFindResource("iconTemplate_DisplaySettings"),
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Display.Settings.png"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.DisplaySettings)
            };
            bool? support_contrast = _ivm.SelectedHomeDevice.HasCapability_Contrast;
            //If the monitor has Brightness/Contrast capability
            //if (moduleCapabilities.BrightnessContrast)
            {
                string title_str = string.Empty;
                sw.Restart();
                if ((support_contrast != null) && support_contrast.Equals(true))
                    title_str = Strings.RightViewHeader_BrightnessContrast;
                else
                    title_str = "Luminance";
                //Option_A, Construct module on BuldModuleList
                //moduleGroup.AddHeader("Brightness/Contrast",new BrightnessModule() );

                //Option_B,Construct module on first Activate
                moduleGroup.AddHeader(title_str, typeof(BrightnessModule), Constants.ModuleName_Brightness);

                sw.Stop();
                _log?.Info($"* BrightnessModule ctor consume {sw.ElapsedMilliseconds} msec");
            }

            //If the monitor has Color capability
            //if (moduleCapabilities.Color)
            {
                sw.Restart();
                //Robert_Lin, 2024-5-30
                //moduleGroup.AddHeader("Color", new ColorModule(_ivm?.SelectedHomeDevice));
                //moduleGroup.AddHeader("Color", typeof(ColorModule));
                moduleGroup.AddHeader(Strings.RightViewHeader_Color, typeof(ColorModule), Constants.ModuleName_Color);
                //new ColorModule() { SelectedHomeDevice = _ivm?.SelectedHomeDevice });
                sw.Stop();
                _log?.Info($"* ColorModule ctor consume {sw.ElapsedMilliseconds} msec");
            }

            //If the monitor has Display Properties capability
            //if (moduleCapabilities.DisplayProperties && !moduleCapabilities.Gaming)
            {
                sw.Restart();

                //Option_A, Construct module on BuldModuleList
                //moduleGroup.AddHeader("Display Properties", new DisplayPropertiesModule());
                //Robert_Lin 20240530-remove argument on ctor
                //new DisplayPropertiesModule(_ivm?.SelectedHomeDevice));
                //);
                //Option_B,Construct module on first Activate
                //moduleGroup.AddHeader("Display Properties", typeof(DisplayPropertiesModule));
                moduleGroup.AddHeader(Strings.RightViewHeader_DisplayProperties, typeof(DisplayPropertiesModule), Constants.ModuleName_DisplayProperties);

                sw.Stop();
                _log?.Info($"* DisplayPropertiesModule ctor consume {sw.ElapsedMilliseconds} msec");
            }

            //If this ModuleGroup has any item, then add into moduleGroups
            if (moduleGroup.HeaderCount > 0)
            {
                groups.Add(moduleGroup);
            }

            //Group[1] Input Source
            //         Header[0] General,   InputSourceModule
            //         Header[1] PIP/PBP,   PipPbpModule
            //         Header[2] Hotkeys,   DisplayHotkeysModule
            moduleGroup = new ModuleGroup()
            {
                GroupName = Constants.GroupName_InputSource, // "InputSource",
                VbarText = Strings.VbarText_InputSource,
                IconTemplate = (ControlTemplate)this.TryFindResource("iconTemplate_InputSource"),
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Display.InputSource.png"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.DisplayInputSource)
            };
            //If the monitor has InputSource capability
            //if (moduleCapabilities.InputSource)
            {
                sw.Restart();
                //Option_A, Construct module on BuldModuleList
                //Robert_Lin 20240530-remove argument on ctor
                //moduleGroup.AddHeader("General", new InputSourceModule(_ivm?.SelectedHomeDevice) { }); //Jason
                //moduleGroup.AddHeader("General", new InputSourceModule()); //Jason
                //Option_B,Construct module on first Activate
                //moduleGroup.AddHeader("General", typeof(InputSourceModule));
                moduleGroup.AddHeader(Strings.RightViewHeader_General, typeof(InputSourceModule), Constants.ModuleName_InputSource);

                sw.Stop();
                _log?.Info($"* InputSourceModule ctor consume {sw.ElapsedMilliseconds} msec");
            }

            //If the monitor has PIP/PBP capability
            //if (moduleCapabilities.PipPbp)
            {
                sw.Restart();
                //moduleGroup.AddHeader("PIP/PBP", typeof(PipPbpModule));
                moduleGroup.AddHeader(Strings.RightViewHeader_PIPPBP, typeof(PipPbpModule), Constants.ModuleName_PipPbp);
                sw.Stop();
                _log?.Info($"* PipPbpModule ctor consume {sw.ElapsedMilliseconds} msec");
            }

            //if (moduleCapabilities.DisplayHotkeys)
            {
                sw.Restart();
                //moduleGroup.AddHeader("Hotkeys", typeof(DisplayHotkeysModule));
                moduleGroup.AddHeader(Strings.RightViewHeader_Hotkeys, typeof(DisplayHotkeysModule), Constants.ModuleName_DisplayHotkeys);
                sw.Stop();
                _log?.Info($"* DisplayHotkeysModule ctor consume {sw.ElapsedMilliseconds} msec");
            }

            //If this ModuleGroup has any item, then add into moduleGroups
            if (moduleGroup.HeaderCount > 0)
            {
                groups.Add(moduleGroup);
            }

            //Group[2] Easy Arrange
            //         Header[0] Layout,        EzArrangeModule
            //         Header[1] Easy Memopry,  EzMemoryModule
            //         Header[2] Settings,      EzSettingsModule
            moduleGroup = new ModuleGroup()
            {
                GroupName = Constants.GroupName_EasyArrange, // "EasyArrange",
                VbarText = Strings.VbarText_EasyArrange,
                IconTemplate = (ControlTemplate)this.TryFindResource("iconTemplate_EasyArrange"),
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Display.EA.png"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.DisplayEA)
            };
            //If the monitor has EasyArrange capability
            //if (moduleCapabilities.EzArrange)
            {
                sw.Restart();
                //moduleGroup.AddHeader("Layout", new EzArrangeModule(_vmDisplay));
                moduleGroup.AddHeader(Strings.RightViewHeader_Layout, new EzArrangeModule(_vmDisplay));
                //moduleGroup.AddHeader("Layout", typeof(EzArrangeModule));
                sw.Stop();
                _log?.Info($"* EzArrangeModule ctor consume {sw.ElapsedMilliseconds} msec");
            }

            //If the monitor has EasyMemory capability
            //if (moduleCapabilities.EzMemeory)
            {
                sw.Restart();
                //moduleGroup.AddHeader("Easy Memory", new EzMemoryModule() { SelectedHomeDevice = _ivm?.SelectedHomeDevice });
                moduleGroup.AddHeader(Strings.RightViewHeader_EasyMemory, new EzMemoryModule(_vmDisplay));
                //moduleGroup.AddHeader(Strings.RightViewHeader_EasyMemory, new EzMemoryModule() { SelectedHomeDevice = _ivm?.SelectedHomeDevice });
                sw.Stop();
                _log?.Info($"* EzMemoryModule ctor consume {sw.ElapsedMilliseconds} msec");
            }

            //If the monitor has Easy Arrange Settings capability
            //if (moduleCapabilities.EzSettings)
            {
                sw.Restart();
                moduleGroup.AddHeader(Strings.RightViewHeader_Settings, typeof(EzSettingsModule), Constants.ModuleName_EzSettings);
                //moduleGroup.AddHeader(Strings.RightViewHeader_Settings, new EzSettingsModule() { SelectedHomeDevice = _ivm?.SelectedHomeDevice });
                sw.Stop();
                _log?.Info($"* EzSettingsModule ctor consume {sw.ElapsedMilliseconds} msec");
            }

            //If this ModuleGroup has any item, then add into moduleGroups
            if (moduleGroup.HeaderCount > 0)
            {
                groups.Add(moduleGroup);
            }

            //Group[3] Gaming
            //         Header[0] General,       GamingModule
            //         Header[1] Vision Engine, VisionEngineModule
            moduleGroup = new ModuleGroup()
            {
                GroupName = Constants.GroupName_Gaming, // "Gaming",
                VbarText = Strings.VbarText_Gaming,
                IconTemplate = (ControlTemplate)this.TryFindResource("iconTemplate_Gaming"),
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Display.Gaming.png"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.DisplayGaming)
            };

            //If the monitor has Gaming capability
            //if (moduleCapabilities.Gaming)
            {
                sw.Restart();
                //moduleGroup.AddHeader("General", typeof(GamingModule));
                moduleGroup.AddHeader(Strings.RightViewHeader_General, typeof(GamingModule), Constants.ModuleName_Gaming);
                sw.Stop();
                _log?.Info($"* GamingModule ctor consume {sw.ElapsedMilliseconds} msec");
            }
            //If the monitor has VisionEngine capability
            //if (moduleCapabilities.VisionEngine)
            {
                sw.Restart();
                //moduleGroup.AddHeader("Vision Engine", typeof(VisionEngineModule));
                moduleGroup.AddHeader(Strings.RightViewHeader_VisionEngine, typeof(VisionEngineModule), Constants.ModuleName_VisionEngine);
                sw.Stop();
                _log?.Info($"* VisionEngineModule ctor consume {sw.ElapsedMilliseconds} msec");
            }

            //If this ModuleGroup has any item, then add into moduleGroups
            if (moduleGroup.HeaderCount > 0)
            {
                groups.Add(moduleGroup);
            }

            /*//Group[3.5] Monitor Audio
            //         Header[0] Audio
            moduleGroup = new ModuleGroup()
            {
                GroupName = Constants.GroupName_MonitorAudio, // "Audio",
                VbarText = Strings.VbarText_MonitorAudio,
                IconTemplate = (ControlTemplate)this.TryFindResource("iconTemplate_MonitorAudio"),
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Display.MonitorAudio.png"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.DisplayMonitorAudio)
            };
            {
                sw.Restart();
                moduleGroup.AddHeader(Strings.VbarText_MonitorAudio, typeof(MonitorAudioModule), Constants.ModuleName_MonitorAudio);
                sw.Stop();
                _log?.Info($"* MonitorAudioModule ctor consume {sw.ElapsedMilliseconds} msec");
            }
            //If this ModuleGroup has any item, then add into moduleGroups
            if (moduleGroup.HeaderCount > 0)
            {
                groups.Add(moduleGroup);
            }*/

            //Group[4] KVM
            //         Header[0] KVM,   KvmModule
            moduleGroup = new ModuleGroup()
            {
                GroupName = Constants.GroupName_KVM, // "KVM",
                VbarText = Strings.VbarText_KVM,
                IconTemplate = (ControlTemplate)this.TryFindResource("iconTemplate_KVM"),
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Display.KVM.png"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.DisplayKVM)
            };
            //If the monitor has KVM capability
            //if (moduleCapabilities.Kvm)
            {
                sw.Restart();
                //moduleGroup.AddHeader("KVM", typeof(KvmModule));
                moduleGroup.AddHeader(Strings.VbarText_KVM, typeof(KvmModule), Constants.ModuleName_KVM);
                sw.Stop();
                _log?.Info($"* KvmModule ctor consume {sw.ElapsedMilliseconds} msec");
            }

            //If this ModuleGroup has any item, then add into moduleGroups
            if (moduleGroup.HeaderCount > 0)
            {
                groups.Add(moduleGroup);
            }

            //Group[5] Others
            //         Header[0] Others,   DisplayOthersModule
            moduleGroup = new ModuleGroup()
            {
                GroupName = Constants.GroupName_DisplayOthers, //  "Others",
                VbarText = Strings.VbarText_DisplayOthers,
                IconTemplate = (ControlTemplate)this.TryFindResource("iconTemplate_DisplayOthers"),
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Display.Others.png"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.DisplayOthers)
            };
            //If the monitor has DisplayOthers capability
            //if (moduleCapabilities.DisplayOthers)
            {
                sw.Restart();
                //moduleGroup.AddHeader("Others", new DisplayOthersModule() { SelectedHomeDevice = _ivm?.SelectedHomeDevice });
                moduleGroup.AddHeader(Strings.VbarText_DisplayOthers, new DisplayOthersModule() { SelectedHomeDevice = _ivm?.SelectedHomeDevice });
                sw.Stop();
                _log?.Info($"* DisplayOthersModule ctor consume {sw.ElapsedMilliseconds} msec");
            }

            //If this ModuleGroup has any item, then add into moduleGroups
            if (moduleGroup.HeaderCount > 0)
            {
                groups.Add(moduleGroup);
            }

            //Group[6] Webcam
            //         Header[0] Webcam,   DisplayWebcamModule
            _displayWebCamModule = new DisplayWebcamModule(_webCameraViewModel);
            moduleGroup = new ModuleGroup()
            {
                GroupName = Constants.GroupName_DisplayWebcam, //  "Others",
                VbarText = LangHelper.Instance["AddDevice.Webcam"],
                IconTemplate = (ControlTemplate)this.TryFindResource("iconTemplate_DisplayWebcam"),
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Vbar.Display.Webcam.png"),
                GroupIconCanvas = DdpmCommonHelper.CanvasIconCreator(VbarIcon.DisplayWebcam)
            };
            //If the monitor has DisplayOthers capability
            //if (moduleCapabilities.DisplayOthers)
            {

                sw.Restart();
                var webCamSettingModule = new WebCameraSettingsModule(_webCameraViewModel);
                UpdateFieldOrProperty(webCamSettingModule, "_leftView", _displayWebCamModule.GetLeftView());
                moduleGroup.AddHeader(LangHelper.Instance["Camera.0"], webCamSettingModule);
                sw.Stop();
                _log?.Info($"* DisplayWebcamModule ctor consume {sw.ElapsedMilliseconds} msec");
            }

            {
                sw.Restart();
                moduleGroup.AddHeader(LangHelper.Instance["Camera.1"], new WebCameraColorImageModule(_webCameraViewModel));
                sw.Stop();
                _log?.Info($"* VisionEngineModule ctor consume {sw.ElapsedMilliseconds} msec");
            }

            {
                sw.Restart();
                moduleGroup.AddHeader(LangHelper.Instance["Camera.3"], new WebCameraCaptureModule(_webCameraViewModel));
                sw.Stop();
                _log?.Info($"* VisionEngineModule ctor consume {sw.ElapsedMilliseconds} msec");
            }

            //If this ModuleGroup has any item, then add into moduleGroups
            if (moduleGroup.HeaderCount > 0)
            {
                groups.Add(moduleGroup);
            }

            return groups;
        }
        public bool UpdateFieldOrProperty(object target, string name, object value)
        {
            try
            {
                Type type = target.GetType();
                var props = type.GetProperties();
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                if (false == props.Any(x => x.Name.Equals(name)) &&
                    false == fields.Any(x => x.Name.Equals(name)))
                    return false;

                var prop = props.FirstOrDefault(x => x.Name.Equals(name));
                if (null != prop)
                {
                    prop.SetValue(target, value);
                    return true;
                }

                var field = fields.FirstOrDefault(x => x.Name.Equals(name));
                if (null != field)
                {
                    field.SetValue(target, value);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        //private bool SupportNKVM(string ModelName)
        //{
        //    if (string.IsNullOrEmpty(ModelName))
        //    {
        //        if (ModelName.IndexOf("P2425E") != -1 || ModelName.IndexOf("P2425HE") != -1 || ModelName.IndexOf("P2725HE") != -1)
        //        {
        //            return false;
        //        }
        //        string strSupport = ModelName.Substring(4,1);
        //        switch (strSupport)
        //        {
        //            case "U":
        //            case "C":
        //                return true;
        //        }
        //    }
        //    return false;
        //}

        #endregion Init for Modules

        #region Vbar

        private const int VbarItemId_DisplaySettings = 0;
        private const int VbarItemId_InputSource = 1;
        private const int VbarItemId_EasyArrange = 2;
        private const int VbarItemId_KVM = 3;
        private const int VbarItemId_Others = 4;

#if USE_VBARITEM1

        /// <summary>
        /// Handler when a VbarItem is clicked (MouseLeftButtonDown event)
        /// The event handler is install in DisplayPage ctor:
        ///     _ivm.VbarItemClickCommand = new RelayCommand<VbarItem>(OnVbarItemClicked);
        /// </summary>
        /// <param name="newItem">The VbarItem which is clicked</param>
        private void OnVbarItemClicked(VbarItem1? newItem)
        {
            //If we are in Landing mode
            if (_ivm?.VbarSelectedIndex == -1)
            {
                /*
                //Transit to TwoView mode
                InvokeGotoTwoViewModeAnimation();

                RightFrame.Visibility = Visibility.Visible;
                */
            }

            //Determine the selection index of Vbar items[]
            int newSelectedVarItem = -1;
            if ((newItem != null) && (_ivm != null))
            {
                newSelectedVarItem = newItem.Index;
            }

            //Check if VbarItem selection is NOT changed, if Yes, noting to do
            if (newSelectedVarItem == _ivm?.VbarSelectedIndex)
                return;

            if (_ivm != null)
            {
                //Change the Vbar item selection index
                _ivm.VbarSelectedIndex = newSelectedVarItem;

                //Due to RightViewHeaderCtrl has no SelectionChanged event
                //
                if (_ivm != null)
                {
                    /*
                    if (_ivm.RightViewHeaders != null)
                    {
                        rightViewHeaderCtrl.SetHeaders(_ivm.RightViewHeaders.ToArray());
                    }
                    */
                }
            }
        }

#else
        /// <summary>
        /// Handler when a VbarItem is clicked (MouseLeftButtonDown event)
        /// The event handler is install in DisplayPage ctor:
        ///     _ivm.VbarItemClickCommand = new RelayCommand<VbarItem>(OnVbarItemClicked);
        /// </summary>
        /// <param name="newItem">The VbarItem which is clicked</param>
        private void OnVbarItemClicked(VbarItem? newItem)
        {
            //If we are in Landing mode
            if (_ivm?.VbarSelectedIndex == -1)
            {
                /*
                //Transit to TwoView mode
                InvokeGotoTwoViewModeAnimation();

                RightFrame.Visibility = Visibility.Visible;
                */
            }

            //Determine the selection index of Vbar items[]
            int newSelectedVarItem = -1;
            if ((newItem != null) && (_ivm != null))
            {
                newSelectedVarItem = newItem.Id;
            }

            //Check if VbarItem selection is NOT changed, if Yes, noting to do
            if (newSelectedVarItem == _ivm?.VbarSelectedIndex)
                return;

            if (_ivm != null)
            {
                //Change the Vbar item selection index
                _ivm.VbarSelectedIndex = newSelectedVarItem;

                //Due to RightViewHeaderCtrl has no SelectionChanged event
                //
                if (_ivm != null)
                {
                    /*
                    if (_ivm.RightViewHeaders != null)
                    {
                        rightViewHeaderCtrl.SetHeaders(_ivm.RightViewHeaders.ToArray());
                    }
                    */
                }
            }
        }
#endif

        //Robert_Lin 2024-4-25, unused.
        /*
        private RightViewHeader[] headerDisplaySettings = new RightViewHeader[]
        {
            new RightViewHeader(101, "Brightness/Contrast", new BrightnessModule())
            ,new RightViewHeader(102, "Color",new ColorModule())
            ,new RightViewHeader(103,"Display Properties", new EzArrangeModule())
        };*/
        //Robert_Lin 2024-4-25, unused.
        //private int displaySettingsSelIdx = -1;

        //Robert_Lin 2024-4-25, unused.
        private void VbarItem_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (sender == null)
                return;

            //If we are in Landing mode
            //if (_ivm?.VbarSelectedIndex == -1)
            if (_ivm?.GroupSelIdx == -1)
            {
                //Transit to TwoView mode
                InvokeGotoTwoViewModeAnimation();

                //Set the RightViewHeader seklection to 0
                _ivm.RightViewHeaderSelectedIndex = 0;

                RightFrame.Visibility = Visibility.Visible;
            }

            if (sender is VbarItem)
            {
                VbarItem item = (VbarItem)sender;

                //Check if VbarItem selection is NOT changed
                if (item.Id == _ivm?.VbarSelectedIndex)
                    return;

                if (_ivm != null)
                {
                    _ivm.VbarSelectedIndex = item.Id;

                    //Change RightView headers
                    RightViewHeader[] newHeader = dictModuleMap[_ivm.VbarSelectedIndex];
                    rightViewHeaderCtrl.SetHeaders(newHeader);

                    //Restore header selectedIndex
                }

                SwitchModule();

                switch (item.Id)
                {
                    case VbarItemId_DisplaySettings:
                        //LoadDisplaySettings();
                        break;

                    case VbarItemId_InputSource:
                    case VbarItemId_EasyArrange:
                    case VbarItemId_KVM:
                        //RightFrame.Content = GetTempRightView();
                        break;
                }
            }*/
        }

        //Robert_Lin 2024-4-25, unused.
        private void LoadDisplaySettings()
        {
            ////First time load
            //if (displaySettingsSelIdx < 0)
            //{
            //    displaySettingsSelIdx = 0;
            //    RightFrame.Visibility = Visibility.Visible;
            //}
            //rightViewHeaderCtrl.SelectedId = displaySettingsSelIdx;
            //rightViewHeaderCtrl.SetHeaders(headerDisplaySettings);

            //SwitchLeftRightView();
        }

        //Robert_Lin 2024-4-25, Can remove this method after confirm Left/Right View switch
        //  can be done with DataBinding from xaml code and ViewModel
        private void SwitchLeftRightView()
        {
            //RightFrame.Content = headerDisplaySettings[displaySettingsSelIdx].DdpmModule?.GetRightView();

            //if (headerDisplaySettings[displaySettingsSelIdx].DdpmModule?.GetLeftView() == null)
            //    LeftFrame.Content = GetDefaultLeftView();
            //else
            //    LeftFrame.Content = headerDisplaySettings[displaySettingsSelIdx].DdpmModule?.GetLeftView();
        }

        #endregion Vbar

        #region RightViewHeader

        //Robert_Lin 2024-4-25, This method is required currently.
        // It should be able to remove when we can auto switch RightView
        // with ViewModel and DataBinding.
        //
        private void RightViewHeaderCtrl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;
            /*
            int newSelId = rightViewHeaderCtrl.SelectedIndex;
            if (newSelId != displaySettingsSelIdx)
            {
                if (_ivm != null)
                {
                    _ivm.RightViewHeaderSelectedIndex = newSelId;
                }
                displaySettingsSelIdx = newSelId;
                SwitchLeftRightView();
            }
            */
        }

        #endregion RightViewHeader

        #region Mode Change

        private void InvokeGotoTwoViewModeAnimation()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                Storyboard sb = (Storyboard)this.FindResource("StoryGotoTwoView");
                if (sb != null)
                {
                    sb.Completed += (o, s) =>
                    {
                    };

                    sb.Begin();
                }
            }));
        }

        #endregion Mode Change

        #region LeftView

        private UserControl? _defLeftView;

        private UserControl GetDefaultLeftView()
        {
            if (_defLeftView == null)
            {
                HomeDevice dev = new HomeDevice()
                {
                    DeviceName = "Display 1",
                    DeviceCategory = eDeviceCategory.Display,
                    DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Product_Display.png")
                };
                _defLeftView = new DisplayDefaultLeftView();
                _defLeftView.DataContext = dev;
            }
            return (UserControl)_defLeftView;
        }

        #endregion LeftView

        #region Nav

        private void leftArrow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            //Always reset to Landing mode
            if (_ivm != null)
            {
                //_ivm.GroupSelIdx = -1;
                //_ivm.VbarSelectedIndex = -1;
            }

            //Return to DdpmHomePage
            IConsole? console = DisplayPlugin.PluginIoc.GetService<IConsole>();
            console?.ShowPluginById(DDPM.UI.Common.Constants.DdpmHomePluginId);
        }

        private void OnLeftArrowClick(object sender, RoutedEventArgs e)
        {
            _log?.Info($"@DisplayPage.OnLeftArrowClick, MemoryUsage: {DdpmCommonHelper.GetProcessMemoryUsageMB():F2} MB");
            //Return to DdpmHomePage
            IConsole? console = DisplayPlugin.PluginIoc.GetService<IConsole>();
            console?.ShowHomePage();

            //console?.ShowPluginById(DDPM.UI.Common.Constants.DdpmHomePluginId);
        }

        private void leftView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // For hiding restore to default button.
            //_ivm.SelectedHomeDevice.IsRestoreBtnVisible = Visibility.Collapsed;

            // Jim 20250108 to fix PIMS-340036 [DDPM Win 2.0][R19]Connect two monitors,switch the drop-down menu. One DUT of the Restore to default icons disappears.
            _ivm.SelectedHomeDevice.IsRestoreBtnVisible = Visibility.Visible;
        }

        #endregion Nav

        //private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //}

        //Robert_Lin,2024-7-26, Unused, move to DdpmHomePlugin
        //private void OnAddDeviceClicked(object sender, EventManagerArgs e)
        //{
        //}

        #region Lock/unlock event
        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            bool? isInputSourceLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_ActiveInputSource", e);
            bool? isEALocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_EasyArrangeLayout", e);

            Dispatcher.Invoke(new Action(() =>
            {
                //Apply lock status to VBarItem
                if (isInputSourceLocked != null)
                {
                    basePage.SetLockModuleGroup(Constants.GroupName_InputSource, isInputSourceLocked == true);
                    Trace.WriteLine($"Apply InputSource(Lock) : {isInputSourceLocked}");
                }
                if (isEALocked != null)
                {
                    basePage.SetLockModuleGroup(Constants.GroupName_EasyArrange, isEALocked == true);
                    Trace.WriteLine($"Apply EasyArrange(Lock) : {isEALocked}");
                }
            }));
        }
        #endregion

        #region Handle DDC/CI ON/OFF events

        //2024-8-1 Robert_Lin move to Display Plugin
        private void _deviceManagerSA_DDCCIStatuschanged(object? sender, VcpCore.Common.DDCCIchangedEventArgs e)
        {
            HandleDdcCiOnOffEvent(e.monitors, e.DDCisON);
        }

        private void HandleDdcCiOnOffEvent(MonitorInfo mi, bool isDdcCiOn)
        {
            IModuleOwner? moduleOwner = null;
            if (_vmDisplay != null)
            {
                moduleOwner = _vmDisplay;
            }
            else
            {
                moduleOwner = DdpmCommonHelper.ModuleOwner;
            }

            List<HomeDevice> homeDevices;
            if (_vmDisplay != null)
            {
                homeDevices = _vmDisplay.HomeDevices;
            }
            else
            {
                homeDevices = moduleOwner?.HomeDevices;
            }

            if (homeDevices == null)
                return;

            foreach (HomeDevice homeDev in homeDevices)
            {
                if (homeDev.DeviceCategory != eDeviceCategory.Display)
                    continue;

                //If the homeDev is the monitor that DDC/CI is changed
                if (HomeDevice.IsSameMonitor(homeDev.MonitorInfo, mi, "DDCisON"))
                {
                    //Update the DDCisON status
                    homeDev.MonitorInfo.DDCisON = isDdcCiOn;

                    //If the homeDev is current SelectedHomeDevice (is displaying)
                    //then notify ModuleOwner to update UI
                    HomeDevice? selDev = GetSelectedHomeDevice();
                    if (selDev != null &&
                        HomeDevice.IsSameMonitor(selDev.MonitorInfo, mi, "DDCisON"))
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            DeviceBasePageViewModel vmBase = basePage.DataContext as DeviceBasePageViewModel;
                            if (vmBase != null)
                                vmBase.HandleDdcCiOffEvent(isDdcCiOn);
                        }));
                    }
                }
                else
                {
                    //Robert_Lin, 2024-8-6, not the monitor which DDC/CI is chnaged
                    //then nothing to do.
                    //homeDev.MonitorInfo.DDCisON = isDdcCiOn;
                }
            }
        }

        #endregion Handle DDC/CI ON/OFF events

        #region ModuleOwner

        public HomeDevice? GetSelectedHomeDevice()
        {
            //Using DisplayPageViewModel
            if (_ivm != null)
            {
                return _ivm?.SelectedHomeDevice;
            }
            if (_vmDisplay != null)
            {
                return _vmDisplay?.SelectedHomeDevice;
            }
            if (DdpmCommonHelper.ModuleOwner != null)
            {
                return DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
            }
            return null;
        }

        #endregion ModuleOwner

        #region Exit
        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //Robert_Lin 2025-4-16, Problem found if we call Dispose() here
            //When the Unloaded will be invoked? Ans: When Theme changed
            //Problems found:
            //1 Cannot change RightView when change VbarItem
            //2 Click [BackArrow] button cannot return to homepage.
            //Root cause:
            //1 The DisplayPage.Dispose() will call to 
            //  basePage.LeftArrowClick -= OnLeftArrowClick;
            //  basePage.Dispose();
            //  And basePage.Dispose() will release its ViewModel.
            //Workaround solution:
            //1 Move this DisplayPage.Dispose() call to destructor of DisplayPage
            _log?.Info("DisplayPage.UserControl_Unloaded");
            //Dispose();
            _log?.Info("DisplayPage.UserControl_Unloaded exit");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // 釋放託管資源
                    if (_ivm != null)
                    {
                        _ivm.VbarItemClickCommand = null;

                    }

                    basePage.LeftArrowClick -= OnLeftArrowClick;
                    basePage.Dispose();

                    if (_deviceManagerSA != null)
                    {
                        _deviceManagerSA.DDCCIStatuschanged -= _deviceManagerSA_DDCCIStatuschanged;
                        _deviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;

                    }

                }

                // 釋放非託管資源
                //if (unmanagedResource != IntPtr.Zero)
                //{
                //    // 釋放資源
                //    unmanagedResource = IntPtr.Zero;
                //}
                moduleGroup.Dispose();
                _isDisposed = true;
            }
        }
        ~DisplayPage()
        {
            Dispose(false);
        }
        #endregion Exit
    }
}