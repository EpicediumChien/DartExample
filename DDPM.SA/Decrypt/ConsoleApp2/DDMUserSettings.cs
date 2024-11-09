using Newtonsoft.Json;
using System.Text;



namespace DdmLibrary.Utility
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Hotkey
    {
        public enum HotkeyFun
        {
            PIPPosition,
            PBPZoom,
            UnderScan,
            VisionEngineToggle,
            DarkStablizer,
            USBKVMToggleInputSource,
            USBSwitch,
            Swapping2PIPPBP,

            MenuLauncher,
            MainMenu,
            EAMRU,
            NetworkKVMToggleInputSource,
            //            Input1, //deprecated
            //            Input2, //deprecated
            //            Input3, //deprecated
            //            Input4, //deprecated
            Toggle2InputSource,
            ToggleInputSource,
            FavoriteHotkeyInput,
            BCBrightness1,
            BCBrightness2,
            BCContrast1,
            BCContrast2,
            KVMRestoreMouseCursor,
            DualResolution,
        }
        public int Function { get; set; }
        public List<int> Keys { get; set; }

        public Hotkey(int fun, List<int> keys)
        {
            Function = fun;
            Keys = (keys == null ? new List<int>() : new List<int>(keys));
        }
    }
    public class EAProfile
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Layout { get; set; }
        public List<EAAppInfo> AppInfos { get; set; }

        public EAProfile(int id, string name, int layout, List<EAAppInfo> apps)
        {
            ID = id;
            Name = name;
            Layout = layout;
            AppInfos = (apps == null ? new List<EAAppInfo>() : apps.ConvertAll(app => new EAAppInfo(app.Name, app.Path, app.IsUWP, app.AppUserModelID, app.Param)));
        }

        
    }


    public class DDMUserSettings
    {
        public enum NetworkDataAccessState
        {
            Unknow,
            Yes,
            No,
            Pending,
            NotChinaSKU,
        }

        public enum Themes
        {
            Dark,
            Light
        }

        public enum Languages
        {
            en,//英語
            de,//德語
            es,//西班牙
            fr,//法語
            ja_JP,//日語
            pt_BR,//葡萄牙語
            ru_RU,//俄羅斯
            zh_CN,//簡中
            zh_TW,//繁中
            ko_KR,//韓語
            it_IT,//義大利
            preferred//windows preferred Language
        }

        public const int maxCustLayout = 5;
        public const int CustLayoutBase = 1000;
        public const int maxProfile = 9;

        public double Version { get; set; } // 此值需大於 0 且小於 20。若未來增長至大於 20，則需改動 isAnyIllegal
        public string OS { get; set; }
        public bool SnapEnable { get; set; }
        public bool DisplayMatrixEnable { get; set; }
        public int ColorSchemes { get; set; }
        public bool EAWithoutGap { get; set; }
        public bool EAWithShiftKey { get; set; }
        public bool EASpan { get; set; }
        public int Language { get; set; }
        public bool AutoStart { get; set; }
        public bool OnScreenNotification { get; set; }
        public bool AutoCheckUpdate { get; set; }
        public bool AllowTelemetry { get; set; }
        public bool SilentShowTelemetry { get; set; }
        public bool TelemetryInit { get; set; }
        public bool ShowTelemetryUI { get; set; }
        public bool ImportPermission { get; set; }
        public List<Hotkey> Hotkeys { get; set; }
        public List<CustLayout> CustLayouts { get; set; }
        public Dictionary<string, string> OTAPrompt { get; set; }
        public string OTANextCheckTime { get; set; }
        public bool AutoRestoreWindowLayout { get; set; }
        public List<EAProfile> Profiles { get; set; }
        public NetworkDataAccessState NetworkDataAccess { get; set; } //MDDM-3787
        public bool LockRotate { get; set; } = false; // 20230406 MDDM-4247
        public bool ScheduleBriConIsSync { get; set; }
        public string LastImportedMonitor { get; set; }

        private static readonly object Lock = new object();

        private static List<Hotkey> DefaultHotkeys = new List<Hotkey>()
        {
            //MDDM-2958 Clear all default hotkeys. (2.0.0.138)
            new Hotkey((int)Hotkey.HotkeyFun.Swapping2PIPPBP        , new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.PIPPosition            , new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.PBPZoom                , new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.UnderScan              , new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.VisionEngineToggle     , new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.DarkStablizer          , new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.DualResolution         , new List<int>(){ }),

            new Hotkey((int)Hotkey.HotkeyFun.USBKVMToggleInputSource, new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.USBSwitch              , new List<int>(){ }),

            new Hotkey((int)Hotkey.HotkeyFun.MenuLauncher           , new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.MainMenu               , new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.EAMRU                  , new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.Toggle2InputSource     , new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.ToggleInputSource      , new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.FavoriteHotkeyInput    , new List<int>(){ }),

            //MDDM-4060
            new Hotkey((int)Hotkey.HotkeyFun.BCBrightness1    , new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.BCBrightness2    , new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.BCContrast1    , new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.BCContrast2    , new List<int>(){ }),
#if FEAT_NETWORKKVM
            new Hotkey((int)Hotkey.HotkeyFun.NetworkKVMToggleInputSource, new List<int>(){ }),
            new Hotkey((int)Hotkey.HotkeyFun.KVMRestoreMouseCursor, new List<int>(){ }),
#endif
            #region -default hotkey
            /*
            new Hotkey((int)Hotkey.HotkeyFun.Swapping2PIPPBP        , new List<int>(){(int)Keys.Alt, (int)Keys.W}),
            new Hotkey((int)Hotkey.HotkeyFun.PIPPosition            , new List<int>(){(int)Keys.Alt, (int)Keys.Q}),
            new Hotkey((int)Hotkey.HotkeyFun.PBPZoom                , new List<int>(){(int)Keys.Alt, (int)Keys.Z}),
            new Hotkey((int)Hotkey.HotkeyFun.UnderScan              , new List<int>(){(int)Keys.Alt, (int)Keys.A}),
            new Hotkey((int)Hotkey.HotkeyFun.VisionEngineToggle     , new List<int>(){(int)Keys.Alt, (int)Keys.T}),
            new Hotkey((int)Hotkey.HotkeyFun.DarkStablizer          , new List<int>(){(int)Keys.Alt, (int)Keys.S}),

            new Hotkey((int)Hotkey.HotkeyFun.USBKVMToggleInputSource, new List<int>(){(int)Keys.Alt, (int)Keys.P}),
            new Hotkey((int)Hotkey.HotkeyFun.USBSwitch              , new List<int>(){(int)Keys.Alt, (int)Keys.U}),

            new Hotkey((int)Hotkey.HotkeyFun.MenuLauncher           , new List<int>(){(int)Keys.Alt, (int)Keys.M}),
            new Hotkey((int)Hotkey.HotkeyFun.MainMenu               , new List<int>(){(int)Keys.Alt, (int)Keys.D}),
            new Hotkey((int)Hotkey.HotkeyFun.EAMRU                  , new List<int>(){(int)Keys.Alt, (int)Keys.R}),
            new Hotkey((int)Hotkey.HotkeyFun.Toggle2InputSource     , new List<int>(){(int)Keys.Alt, (int)Keys.I}),
            new Hotkey((int)Hotkey.HotkeyFun.ToggleInputSource      , new List<int>(){(int)Keys.Alt, (int)Keys.C}),
#if FEAT_NETWORKKVM
            //new Hotkey((int)Hotkey.HotkeyFun.NetworkKVMToggleInputSource, new List<int>(){(int)Keys.Alt, (int)Keys.N}),
#endif
            */

            //            new Hotkey((int)Hotkey.HotkeyFun.Input1, new List<int>(){-1,/*(int)Keys.Alt, (int)Keys.D1*/}), //deprecated
            //            new Hotkey((int)Hotkey.HotkeyFun.Input2, new List<int>(){-1,/*(int)Keys.Alt, (int)Keys.D2*/}), //deprecated
            //            new Hotkey((int)Hotkey.HotkeyFun.Input3, new List<int>(){-1,/*(int)Keys.Alt, (int)Keys.D3*/}), //deprecated
            //            new Hotkey((int)Hotkey.HotkeyFun.Input4, new List<int>(){-1,/*(int)Keys.Alt, (int)Keys.D4*/}), //deprecated
            #endregion
        };

        [JsonConstructor]
        public DDMUserSettings()
        {
            Version = 1.7;
            OS = "Windows";
            SnapEnable = false;
            DisplayMatrixEnable = false;
            ColorSchemes = (int)Themes.Dark;
            EAWithoutGap = true;
            EAWithShiftKey = false;
            EASpan = false;
            Language = (int)Languages.preferred;
            AutoStart = true;
            OnScreenNotification = true;
            AutoCheckUpdate = true;
            AllowTelemetry = false;
            TelemetryInit = true;
            ShowTelemetryUI = true;
            SilentShowTelemetry = false;
            ImportPermission = true;
            Hotkeys = new List<Hotkey>();
            CustLayouts = new List<CustLayout>();
            OTAPrompt = new Dictionary<string, string>();
            OTANextCheckTime = null;
            AutoRestoreWindowLayout = false;
            LockRotate = false;
            Profiles = new List<EAProfile>();
            SilentShowTelemetry = false;
            ScheduleBriConIsSync = false;
            LastImportedMonitor = null;
            NetworkDataAccess = DDMUserSettings.NetworkDataAccessState.Unknow;
        }

        public static bool restoreDDMUserSettings(ref DDMUserSettings userSettings, string filePath)
        {
            byte[] content;
            try
            {
                content = File.ReadAllBytes(filePath);
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
            userSettings = JsonConvert.DeserializeObject<DDMUserSettings>(JsonConetent);
            return true;
        }

    }
}

