using Newtonsoft.Json;
using System.Text;


namespace DdmLibrary.Utility
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class FriendlyName
    {
        public int Input { get; set; }
        public string Name { get; set; }
        public FriendlyName(int input, string name)
        {
            Input = input;
            Name  = name;
        }
    }
    public class Input
    {
        public List<FriendlyName> FriendlyNames { get; set; }
        public List<int> SwitchInputHotkeysInfo { get; set; }
        public List<int> Toogle2InputHotkeysInfo { get; set; }
        public int FavoriteHotkeyInput { get; set; }
        //deprecated
//        public List<SwitchInputHotkey> SwitchInputHotkeys { get; set; }
        //deprecated
//        public Toogle2InputHotkeys Toogle2InputHotkeys { get; set; }
        public Input()
        {
            FriendlyNames = new List<FriendlyName>();
            SwitchInputHotkeysInfo = new List<int>();
            Toogle2InputHotkeysInfo = new List<int>();
            //deprecated
//            SwitchInputHotkeys = new List<SwitchInputHotkey>();
        }
    }

    public class Display
    {
        public bool ProjectInExtend { get; set; }
        public bool ApplicationSettingsImportDoNotPrompt { get; set; }
        public bool ApplicationSettingsImportYesOrNo { get; set; }
        public DEVMODE devmode { get; set; }
        public int scale { get; set; }
        public DisplayConfigRotation orientation { get; set; }
        public bool SmartHDR { get; set; }

        public Display()
        {
            ProjectInExtend                      = false;
            ApplicationSettingsImportDoNotPrompt = false;
            ApplicationSettingsImportYesOrNo     = true;
            devmode                              = new();
            scale                                = 100;
            orientation                          = DisplayConfigRotation.Identity;
            SmartHDR                             = false;
        } 
    }

    public class AppInfo
    {
        public string Name { get; set; }
        public string ExeName { get; set; }
        public string Path { get; set; }
        public int Color { get; set; }
        public int HDRColor { get; set; }
        public string ExeArg { get; set; }
        public AppInfo(string name, string exeName, string path, int color = 0, int HDRcolor = -1, string arg = "")
        {
            Name = name;
            ExeName = exeName;
            Path = path;
            Color = color;
            HDRColor = HDRcolor;
            ExeArg = arg;
        }
    }

    public class ColorBinding
    {
        public int Color { get; set; }
        public int Brightness { get; set; }
        public int Contrast { get; set; }

        public ColorBinding(int color, int brightness, int contrast)
        {
            Color = color;
            Brightness = brightness;
            Contrast = contrast;
        }

        public ColorBinding Clone()
        {
            return new ColorBinding(Color, Brightness, Contrast);
        }

        
    }

    public class ColorPreset
    {
        public bool LockAuto { get; set; }
        public bool Auto { get; set; }
        public List<AppInfo> AppInfos { get; set; }
        public List<ColorBinding> ColorBindings { get; set; }
        public int ColorManagement { get; set; }
        public ColorPreset()
        {
            LockAuto        = false; //disable automode
            Auto            = false;
            AppInfos        = new List <AppInfo> ();
            ColorBindings   = new List <ColorBinding> ();
            ColorManagement = 0x2;
        }

      
    }


    public class EAAppInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsUWP { get; set; }
        public string AppUserModelID { get; set; }
        public string Param { get; set; }
        public EAAppInfo()
        {

        }

        public EAAppInfo(string name, string path, bool isUWP, string appUserModelID, string param)
        {
            Name = name;
            Path = path;
            IsUWP = isUWP;
            AppUserModelID = appUserModelID;
            Param = param;
        }

        public bool IsAnyIllegal()
        {
            // Name
            // Path
            // IsUWP
            // AppUserModelID
            // Param

            return false;
        }
    }

    public class ProfileSetting
    {
        public int ID { get; set; }
        public bool Auto { get; set; }
        public long? AutoStartTime { get; set; }
        public bool StartUpLaunch { get; set; }       

        public ProfileSetting(int id, bool auto, long autostarttime, bool startuplaunch)
        {
            ID = id;
            Auto = auto;
            AutoStartTime = autostarttime;
            StartUpLaunch = startuplaunch;
        }

      
    }

    //deprecated
    public class Profile
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Layout { get; set; }
        public bool Auto { get; set; }
        public long? AutoStartTime { get; set; }
        public bool StartUpLaunch { get; set; }
        public List<EAAppInfo> AppInfos { get; set; }

        public Profile(int id, string name, int layout, bool auto, long autostarttime, bool startuplaunch, List<EAAppInfo> apps)
        {
            ID = id;
            Name = name;
            Layout = layout;
            Auto = auto;
            AutoStartTime = autostarttime;
            StartUpLaunch = startuplaunch;
            AppInfos = (apps == null ? new List<EAAppInfo>() : apps.ConvertAll(app => new EAAppInfo(app.Name, app.Path, app.IsUWP, app.AppUserModelID, app.Param)));
        }

        
    }

    public class Desktop
    {
        public string ID { get; set; }
        public int Index { get; set; }
        public int ActiveLayout { get; set; }
        public List<int> LayoutMRU { get; set; }
        public List<int> ProfileMRU { get; set; }
 //       public List<CustLayout> CustLayouts { get; set; }
        public List<Profile> Profiles { get; set; } //deprecated
        public List<ProfileSetting> ProfileSettings { get; set; }
        public Desktop(string id, int activelayout)
        {
            ID = id;
            ActiveLayout = activelayout;
            LayoutMRU = new List<int>();
            LayoutMRU.Add(1);
            LayoutMRU.Add(2);
            LayoutMRU.Add(3);
            LayoutMRU.Add(4);
            LayoutMRU.Add(8);
            ProfileMRU = new List<int>();
 //           CustLayouts = new List<CustLayout>();
            Profiles = new List<Profile>(); //deprecated
            ProfileSettings = new List<ProfileSetting>();
        }

        
    }

    public class EasyArrangement
    {
        public const int maxMRU = 5;
        public List <Desktop> Desktops { get; set; }
        public EasyArrangement()
        {
            Desktops = new List<Desktop>();
        }

        
    }

    public class USB
    {
        public bool WizardRuned { get; set; }
        public bool AutoSwitchUSB { get; set; }
        public List <int> PCs { get; set; }

        public USB()
        {
            WizardRuned   = false;
            AutoSwitchUSB = false;
            PCs = new List<int>();
        }

        
    }

    public class Network
    {
        public enum SetupType
        {
            None,
            Single,
            Multiple
        }
        public bool WizardRuned { get; set; }
        public int LastSetupType { get; set; }
        public Network()
        {
            WizardRuned = false;
            LastSetupType = (int) SetupType.None;
        }

        
    }

    public class KVM
    {
        public enum KVMType
        {
            USB,
            Network
        }
        public int LastUsed { get; set; }
        public USB USB { get; set; }
        public Network Network { get; set; }

        public KVM()
        {
#if !FEAT_NETWORKKVM
            LastUsed = (int)KVMType.USB;
#else
            LastUsed = (int)KVMType.Network;
#endif
            USB = new USB();
            Network = new Network();
        }

        
    }

    public class Personalize
    {
        public enum Menu
        {
            Brightness,            
            Color,
            Display,
            EasyArrangement,
            Gaming,
            KVM,
            Audio,
            WebCam            
        }
        public List <int> Launcher { get; set; }
        public Personalize()
        {
            Launcher = new List<int>();
        }

        
    }

    public class Others
    {
        public const int PowerNapOptions_ON_BitMask     = 0x1;
        public const int PowerNapOptions_Reduce_BitMask = 0x2;
        public const int PowerNapOptions_Sleep_BitMask  = 0x4;
        public int PowerNap { get; set; }
        
        public Others()
        {
            PowerNap = 0x2; // PowerNap �w�] Off �B�]�� reduce
        }

        
    }

    public class VCP
    {
        public int Code { get; set; }
        public List <int> Value { get; set; }

        public VCP()
        {
        }

        public VCP(int code, int value)
        {
            Code = code;
            Value = new List<int>();
            Value.Add(value);
        }

        public VCP(int code, List<int> value)
        {
            Code = code;
            Value = ((value == null) ? new List<int>() : new List<int>(value));
        }

        
    }

    public class DisplayInfo
    {
        public int Resolution_w { get; set; }
        public int Resolution_h { get; set; }
        public double RefreshRate { get; set; }
        public DisplayInfo()
        {
        }
        public DisplayInfo(int resolution_w, int resolution_h, double refreshrate)
        {
            Resolution_w = resolution_w;
            Resolution_h = resolution_h;
            RefreshRate = refreshrate;
        }

        
    }

    #region Schedule Brightness/Contrast
    public class BriConProfile
    {
        public bool IsOverwrittenPresetName { get; set; }
        public string PresetName { get; set; }
        /// <summary>
        /// "hh:mm tt" format
        /// </summary>
        public string Time { get; set; }
        /// <summary>
        /// unit is minute 
        /// </summary>
        public byte Duration { get; set; }
        public ushort Brightness { get; set; }
        public ushort Contrast { get; set; }

        
    }

    public class BriConSchedule
    {
        public bool IsOverwrittenTargetValue { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsSync { get; set; }
        public BriConProfile Profile1 { get; set; }
        public BriConProfile Profile2 { get; set; }

        public BriConSchedule(string model=null)
        {
            IsOverwrittenTargetValue = false;
            IsEnabled = false;
            IsSync = false;
            Profile1 = new BriConProfile
            {
                IsOverwrittenPresetName = false,
                PresetName = "Day",
                Time = "08:00 AM",
                Duration = 60,
                Brightness = 75,
                Contrast = 75
            };
            Profile2 = new BriConProfile
            {
                IsOverwrittenPresetName = false,
                PresetName = "Night",
                Time = "05:00 PM",
                Duration = 60,
                Brightness = 50,
                Contrast = 50
            };
        }
    }
    #endregion

    public class DDMMonitorSettings
    {
        public const int CMOptions_On_BitMask              = 0x1;
        public const int CMOptions_ChangebyMonitor_BitMask = 0x2;
        public const int CMOptions_ChangebyProfile_BitMask = 0x4;

        [JsonProperty("Version:")]
        public double Version { get; set; } 
        public string OS { get; set; }
        public string Model { get; set; }
        public string ServiceTag { get; set; }
        public Input Input { get; set; }
        public Display Display { get; set; }
        public ColorPreset ColorPreset { get; set; }
//        public Gaming Gaming { get; set; }
        public EasyArrangement EasyArrangement { get; set; }
        public KVM KVM { get; set; }
        public Personalize Personalize { get; set; }
        public Others Others { get; set; }
        public List<VCP> VCPs { get; set; }
        public DisplayInfo DisplayInfo { get; set; }
        public BriConSchedule BriConSchedule { get; set; }

        [JsonConstructor]
        public DDMMonitorSettings()
        {
            Version = 1.2;
            OS = "Windows";
            Model = "";
            ServiceTag = "";
            Input = new Input();
            Display = new Display();
            ColorPreset = new ColorPreset();
            ColorPreset.AppInfos = new List<AppInfo>();
            //            Gaming = new Gaming();
            EasyArrangement = new EasyArrangement();
            KVM = new KVM();
            Personalize = new Personalize();
            Others = new Others();
            VCPs = new List<VCP>();
            DisplayInfo = new DisplayInfo();
            BriConSchedule = new BriConSchedule();
        }

        public static bool restoreDDMMonitorSettings(ref DDMMonitorSettings monitorSettings, string filename)
        {
            byte[] content = default;

            try
            {
                content = File.ReadAllBytes(filename);
            }
            catch
            {
                return false;
            }

            //Decrypted with AESKey in new setting file
            var key = Decryption.GetAESKeyFromKeyContainer();
            var decryptAES = Decryption.Decrypt(content, key);
            content = decryptAES;
            string JsonConetent = Encoding.UTF8.GetString(content, 0, content.Length);
            monitorSettings = JsonConvert.DeserializeObject<DDMMonitorSettings>(JsonConetent);

            return true;
        }
    }
}

