using System.Collections.Generic;

namespace DDPM.SA.Common
{
    public enum DisplayOrientation
    {
        Unknow = -1,
        Angle0 = 0,
        Angle90 = 1,
        Angle180 = 2,
        Angle270 = 3,
    }

    public enum USBCPrioritizationType
    {
        Unknow = -1,
        HighDataSpeed = 0,
        HighResolution = 1
    }

    public enum Gaming_Supported
    {
        GameEnhancementMode = 0x10,
        ResponseTime = 0x20,
        DarkStabilizer = 0x30,
        HDRType = 0x40
    }

    public enum Gaming_GameEnhancementMode
    {
        Off = 0x00,
        Frame_Rate = 0x01,
        Display_Alignment = 0x02,
        Timer__30min = 0x03,
        Timer__40min = 0x04,
        Timer__50min = 0x05,
        Timer__60min = 0x06,
        Timer__90min = 0x07,
        Disable = 0x0E
    }

    public enum Gaming_ResponseTime
    {
        Extreme = 0x00,
        Super_Fast = 0x01,
        Fast = 0x02,
        Normal = 0x03,
        Disable = 0x0E
    }

    public enum Gaming_DarkStabilizer
    {
        Level_0 = 0x00,
        Level_1 = 0x01,
        Level_2 = 0x02,
        Level_3 = 0x03,
        Disable = 0x0E
    }

    public enum Gaming_HDRType
    {
        Off = 0x00,
        Desktop = 0x01,
        Movie_HDR = 0x02,
        Game_HDR = 0x03,
        Display_HDR = 0x04,
        Custom_Color_HDR = 0x05,
        HDRPeak1000 = 0x06,
        Disable = 0x0E
    }

    public enum Gaming_DualResolutionType : uint
    {
        _4K = 0xF811,
        _FHD = 0xF810,
        Unknow = 0x0000
    }

    public enum Gaming_VisionEngineType : uint
    {
        off = 0x00,
        Night_Vision = 0x01,
        Clear_Vision = 0x02,
        Bino_Vision = 0x03,
        Chroma_Vision = 0x04,
        Steady_Vision = 0x05,
        Crosshair = 0x06
    }

    /// <summary>
    /// Gaming螢幕屬性(解析度、HDR等)
    /// </summary>
    public class GamingDisplayPropertiesInfo
    {
        public string DisplayName;
        public bool IsSupported_GameEnhancementMode;
        public bool IsSupported_ResponseTime;
        public bool IsSupported_DarkStabilizer;
        public bool IsSupported_HDRType;
        public bool IsSupported_DualResolutionType;
        public bool IsSupported_VisionEngineType;
        public Gaming_GameEnhancementMode? Current_GameEnhancementMode;
        public Gaming_ResponseTime? Current_ResponseTime;
        public Gaming_DarkStabilizer? Current_DarkStabilizer;
        public Gaming_HDRType? Current_HDRType;
        public Gaming_DualResolutionType? Current_DualResolutionType;
        public Gaming_VisionEngineType Current_VisionEngineType;
        public bool[] IsEnable_VisionEngineType;
        public List<Gaming_GameEnhancementMode> Supported_GameEnhancementMode;
        public List<Gaming_ResponseTime> Supported_ResponseTime;
        public List<Gaming_DarkStabilizer> Supported_DarkStabilizer;
        public List<Gaming_HDRType> Supported_HDRType;
        public List<Gaming_DualResolutionType> Supported_DualResolutionType;
        public List<Gaming_VisionEngineType> Supported_VisionEngineType;
        public DisplaySupportedProperties SupportedProperties;

        public GamingDisplayPropertiesInfo()
        {
            Current_GameEnhancementMode = null;
            Current_ResponseTime = null;
            Current_DarkStabilizer = null;
            Current_HDRType = null;
            Current_DualResolutionType = null;
            IsEnable_VisionEngineType = new bool[6];
            Supported_GameEnhancementMode = new List<Gaming_GameEnhancementMode>();
            Supported_ResponseTime = new List<Gaming_ResponseTime>();
            Supported_DarkStabilizer = new List<Gaming_DarkStabilizer>();
            Supported_HDRType = new List<Gaming_HDRType>();
            Supported_DualResolutionType = new List<Gaming_DualResolutionType>();
            Supported_VisionEngineType = new List<Gaming_VisionEngineType>();
            SupportedProperties = new DisplaySupportedProperties();
        }
    }

