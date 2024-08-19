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
    public enum Gaming_GameEnhancementMode
    {
        Off = 0x00,
        FrameRate = 0x01,
        DisplayAlignment = 0x02,
        Timer_30min = 0x03,
        Timer_40min = 0x04,
        Timer_50min = 0x05,
        Timer_60min = 0x06,
        Timer_90min = 0x07,
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
        MovieHDR = 0x02,
        GameHDR = 0x03,
        DisplayHDR = 0x04,
        CustomColorHDR = 0x05,
        HDRPeak1000 = 0x06,
        Disable = 0x0E
    }
    /// <summary>
    /// Gaming螢幕所有屬性(解析度、HDR等)
    /// </summary>
    public class GamingDisplayPropertiesInfo
    {
        public string DisplayName;

        /// <summary>
        /// 是否支援HDR
        /// </summary>
        public bool SupportedHDR;

        /// <summary>
        /// 螢幕HDR Type
        /// </summary>
        public Gaming_HDRType HDRType;

        /// <summary>
        /// 螢幕可支援的解析度刷新率、方向列表(含現在值、建議值)
        /// </summary>
        public DisplaySupportedProperties SupportedProperties;

        public GamingDisplayPropertiesInfo()
        {
            CurrentOrientation = DisplayOrientation.Unknow;
            USBCPrioritizationType = USBCPrioritizationType.Unknow;
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
        /// 螢幕可支援的解析度刷新率、方向列表(含現在值、建議值)
        /// </summary>
        public DisplaySupportedProperties SupportedProperties;

        public DisplayPropertiesInfo()
        {
            SupportedProperties = new DisplaySupportedProperties();
            CurrentOrientation = DisplayOrientation.Unknow;
            USBCPrioritizationType = USBCPrioritizationType.Unknow;
        }
    }

    /// <summary>
    /// 螢幕可支援的解析刷新率、方向
    /// </summary>
    public class DisplaySupportedProperties
    {
        public List<Properties> Properties { get; set; }
        public DisplayOrientation[] Orientations { get; set; }

        public DisplaySupportedProperties()
        {
            Properties = new List<Properties>();
            Orientations = new DisplayOrientation[4];
        }
    }

    /// <summary>
    /// 顯示器資訊
    /// </summary>
    public class Properties
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
}