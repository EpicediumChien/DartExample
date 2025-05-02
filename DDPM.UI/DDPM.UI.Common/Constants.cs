#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using DDPM.SA.Common.Settings;
using DDPM.UI.Common.Interfaces;

namespace DDPM.UI.Common;

/// <summary>
/// Constants class
/// </summary>
public class Constants
{
    #region Plugin IDs
    /// <summary>
    /// DDPM.UI.Plugin.DisplayPlugin PluginId
    /// </summary>
    public const string DdpmHomePluginId = "{B3E16E00-9666-4D1E-B924-397058F1AD3B}";

    /// <summary>
    /// DDPM.UI.DisplayPlugin PluginId
    /// </summary>
    public const string DisplayPluginId = "{638743AA-D515-43ED-8FDB-E0025AAC6A95}";

    /// <summary>
    /// DDPM.UI.Plugin.KeyboardPlugin PluginId
    /// </summary>
    public const string KeyboardPluginId = "{0cf83d41-5db7-4bbc-906c-dc600db40382}";

    /// <summary>
    /// DDPM.UI.Plugin.MousePlugin PluginId
    /// </summary>
    public const string MousePluginId = "{07123933-b552-40d6-b477-6d233391d604}";

    /// <summary>
    /// DDPM.UI.Plugin.PenPlugin PluginId
    /// </summary>
    public const string PenPluginId = "{932d5cee-3ca0-41e5-86ec-7b715baa4217}";

    /// <summary>
    /// DDPM.UI.Plugin.PenPlugin PluginId
    /// </summary>
    public const string HeadsetPluginId = "{ce791669-1a9d-454b-aa36-18b32bef6c21}";

    /// <summary>
    /// DDPM.UI.Plugin.PenPlugin PluginId
    /// </summary>
    public const string AirAudioPluginId = "{9d619c49-25e7-8035-6bfc-d756738ee038}";

    /// <summary>
    /// DDPM.UI.Plugin.AddDevicePlugin PluginId
    /// </summary>
    public const string AddDevicePluginId = "{53efd083-c124-4269-af20-1bd6b90e4375}";

    /// <summary>
    /// DDPM.UI.Plugin.WholeWindowPlugin PluginId
    /// </summary>
    public const string WholeWindowPluginId = "{2205CBDD-2D5C-4569-904B-FCF786C0B62D}";

    // <summary>
    /// DDPM.UI.Plugin.SettingsPlugin PluginId
    /// </summary>
    public const string SettingsPluginId = "{20A85B11-F15A-45AD-A0CA-CDE6DBE679B9}";

    /// <summary>
    /// DDPM.UI.Plugin.WebCameraPlugin PluginId
    /// </summary>
    public const string WebCameraPluginId = "{51821aad-be43-4829-bce6-8d830f1ef513}";

    /// <summary>
    /// DDPM.UI.Plugin.DockPlugin PluginId
    /// </summary>
    public const string DockPluginId = "{7E6A0BF1-3DE0-4CC9-BC64-574EB7C8A0FA}";

    /// <summary>
    /// DDPM.UI.Plugin.SoundBarPlugin PluginId
    /// </summary>
    public const string SoundBarPluginId = "{87AA6D04-47D6-442A-BDA1-2660CA216CBD}";

    /// <summary>
    /// DDPM.UI.Plugin.WalkThroughPlugin PluginId
    /// </summary>
    public const string WalkThroughPluginId = "{CB7DD7CC-72C4-4D70-9C6B-AFF7758E5A29}";
    /// <summary>
    /// DDPM.UI.Plugin.Bootloader PluginId
    /// </summary>
    public const string BootloaderPluginId = "{7EA1CB12-F945-4CAB-915B-BD4C3D25259E}";

    /// <summary>
    /// DDPM.UI.Plugin.RthHub PluginId
    /// </summary>
    public const string RtkHubPluginId = "{096B2E88-D78E-4FBF-BA37-2F79961B82CC}";

    /// <summary>
    /// DDPM.UI.Plugin.ExitAppPlugin PluginId
    /// </summary>
    public const string ExitAppPluginId = "{10AFA730-380B-4F3F-8427-E96392FC2796}";
    #endregion Plugin IDs

    // (Move to DDPM.SA.Common/Display/EAEMConstants.cs)
    //#region EasyArrange Constants
    //public const int MaxCustomItems = 5; //DDPMW-843
    //public const int MaxCustomNameLenth = 30; //DDPMW-843, should be implemented in DDPM.SA
    //#endregion

    #region ModuleGroupNames
    //Used to find a specific ModuleGroup and the associate VBarItem

    public const string GroupName_DisplaySettings = "DisplaySettings";
    public const string GroupName_InputSource = "InputSource";
    public const string GroupName_EasyArrange = "EasyArrange";
    public const string GroupName_Gaming = "Gaming";
    public const string GroupName_MonitorAudio = "Audio";
    public const string GroupName_KVM = "KVM";
    public const string GroupName_DisplayOthers = "Others";
    public const string GroupName_DisplayWebcam = "Webcam";

    #endregion ModuleGroupNames
    //Robert_Lin, 2024-11-15 added to standardlize GroupNames/ModuleNames
    //
    //The ModuleName propety defined in a IDdpmModule
    //For example:
    //   public class BrightnessModule : IDdpmModule
    //   {
    //        public string ModuleName { get => "BrightnessModule"; }
    //
    #region ModuleNames - DisplayPlugin
    //GroupName_DisplaySettings, "DisplaySettings"
    public const string ModuleName_Brightness = "BrightnessModule";
    public const string ModuleName_Color = "ColorModule";
    public const string ModuleName_DisplayProperties = "DisplayPropertiesModule";
    //GroupName_InputSource = "InputSource"
    public const string ModuleName_InputSource = "InputSourceModule";
    public const string ModuleName_PipPbp = "PipPbpModule";
    public const string ModuleName_DisplayHotkeys = "DisplayHotkeysModule";
    //GroupName_EasyArrange = "EasyArrange";
    public const string ModuleName_EzArrange = "EzArrangeModule";
    public const string ModuleName_EzMemory = "EzMemoryModule";
    public const string ModuleName_EzSettings = "EzSettingsModule";
    //GroupName_Gaming = "Gaming";
    public const string ModuleName_Gaming = "GamingModule";
    public const string ModuleName_VisionEngine = "VisionEngineModule";
    //GroupName_KVM = "KVM";
    public const string ModuleName_KVM = "KvmModule";
    //GroupName_DisplayOthers = "Others"
    public const string ModuleName_DisplayOthers = "DisplayOthersModule";
    //MonitorAudio = "MonitorAudio"
    public const string ModuleName_MonitorAudio = "MonitorAudioModule";

    //GroupName Webcam
    public const string ModuleName_DisplayWebcam = "DisplayWebcamModule";
    #endregion  ModuleNames - DisplayPlugin

    #region General Constants
    public const string DTH_ServiceName = "DellTechHub";
    #endregion General Constants
}