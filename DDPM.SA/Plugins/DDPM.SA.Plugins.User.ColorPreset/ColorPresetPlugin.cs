#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// ColorPresetPlugin.cs created on 28/03/2024T09:50 AM
//

#endregion

using DDPM.SA.Common;
using DDPM.ShowOSD;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VcpCore.Common;
using WinCopies.Util;

namespace ColorPreset.Plugins
{
    [Plugin(DDPM.SA.Common.IDs.DDPM_COLOR_PRESET_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IColorPresetSA) })]
    [PluginRequires(Id = DDPM.SA.Common.IDs.Device_Manager_Plugin_ID, Version = "1.0.0", AllowDynamicResolving = true)]
    public class ColorPresetPlugin : BaseAgentPlugin, IColorPresetSA, IDisposableObservable
    {
        #region Private Members

        private const string pluginName = "ColorPresetPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements ColorPreset Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements ColorPreset Plugin.";

        private IAgent _agent;
        public const string PluginLogId = "ColorPreset";

        private Dictionary<string, InstalledAppInfo> _AllAppData = new Dictionary<string, InstalledAppInfo>();
        private List<string> _supported_preset = new List<string>();

        private List<string> ColorPresetSupportList = new List<string>();

        //20240802 jim add
        public List<string> Support_ICC_DeviceName { get; set; } = new List<string> { "U4021QW", "U2723QE", "U3223QE", "U3223QZ", "U3423WE", "U3824DW", "U4924DW", "U3224KB", "U2724D", "U2724DE", "U3425WE", "U4025QW", "UP2720Q", "UP3221Q" };

        private List<X509Certificate2> TrustedPublisher = new List<X509Certificate2>();
        private List<X509Certificate2> TrustedRoot = new List<X509Certificate2>();

        //private readonly string[] Issuer = { "CN=Entrust Certification Authority - L1F, O=\"Entrust, C=US", "CN=localhost, O=DigiNow, C=US" };
        private readonly string[] Issuer = { "Entrust Certification Authority - L1F, OU=\"(c) 2016 Entrust, Inc. - for authorized use only\", OU=See www.entrust.net/legal-terms, O=\"Entrust, Inc.\", C=US" };

        private readonly string[] Subject = { "CN=content-cdn.dell.com, O=Dell, L=Round Rock, S=Texas, C=US" };

        private string[] Issuers;
        private string[] Subjects;

        private enum log_type
        {
            info = 0,
            error
        }

        public event EventHandler<VCPchangedEventArgs> VCPchanged;

        private ShowOSDWin OsdWin = null;
        private string iconFolderPath = string.Empty;

        #endregion

        #region Constructor

        public ColorPresetPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            writelog("ColorPresetPlugin constructor ...");

            LoadInstalledAppList(true);
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            PluginCondition = new PluginStartedCondition();

            writelog("ColorPreset plugin started");
        }

        #endregion

        #region IColorPresetSA implementation

        public void SetAppIconFolder(string folder_path)
        {
            if (string.IsNullOrEmpty(folder_path))
                return;

            iconFolderPath = folder_path;
        }

        public Task<Dictionary<string, InstalledAppInfo>> GetInstalledAppsList(bool isReload = false)
        {
            LoadInstalledAppList(isReload);
            return Task.FromResult(_AllAppData);
        }

        public int get_index_of_json_config_for_cur_monitor(MonitorInfo mo)
        {
            int index = -1;

            if (Test_AddAppCollectionData.GetInstance()._monitorConfigs != null)
            {
                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count > 0)
                {
                    index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                                                    x.DeviceInfo.ModelName.Trim() == mo.edid.ModelName.Trim() &&
                                                    x.DeviceInfo.SerialNumber.Trim() == mo.edid.SerialNumber.Trim());
                }
            }
            return index;
        }

        public ColorPresetSettings get_cur_monitor_preset_config(MonitorInfo mo, List<ColorPresetSettings> config)
        {
            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(mo);

            if (index < 0)
            {
                //data not exist, create new to config for current monitor
                Test_AddAppCollectionData.GetInstance()._monitorConfigs.Add(new ColorPresetSettings()
                {
                    DeviceInfo = mo.edid,
                    RunType = (int)ColorPresetRunType.Auto,
                    AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>(),
                    PresetForManual = "Standard"
                });

                index = get_index_of_json_config_for_cur_monitor(mo);

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add("Desktop Application", new ColorPresetSettings_AppInfo()
                {
                    ColorPresetName = "Standard",
                    IconName = "Assets/palette.png",
                });

                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add("UWP Application", new ColorPresetSettings_AppInfo()
                {
                    ColorPresetName = "Standard",
                    IconName = "Assets/palette.png",
                });
            }
            return Test_AddAppCollectionData.GetInstance()._monitorConfigs[index];
        }

        public Task<List<ColorPresetSettings>> AddColorPresetForMonitorConfig(MonitorInfo mo, string AppName, string ColorPreset_Name, string supported_preset, List<ColorPresetSettings> config)
        {
            ColorPresetSettings temp = get_cur_monitor_preset_config(mo, config);

            if (temp.AppInfo.Count <= 0)
            {
                //Default items
                temp.AppInfo.Add("Desktop Application", new ColorPresetSettings_AppInfo()
                {
                    ColorPresetName = "Standard",
                    IconName = "Assets/palette.png",
                });
                temp.AppInfo.Add("UWP Application", new ColorPresetSettings_AppInfo()
                {
                    ColorPresetName = "Standard",
                    IconName = "Assets/palette.png",
                });
            }

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(mo);

            if (index < 0)
            {
                //data not exist, create new to config for current monitor
                Test_AddAppCollectionData.GetInstance()._monitorConfigs.Add(new ColorPresetSettings()
                {
                    DeviceInfo = mo.edid,
                    RunType = (int)ColorPresetRunType.Auto,
                    AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>(),
                    PresetForManual = "Standard"
                });

                index = get_index_of_json_config_for_cur_monitor(mo);
            }

            _supported_preset = ReadColorPreset(mo, supported_preset).Result;

            int index_AppPresetIdx = 0;

            if (_supported_preset.Count > 0)
            {
                index_AppPresetIdx = _supported_preset.FindIndex(x => x.StartsWith(ColorPreset_Name));

                if (index_AppPresetIdx <= 0)
                    index_AppPresetIdx = 0;
            }

            AppData be = Test_AddAppCollectionData.GetInstance().AppsList.FirstOrDefault(x => x.AppName == (AppName));

            KeyValuePair<string, InstalledAppInfo> kvp = _AllAppData.First(x => x.Value.AppName == (AppName));

            if (be != null)
            { }
            else
            {
                Test_AddAppCollectionData.GetInstance().AppsList.Add(new AppData
                {
                    AppIcon = kvp.Value.IconName,
                    AppName = AppName,
                    AppPresetIdx = index_AppPresetIdx,
                    SupportPreset = new List<string>(_supported_preset),
                });
            }

            if (!(Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.ContainsKey(AppName)))
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add(AppName, new ColorPresetSettings_AppInfo()
                {
                    ColorPresetName = ColorPreset_Name,
                    IconName = kvp.Value.IconName,
                });
            }

            return Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        public Task<List<ColorPresetSettings>> ChangeColorPresetForMonitorConfig(MonitorInfo mo, string AppName, string ColorPreset_Name, List<ColorPresetSettings> config)
        {
            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index_config = get_index_of_json_config_for_cur_monitor(mo);
            if (index_config >= 0)
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].RunType = (int)ColorPresetRunType.Auto;
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].AppInfo[AppName].ColorPresetName = ColorPreset_Name;
            }

            return Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        public Task<List<ColorPresetSettings>> DeleteColorPresetForMonitorConfig(MonitorInfo mo, string AppName, List<ColorPresetSettings> config)
        {
            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index_config = get_index_of_json_config_for_cur_monitor(mo);

            if (index_config >= 0)
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].RunType = (int)ColorPresetRunType.Auto;
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].AppInfo.Remove(AppName);
            }

            return Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        public Task<List<ColorPresetSettings>> AutoSetColorPresetForMonitorConfig(MonitorInfo mo, string on_off, List<ColorPresetSettings> config)
        {
            if (on_off.Equals("on", StringComparison.CurrentCultureIgnoreCase))
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

                int index = get_index_of_json_config_for_cur_monitor(mo);

                if (index >= 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Auto;
                }
            }
            else if (on_off.Equals("off", StringComparison.CurrentCultureIgnoreCase))
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

                int index = get_index_of_json_config_for_cur_monitor(mo);

                if (index >= 0)
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Manual;
                }
            }

            return Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        public void ShowOSD_ColoPreset(MonitorInfo m, string strMsg, bool is_ShowUI = true, bool is_AUTO = false)
        {
            Log.Info($"ShowOSD_ColoPreset requested ...");
            writelog("ColorPresetPlugin ShowOSD_ColoPreset requested ...");

            Thread thread = new Thread(() =>
            {
                if (m != null)
                {
                    var v = (MonitorInfo)m;

                    System.Windows.Forms.Screen s = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => x.DeviceName == v.DisplayName);

                    if (s != null)
                    {
                        if (OsdWin != null)
                        {
                            OsdWin.Close();
                        }

                        OsdWin = new ShowOSDWin(strMsg, 40, v.edid);

                        var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                        var varX = (int)dpiXProperty.GetValue(null, null);
                        double dpiX = (double)varX / (double)96;
                        try
                        {
                            OsdWin.Top = s.WorkingArea.Top / (double)dpiX;
                        }
                        catch (Exception)
                        {
                            OsdWin.Top = s.WorkingArea.Top;
                        }

                        try
                        {
                            OsdWin.Left = s.WorkingArea.Left / (double)dpiX;
                        }
                        catch (Exception)
                        {
                            OsdWin.Left = s.WorkingArea.Left;
                        }

                        double dbfactor = ((double)((double)40 / (double)96));

                        double dbscale = dbfactor * dpiX;

                        OsdWin.Height = (s.WorkingArea.Height * dbscale) / ((40 * dbscale));
                        OsdWin.Width = (s.WorkingArea.Width) / ((40 * dbfactor));

                        if (is_ShowUI)
                            OsdWin.Show();
                        OsdWin = null;
                    }
                }
                // 啟動消息循環
                System.Windows.Threading.Dispatcher.Run();
            });

            // 設定為單線程單元（STA），WPF需要STA模式
            thread.SetApartmentState(ApartmentState.STA);

            // 啟動執行緒
            thread.Start();
        }

        public Task<List<string>> ReadColorPreset(MonitorInfo m, string vcp_capbilities)
        {
            Log.Info($"ReadColorPreset requested ...");

            if (string.IsNullOrEmpty(vcp_capbilities))
            {
                ColorPresetSupportList.Clear();
                return Task.FromResult(ColorPresetSupportList);
            }

            // 20240619 jim add
            if (!string.IsNullOrEmpty(vcp_capbilities))
            {
                var Capabilities = (JObject)JsonConvert.DeserializeObject(vcp_capbilities);
                if (Capabilities.ContainsKey("CapsDataMap"))
                {
                    var CapsDataMap = (JObject)Capabilities["CapsDataMap"];
                    if (CapsDataMap.ContainsKey("ColorPreset"))
                    {
                        JArray colorrreset = (JArray)CapsDataMap["ColorPreset"];

                        ColorPresetSupportList.Clear();

                        foreach (var tmp in colorrreset)
                            ColorPresetSupportList.Add(new string(tmp.ToString()));
                    }
                }
            }

            int index = 0;

            Console.WriteLine("[" + m.AliasDeviceName + "] Color Preset SupportList : ");

            foreach (var h in ColorPresetSupportList)
            {
                Console.WriteLine("[" + index.ToString() + "] : " + h);
                index++;
            }

            return Task.FromResult(ColorPresetSupportList);
        }

        public Task<List<ColorPresetSettings>> WriteColorPreset(MonitorInfo m, string ColorPreset_Name, List<ColorPresetSettings> config)
        {
            Log.Info($"WriteColorPreset requested ...");

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(m);

            if (index >= 0)
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Manual;
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].PresetForManual = ColorPreset_Name;
            }

            return Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        public Task<List<ColorPresetSettings>> WriteColorPreset_AUTO(MonitorInfo m, string ColorPreset_Name, List<ColorPresetSettings> config)
        {
            Log.Info($"WriteColorPreset_AUTO requested ...");

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = config;

            int index = get_index_of_json_config_for_cur_monitor(m);

            if (index >= 0)
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].RunType = (int)ColorPresetRunType.Auto;
                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].PresetForManual = ColorPreset_Name;
            }

            return Task.FromResult(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
        }

        #endregion

        #region IDisposableObservable Support

        /// <summary>
        /// To detect redundant calls
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Override for Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion

        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private void writelog(string text, log_type log_type = log_type.info)
        {
            text = "[ColorPreset] " + text;
            Console.WriteLine(text);
            if (log_type == log_type.info)
                Log.Info(text);
            else
                Log.Error(text);
        }

        #region Event Handler

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;
        }

        private void LoadInstalledAppList(bool renew_data = false)
        {
            writelog("IColorPresetSA get LoadInstalledAppList request");
            if (_AllAppData == null || _AllAppData.Count == 0 || renew_data == true)
            {
                writelog("enter reflash procedure");
                AppsCollectShell appshell = new AppsCollectShell(iconFolderPath);
                _AllAppData = appshell.FindAppsbyShell();
            }
            writelog($"appshell.FindAppsbyShell get app count({_AllAppData.Count})");
        }

        // The cryptographic service provider.
        private SHA256 Sha256 = SHA256.Create();

        // Compute the file's hash.
        private byte[] GetHashSha256(string filename)
        {
            using (FileStream stream = System.IO.File.OpenRead(filename))
            {
                return Sha256.ComputeHash(stream);
            }
        }

        // 驗證伺服器證書
        private bool CheckCertificateIsVaild(X509Certificate2 certificate)
        {
            bool result = false;
            try
            {
                X509Chain x509Chain = new X509Chain();
                x509Chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                x509Chain.ChainPolicy.UrlRetrievalTimeout = new System.TimeSpan(0, 1, 0);
                x509Chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                result = x509Chain.Build(certificate);
            }
            catch (System.Exception ex)
            {
                //Console.WriteLine("[CheckCertificateIsVaild] error: " + ex.Message);
                writelog("[CheckCertificateIsVaild] error: " + ex.Message);
            }
            return result;
        }

        private bool CheckIssuerAndSubject(X509Certificate2 certificate)
        {
            try
            {
                Console.WriteLine($"Issuer:{certificate.Issuer.ToString()}");
                Console.WriteLine($"Subject:{certificate.Subject.ToString()}");
                writelog($"Issuer:{certificate.Issuer.ToString()}");
                writelog($"Subject:{certificate.Subject.ToString()}");

                foreach (string s in Issuers)
                {
                    if (!certificate.Issuer.Contains(s))
                    {
                        Console.WriteLine("[CheckIssuerAndSubject] Not match.");
                        writelog("[CheckIssuerAndSubject] Not match.");
                        return false;
                    }
                }

                foreach (string s in Subjects)
                {
                    if (!certificate.Subject.Contains(s))
                    {
                        Console.WriteLine("[CheckIssuerAndSubject] Not match.");
                        writelog("[CheckIssuerAndSubject] Not match.");
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                Console.WriteLine("[CheckIssuerAndSubject] Error.");
                writelog("[CheckIssuerAndSubject] Error.");
            }
            return false;
        }

        private IIC_Metadata RunDeserializeObject(string value)
        {
            IIC_Metadata retList = new IIC_Metadata();

            try
            {
                // 20240725 jim remove
                //Dictionary<string, List<ICC_SupportDeviceName>> _temp = new Dictionary<string, List<ICC_SupportDeviceName>>() { };

                retList._support_ICC_DeviceName = JsonConvert.DeserializeObject<Dictionary<string, List<ICC_SupportDeviceName>>>(value);

                //retList._supportDeviceName

                //retList = JsonConvert.DeserializeObject<IIC_Metadata>(value);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[DownloadICCData] RunDeserializeObject() error:" + ex.Message.ToString());
                writelog("[DownloadICCData] RunDeserializeObject() error:" + ex.Message.ToString());

                return retList;
            }

            return retList;
        }

        // Return a byte array as a sequence of hex values.
        public static string BytesToString(byte[] bytes)
        {
            string result = "";
            foreach (byte b in bytes) result += b.ToString("x2");
            return result;
        }

        public bool CheckCA(string URL)
        {
            bool flag = false;
            int num = 1;
            GetLocalTrustedCert();
            while (!flag && num > 0)
            {
                try
                {
                    HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(URL);
                    httpWebRequest.ServerCertificateValidationCallback = PinPublicKey;
                    httpWebRequest.GetResponse();
                    flag = true;
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("[CheckCA] error:" + ex.Message.ToString());
                    writelog("[CheckCA] error:" + ex.Message.ToString());
                    flag = false;
                    Console.WriteLine(string.Format("[CheckCA] error, retry:" + num));
                    writelog(string.Format("[CheckCA] error, retry:" + num));
                    Thread.Sleep(1000);
                }
                num--;
            }
            Console.WriteLine(string.Format("[CheckCA] res:" + flag));
            writelog(string.Format("[CheckCA] res:" + flag));
            if (!flag)
            {
                flag = CheckCAHTTP(URL);
            }
            if (!flag)
            {
                Console.WriteLine(string.Format("[CheckCA][CheckCAHTTP] Fail, Send Telemetry." + flag));
                writelog(string.Format("[CheckCA][CheckCAHTTP] Fail, Send Telemetry." + flag));
            }

            return flag;
        }

        private bool CheckCAHTTP(string URL)
        {
            try
            {
                if (CheckHTTPAvailable(URL))
                {
                    HttpClientHandler httpClientHandler = new HttpClientHandler();
                    httpClientHandler.ServerCertificateCustomValidationCallback = ValidateCertificate;
                    HttpClient client = new HttpClient(httpClientHandler);
                    bool response = GetResponse(client, URL);
                    Console.WriteLine("[CheckCAHTTP] result:" + response);
                    writelog("[CheckCAHTTP] result:" + response);
                    return response;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[CheckCAHTTP] error:" + ex.Message.ToString());
                writelog("[CheckCAHTTP] error:" + ex.Message.ToString());
            }
            return false;
        }

        private bool ValidateCertificate(HttpRequestMessage request, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
            {
                Console.WriteLine("[ValidateCertificate] certificate null.");
                writelog("[ValidateCertificate] certificate null.");
                return false;
            }

            if (request == null)
            {
                Console.WriteLine("[ValidateCertificate] request null.");
                writelog("[ValidateCertificate] request null.");
            }

            if (sslPolicyErrors != 0)
            {
                Console.WriteLine($"[ValidateCertificate] sslPolicyErrors is Error. {sslPolicyErrors}");
                writelog($"[ValidateCertificate] sslPolicyErrors is Error. {sslPolicyErrors}");
            }

            if (chain == null)
            {
                Console.WriteLine("[ValidateCertificate] chain null.");
                writelog("[ValidateCertificate] chain null.");
                return false;
            }
            return CheckCertificateIsVaild(certificate) && CheckIssuerAndSubject(certificate);
        }

        private bool PinPublicKey(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            X509Certificate2 certificate2 = new X509Certificate2(certificate);
            if (certificate == null)
            {
                Console.WriteLine("[PinPublicKey] certificate null.");
                writelog("[PinPublicKey] certificate null.");
                return false;
            }
            HttpWebRequest httpWebRequest = sender as HttpWebRequest;
            if (httpWebRequest == null)
            {
                Console.WriteLine("[PinPublicKey] request null.");
                writelog("[PinPublicKey] request null.");
            }
            if (chain == null)
            {
                Console.WriteLine("[PinPublicKey] chain null.");
                writelog("[PinPublicKey] chain null.");
                return false;
            }

            return CheckIssuerAndSubject(certificate2) && CheckCertificateIsVaild(certificate2);
        }

        private bool CheckHTTPAvailable(string URL)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                HttpClient httpClient = new HttpClient(handler);
                httpClient.GetAsync(URL).GetAwaiter().GetResult();
                return true;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[CheckHTTPAvailable] error:" + ex.Message.ToString());
                writelog("[CheckHTTPAvailable] error:" + ex.Message.ToString());
            }
            return false;
        }

        private bool GetResponse(HttpClient client, string URL)
        {
            bool flag = false;
            int num = 5;
            while (!flag && num > 0)
            {
                try
                {
                    HttpResponseMessage result = client.GetAsync(URL).GetAwaiter().GetResult();
                    Console.WriteLine("[GetResponse] statusCode:" + result.StatusCode);
                    writelog("[GetResponse] statusCode:" + result.StatusCode);
                    flag = true;
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("[GetResponse] error:" + ex.Message.ToString());
                    writelog("[GetResponse] error:" + ex.Message.ToString());
                    flag = false;
                    Console.WriteLine(string.Format("[GetResponse] error, retry:" + num));
                    writelog(string.Format("[GetResponse] error, retry:" + num));
                    Thread.Sleep(1000);
                }
                num--;
            }
            Console.WriteLine(string.Format("[GetResponse] result:" + flag));
            writelog(string.Format("[GetResponse] result:" + flag));
            return flag;
        }

        private void GetLocalTrustedCert()
        {
            try
            {
                X509Store x509Store = new X509Store(StoreName.TrustedPublisher, StoreLocation.LocalMachine);
                x509Store.Open(OpenFlags.ReadOnly);
                foreach (X509Certificate2 certificate in x509Store.Certificates)
                {
                    TrustedPublisher.Add(certificate);
                }
                x509Store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                x509Store.Open(OpenFlags.ReadOnly);
                foreach (X509Certificate2 certificate2 in x509Store.Certificates)
                {
                    TrustedRoot.Add(certificate2);
                }
                x509Store.Close();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("[GetLocalTrustedCert] error:" + ex.Message.ToString());
                writelog("[GetLocalTrustedCert] error:" + ex.Message.ToString());
            }
        }

        /// <summary>
        /// 從伺服端下載ICC.json檔，下載後會 Run Deserialize接續下載ICC profile .icm檔
        /// </summary>
        /// <param name="m">Monitor Info</param>
        /// <returns> Run Deserialize ICC.json後的 object   </returns>
        public Task<IIC_Metadata> DownloadICCData(MonitorInfo m, string savelPath = "")
        {
            IIC_Metadata _ICC_Metadata = new IIC_Metadata();
            try
            {
                // ICC profiles mapping schema
                FileStream fileStream;
                FileStream fileStream_ICM;

                _ICC_Metadata.Is_Support_ICC_DeviceName = false;

                int index_Support_ICC_DeviceName = -1;

                foreach (string strDeviceName in Support_ICC_DeviceName)
                {
                    index_Support_ICC_DeviceName++;

                    if (m.modelName.ToUpper().Contains(strDeviceName.ToUpper()))
                    {
                        _ICC_Metadata.Is_Support_ICC_DeviceName = true;
                        break;
                    }
                }

                if (_ICC_Metadata.Is_Support_ICC_DeviceName)
                {
                    string strICC_Folder;
                    if (string.IsNullOrEmpty(savelPath))
                    {
                        strICC_Folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + "Dell Display and Peripheral Manager" + @"\ICC\";
                    }
                    else
                    {
                        strICC_Folder = savelPath;
                    }

                    if (!Directory.Exists(strICC_Folder))
                    {
                        Directory.CreateDirectory(strICC_Folder);
                    }

                    _ICC_Metadata.strICC_Folder = String.Format($"{strICC_Folder}");

                    string url = string.Empty;
                    string strFilePath = string.Empty;
                    // 20240627 jim add
                    string str_EnableDDPMMetadataTest = string.Empty;
                    string str_IncludeTestPath = string.Empty;

                    // 20240627 jim add
                    RegistryKey localKey64 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);

                    if (localKey64 != null)
                    {
                        RegistryKey registryKey = localKey64.OpenSubKey(@"SOFTWARE\DELL\Dell Display and Peripheral Manager\UpdateServer", false);

                        if (registryKey != null)
                        {
                            object obj_tmp_key_EnableDDPMMetadataTest = registryKey?.GetValue("EnableDDPMMetadataTest");
                            object obj_tmp_keyIncludeTestPath = registryKey?.GetValue("IncludeTestPath");

                            if (obj_tmp_key_EnableDDPMMetadataTest != null)
                            {
                                str_EnableDDPMMetadataTest = (string)obj_tmp_key_EnableDDPMMetadataTest;
                            }

                            if (obj_tmp_keyIncludeTestPath != null)
                            {
                                str_IncludeTestPath = (string)obj_tmp_keyIncludeTestPath;
                            }
                        }
                    }

                    if (str_EnableDDPMMetadataTest.ToUpper().Contains("TRUE"))
                    {
                        string str_url_prefix = @"https://clientperipherals.dell.com/DDPM/";
                        str_url_prefix += str_IncludeTestPath;
                        str_url_prefix += @"/Windows/Display/ICC/";
                        str_url_prefix += m.modelName;
                        str_url_prefix += @"/ICC.json";

                        url = str_url_prefix;

                        if (!string.IsNullOrEmpty(url))
                        {
                            for (int x = 0; x < Issuer.Length; x++)
                            {
                                Issuers = Issuer[x].Split(",");
                                Subjects = Subject[x].Split(",");
                            }

                            // 向遠端伺服器發送請求
                            if (CheckCA(url))
                            {
                                HttpClient httpClient = new HttpClient();
                                // 取得回應標頭
                                var header = httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;
                                // 取得檔案大小
                                var size = header.Content.Headers.ContentLength;

                                var stream = httpClient.GetStreamAsync(url).Result;
                                fileStream = System.IO.File.Create(Path.Combine(strICC_Folder, Path.GetFileName(url)));
                                stream.CopyToAsync(fileStream).Wait();
                                fileStream.Close();

                                strFilePath = Path.Combine(strICC_Folder, Path.GetFileName(url));

                                if (System.IO.File.Exists(strFilePath))
                                {
                                    string strReadJson = string.Empty;
                                    using (var reader = new StreamReader(strFilePath))
                                    {
                                        strReadJson = reader.ReadToEnd();
                                    }

                                    ///if (strReadJson == string.Empty || strReadJson.Length == 0)
                                    //return Task.FromResult(_preset_settings);

                                    try
                                    {
                                        //_ICC_Metadata._supportDeviceName.Add("U3224KB",);

                                        _ICC_Metadata = RunDeserializeObject(strReadJson);
                                        _ICC_Metadata.strICC_Folder = strICC_Folder;
                                        _ICC_Metadata.Is_Support_ICC_DeviceName = true;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        Console.WriteLine("[DownloadICCData] RunDeserializeObject error:" + ex.Message.ToString());
                                        writelog("[DownloadICCData] RunDeserializeObject error:" + ex.Message.ToString());
                                    }
                                }

                                int count = _ICC_Metadata._support_ICC_DeviceName[m.modelName].Count;

                                // 20240725 jim add

                                str_url_prefix = string.Empty;

                                str_url_prefix = @"https://clientperipherals.dell.com/DDPM/";
                                str_url_prefix += str_IncludeTestPath;
                                str_url_prefix += @"/Windows/Display/ICC/";
                                str_url_prefix += m.modelName;
                                str_url_prefix += @"/";

                                for (int i = 0; i < count; i++)
                                {
                                    url = string.Empty;
                                    url = str_url_prefix + _ICC_Metadata._support_ICC_DeviceName[m.modelName][i].File;

                                    // 向遠端伺服器發送請求
                                    if (CheckCA(url))
                                    {
                                        HttpClient httpClient_ICM = new HttpClient();
                                        // 取得回應標頭
                                        var header_ICM = httpClient_ICM.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).Result;
                                        // 取得檔案大小
                                        var size_ICM = header_ICM.Content.Headers.ContentLength;

                                        var stream_ICM = httpClient_ICM.GetStreamAsync(url).Result;
                                        fileStream_ICM = System.IO.File.Create(Path.Combine(strICC_Folder, Path.GetFileName(url)));
                                        stream_ICM.CopyToAsync(fileStream_ICM).Wait();
                                        fileStream_ICM.Close();

                                        strFilePath = Path.Combine(strICC_Folder, Path.GetFileName(url));

                                        string txtSha256 = BytesToString(GetHashSha256(strFilePath));

                                        if (!string.Equals(txtSha256, _ICC_Metadata._support_ICC_DeviceName[m.modelName][i].SHA256, StringComparison.OrdinalIgnoreCase))
                                        {
                                            writelog($"[DownloadICCData] {_ICC_Metadata._support_ICC_DeviceName[m.modelName][i]} SHA256 error:{txtSha256} , {_ICC_Metadata._support_ICC_DeviceName[m.modelName][i].SHA256}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return Task.FromResult(_ICC_Metadata);
            }
            catch (Exception ex)
            {
                return Task.FromResult(_ICC_Metadata);
            }
        }

        #endregion
    }
}