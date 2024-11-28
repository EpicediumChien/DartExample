#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.Common
{
    /// <summary>
    /// Constants class
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Defines how long the splash screen fade should take
        /// </summary>
        /// <remarks>WPF defaults this to 300ms</remarks>
        public static readonly TimeSpan SplashScreenFade = new(0, 0, 0, 0, 0);

        /// <summary>
        /// AboutView PluginId
        /// </summary>
        public const string AboutViewPluginId = "{0368DF6B-3C30-49F7-B601-001044C3218D}";

        /// <summary>
        /// Preferences page PluginId
        /// </summary>
        public const string PreferencesPagePluginId = "{858910c4-72ca-4dec-8d73-bee7bd2b3346}";

        /// <summary>
        /// Privacy Notice Page PluginId
        /// </summary>
        public const string TelemetryConsentPagePluginId = "{80B32010-684C-46AF-98B4-2AC5C1FED3C1}";

        /// <summary>
        /// Privacy Notice Interrupter PluginId
        /// </summary>
        public const string TelemetryConsentFirstLaunchPlugin = "{9C0454AA-6B66-49A8-9AEE-33E8869923A2}";

        /// <summary>
        /// Product Registration page PluginId
        /// </summary>
        public const string ProductRegistrationPagePluginId = "{223910c4-71ca-4dec-6d73-bee8bd1b2279}";

        /// <summary>
        /// In-App Notification PluginId
        /// </summary>
        public const string InAppNotificationPluginId = "{AC678543-B4E8-4445-887B-2332966E40AA}";

        /// <summary>
        /// User Guides page PluginId
        /// </summary>
        public const string UserGuidesPagePluginId = "{A49769CA-4FCC-46FD-A94C-1AA3B3DB2C97}";

        /// <summary>
        /// Console Preferences Helper PluginId
        /// </summary>
        public const string ConsolePreferencesHelperPluginId = "{8704fea1-f55c-4100-aea7-34ee21582397}";

        /// <summary>
        /// Welcome PluginId
        /// </summary>
        public const string WelcomePluginId = "{D3F8A593-FF52-48A7-BF48-F707E96E0AFD}";

        /// <summary>
        /// Homepage PluginId
        /// </summary>
        public const string HomePagePluginId = "{8fcd239d-d951-4061-9625-2144e2ee0f76}";

        /// <summary>
        /// Manager Notification PluginId
        /// </summary>
        public const string ManagerNotificationPluginId = "{796becec-fd5a-450e-87e7-fad20cf2ea12}";

        /// <summary>
        /// Manager Notification Protocol PluginId
        /// </summary>
        public const string ManagerNotificationProtocolPluginId = "{8BF24F81-609A-4F77-BB18-7891B997FB0F}";

        /// <summary>
        /// Systray StrategyId
        /// </summary>
        public const string SystrayStrategyId = "{676B8C99-F27E-4FD1-A899-320144412D9A}";

        /// <summary>
        /// Suite Name, 2024-5-16, Robert_Lin@wistron.com, change "MyDell" to "DDPM2.0"
        /// </summary>
        public const string SuiteName = "DDPM2.0";

        /// <summary>
        /// Product Name for ThickClient.
        /// </summary>
        public const string ThickClientProductName = "Console";

        /// <summary>
        /// Product Name for Manager.
        /// </summary>
        public const string ManagerProductName = "MyDell Notification Manager";

        /// <summary>
        /// Product Name for SysTray.
        /// </summary>
        public const string SysTrayProductName = "Systray";

        /// <summary>
        /// NotificationDetail StorageId
        /// </summary>
        public const string NotificationDetailStorageId = "{1CC94053-E25B-4467-B14C-48FFC0450C2B}";

        /// <summary>
        /// LatestResponseTypeStorageId StorageId
        /// </summary>
        public const string LatestResponseTypeStorageId = "{252A3D9D-69D7-4388-9061-C2BC95370C91}";

        /// <summary>
        /// UserAction StorageId
        /// </summary>
        public const string UserActionStorageId = "{E31CFC3F-715A-44F3-AEC0-4704A2C5DE08}";

        /// <summary>
        /// OperationResult StorageId
        /// </summary>
        public const string OperationResultStorageId = "{035EAA96-C886-4C8D-8781-FF55C64AFB58}";

        /// <summary>
        /// OperationHistory StorageId
        /// </summary>
        public const string OperationHistoryStorageId = "{CC05AE46-D0AF-48B9-BCC8-D32A65AB203A}";

        /// <summary>
        /// Preferences StorageId
        /// </summary>
        public const string PreferencesStorageId = "{DE5950A5-060D-44C1-9621-EF3A45174905}";

        /// <summary>
        /// Preferences PluginId
        /// </summary>
        public const string PreferencesPluginId = "{9DD6DE48-F652-4D1F-8B0D-964C5978B089}";

        /// <summary>
        /// Initialize Preference PluginId
        /// </summary>
        public const string InitializePreferencePluginId = "{7FE6DE58-F122-4B1F-1B0D-164C5977B060}";

        /// <summary>
        /// PreferencesHelper PluginId
        /// </summary>
        public const string PreferencesHelperPluginId = "{07F78E4B-4D3B-4999-938B-708852E1BE4C}";

        /// <summary>
        /// SecureStorage PluginId
        /// </summary>
        public const string SecureStoragePluginId = "{3FE599CB-CD16-44B3-B690-181A50F4582E}";

        /// <summary>
        /// TelemetryConsent PluginId
        /// </summary>
        public const string TelemetryConsentPluginId = "{3B0E026C-40B7-4914-9067-42BEDCE5609F}";

        /// <summary>
        /// MyDell SplashScreen 4k Resolution
        /// </summary>
        public const string SplashScreenResolution4K = "4k";

        /// <summary>
        /// MyDell square SplashScreen
        /// </summary>
        public const string SplashScreenSquareShape = "square";

        /// <summary>
        /// MyDell round corner SplashScreen
        /// </summary>
        public const string SplashScreenRoundShape = "round";

        /// <summary>
        /// Certificate Store Name.
        /// </summary>
        public const string DellTrust = "Dell Trust";

        /// <summary>
        /// DiscoveryNotification StorageId
        /// </summary>
        public const string DiscoveryNotificationDetailsStorageId = "{F45EFC3F-715A-41F3-AEC0-4702A2C6DE99}";

        /// <summary>
        /// Discovery Notification PluginId.
        /// </summary>
        public const string DiscoveryNotificationPluginId = "{896becec-fd5a-450e-87e7-fad20cf2ea34}";

        /// <summary>
        /// MyDell Application Name.
        /// </summary>
        public const string ApplicationName = "DDPM Application";

        /// <summary>
        /// Notification Preference Guid
        /// </summary>
        public const string NotificationPreferenceId = "{BBCD730C-10AC-4B40-8365-72AC9BE5155D}";

        /// <summary>
        /// Telemetry Preference Guid
        /// </summary>
        public const string TelemetryPreferenceId = "{E458696A-82E3-4904-9391-784483F0B17E}";

        /// <summary>
        /// Telemetry Consent Plugin (Unified Consent) Preference Guid
        /// </summary>
        public const string TelemetryConsentPreferenceId = "{82C4B570-CA3C-4657-A19B-5FB63DED76E7}";

        /// <summary>
        /// Enable Restore Event Guid
        /// </summary>
        public const string EnableRestoreEvent = "{ea95fe1c-9672-4d27-bb9c-ae946f1cf2cb}";

        /// <summary>
        /// Restore Clicked Event Guid
        /// </summary>
        public const string RestoreClickedEvent = "{03d28b97-7e26-40ee-b953-17667b792095}";

        /// <summary>
        /// FirstTime HomePage ActivatedId
        /// </summary>
        public const string FirstTimeHomePageActivated = "{BFBFE698-1C39-49B3-BB59-470F18723BEF}";

        /// <summary>
        ///  Expander Preference Guid
        /// </summary>
        public const string ExpanderPreferenceId = "{D4C1A31B-D255-4D82-8A2A-4AD0AC2C75F5}";

        /// <summary>
        /// Defines the Starting Plugin argument name
        /// </summary>
        public const string StartingPlugin = "StartingPlugin";

        /// <summary>
        /// Defines the Starting Plugin Parameter name
        /// </summary>
        public const string StartingPluginParameter = "StartingPluginParameter";

        /// <summary>
        /// Defines the Starting Plugin Priority name
        /// </summary>
        public const string StartingPluginPriority = "StartingPluginPriority";

        /// <summary>
        /// Defines the Starting Plugin Parameter Model version
        /// </summary>
        public const int StartingPluginParameterModelVersion1 = 1;

        /// <summary>
        /// SystemOverview Plugin Id
        /// </summary>
        public const string SystemOverviewPluginId = "{ec21749c-7318-4909-a56f-3d50dbee6f35}";

        /// <summary>
        ///  Tile Plugin Id
        /// </summary>
        public const string TilePluginId = "{6438e451-5b3f-489e-ad00-4cf566e85c6a}";

        /// <summary>
        /// Suggestion Plugin Id
        /// </summary>
        public const string SuggestionPluginId = "{b7d818af-ae65-4720-bb0e-d32ccfb88046}";

        /// <summary>
        /// HomePageLayout Plugin Id
        /// </summary>
        public const string HomePageLayoutPluginId = "{26FEFA64-A3E6-456B-9B5B-CBE248703C29}";

        /// <summary>
        /// UniqueID for the Systray
        /// </summary>
        public const string SysTrayUniqueGuid = "{827D5FED-4299-4FF0-859B-FB624A41180E}";

        /// <summary>
        /// UniqueID for the DO Systray
        /// </summary>
        public const string DOSysTrayUniqueGuid = "822722EF-5AE3-4845-A428-6E83EC6D23D3";

        /// <summary>
        /// UniqueId for the thick client
        /// </summary>
        public const string ThickClientUniqueGuid = "{856AEE4D-705F-4913-A0C6-7A4FD23715DD}";//"{706b7610-0f15-4ca2-8373-dadd04e25bb4}"

        /// <summary>
        /// Defines the registry Path for Notification Manager
        /// </summary>
        public const string ManagerRegistryPath = "SOFTWARE\\Dell\\Notification Manager";

        /// <summary>
        /// Defines the ProtocolUri Path for registered products
        /// </summary>
        public const string ProtocolUris = "ProtocolURIs";

        /// <summary>
        /// Defines the Protocol Version name
        /// </summary>
        public const string ProtocolVersion = "ProtocolVersion";

        /// <summary>
        /// Defines the URI name
        /// </summary>
        public const string Uri = "URI";

        /// <summary>
        /// defines the Product Names
        /// </summary>
        public const string ProductNames = "ProductNames";

        /// <summary>
        /// Defines the Systray AgentId
        /// </summary>
        public const string SystrayAgentId = "SystrayAgentId";

        /// <summary>
        ///  Telemetry Registry Key
        /// </summary>
        public static string TelemetryRegKey = @"SOFTWARE\\Dell\\DCFShared\\Telemetry";

        /// <summary>
        ///  ConsentConfirmed Registry Value
        /// </summary>
        public static string ConsentConfirmedRegValue = "ConsentConfirmed";
    }
}