    /// <summary>
    /// 螢幕所有屬性(解析度、HDR等)
    /// </summary>
    public class DisplayPropertiesInfo
    {
        public string DisplayName;

        /// <summary>
        /// 是否支援HDR
        /// </summary>
        public bool SupportedHDR;

        /// <summary>
        /// HDR狀態
        /// </summary>
        public bool isHDREnable;

        /// <summary>
        /// 是否支援USB-C Prioritization
        /// </summary>
        public bool SupportedUSBCPrioritization;

        /// <summary>
        /// USB-C Prioritization狀態
        /// </summary>
        public USBCPrioritizationType USBCPrioritizationType;

        /// <summary>
        /// 現在螢幕畫面方向
        /// </summary>
        public DisplayOrientation CurrentOrientation;
        /// <summary>
        /// 螢幕是否支援OSD方向,True支援寫入,False僅支援讀取,null為沒有AA
        /// </summary>
        public bool? Supported_OSD_Orientation;
        /// <summary>
        /// 現在OSD菜單方向
        /// </summary>
        public DisplayOrientation Current_OSD_Orientation;

        /// <summary>
        /// 螢幕可支援的解析度刷新率、方向列表(含現在值、建議值)
        /// </summary>
        public DisplaySupportedProperties SupportedProperties;

        public DisplayPropertiesInfo()
        {
            SupportedProperties = new DisplaySupportedProperties();
            CurrentOrientation = DisplayOrientation.Unknow;
            USBCPrioritizationType = USBCPrioritizationType.Unknow;
            Current_OSD_Orientation = DisplayOrientation.Unknow;
        }
    }

    /// <summary>
    /// 螢幕可支援的解析刷新率、方向
    /// </summary>
    public class DisplaySupportedProperties
    {
        public List<Properties> Properties { get; set; }
        public DisplayOrientation[] Orientations { get; set; }
        public DisplayOrientation[] OSD_Orientations { get; set; }

        public DisplaySupportedProperties()
        {
            Properties = new List<Properties>();
            Orientations = new DisplayOrientation[4];
            OSD_Orientations = new DisplayOrientation[4];
        }
    }

    /// <summary>
    /// 顯示器資訊
    /// </summary>
    public sealed class Properties
    {
        /// <summary>
        /// 解析度:寬
        /// </summary>
        public int Resolutions_Width;

        /// <summary>
        /// 解析度:高
        /// </summary>
        public int Resolutions_High;

        /// <summary>
        /// 刷新率
        /// </summary>
        public int Frequency;

        /// <summary>
        /// 位元深度
        /// </summary>
        public int BitsPerPixel;

        /// <summary>
        /// 是否為建議值
        /// </summary>
        public bool isRecommended;

        /// <summary>
        /// 是否為現在值
        /// </summary>
        public bool isCurrent;

        public Properties()
        {
            Resolutions_Width = 0;
            Resolutions_High = 0;
            Frequency = 0;
            BitsPerPixel = 0;
            isRecommended = false;
            isCurrent = false;
        }

        public bool Equals(Properties properties)
        {
            return this.Resolutions_Width == properties.Resolutions_Width &&
                this.Resolutions_High == properties.Resolutions_High &&
                this.Frequency == properties.Frequency;
        }
    }

    /// <summary>
    /// 現在顯示器的參數
    /// </summary>
    public class DisplayCurrentPropertiesInfo
    {
        public string DisplayName;

        /// <summary>
        /// HDR狀態
        /// </summary>
        public bool isHDREnable;

        /// <summary>
        /// 現在螢幕畫面方向
        /// </summary>
        public DisplayOrientation CurrentOrientation;

        /// <summary>
        /// 螢幕可支援的解析度刷新率、方向列表(含現在值、建議值)
        /// </summary>
        public Properties CurrentProperties;

        /// <summary>
        /// USB-C Prioritization狀態
        /// </summary>
        public USBCPrioritizationType USBCPrioritizationType;

        public DisplayCurrentPropertiesInfo()
        {
            CurrentOrientation = DisplayOrientation.Unknow;
            CurrentProperties = new Properties();
            USBCPrioritizationType = USBCPrioritizationType.Unknow;
        }
    }
}