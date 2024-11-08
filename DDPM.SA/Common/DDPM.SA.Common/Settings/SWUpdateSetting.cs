using DDPM.SA.Common.Security;
using Dell.Client.Framework.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using VcpCore.Common;

namespace DDPM.SA.Common.Settings
{
    public class SWUpdateSetting
    {
        private static string URL = $"https://clientperipherals.dell.com/DDPM/";
        private static string URL_Folder = $"/Windows/Application/";
        private static void SetSWUServer()
        {
            RegistryKey localKey64 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
            URL = URL + URL_Folder;
            if (localKey64 != null)
            {
                RegistryKey registryKey = localKey64.OpenSubKey("SOFTWARE\\Dell\\DDPM Subagent\\", false);
                if (registryKey != null)
                {
                    var obj = registryKey?.GetValue("TestServerURL");
                    if (obj != null)
                    {
                        string s = obj.ToString();
                        if (!string.IsNullOrEmpty(s))
                        {
                            URL = obj + URL_Folder;
                        }
                    }
                }
            }
        }
        public static SWUpdateHelper GetSWMetadata(bool isSkipCA, out string info, ISettingsManagerSA settingsPlugin, List<string> InserInfoPkey, Logs logs)
        {
            SWUpdateHelper data = new SWUpdateHelper();
            SetSWUServer();
            CertificateCheck certificateCheck = new CertificateCheck(logs);
            if (!isSkipCA)
            {
                if (!certificateCheck.CheckURLCACertificate(URL))
                {
                    info = $"{nameof(GetSWMetadata)} URL CA check fail";
                    logs?.DebugMsg_1(info);
                    return data;
                }
            }
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        client.Timeout = TimeSpan.FromSeconds(5);
                        HttpResponseMessage response = client.GetAsync(URL + "SWMetaData.json").Result;
                        response.EnsureSuccessStatusCode();
                        string fileContent = response.Content.ReadAsStringAsync().Result;
                        List<string> InfoPkey = new List<string>();
                        if (InserInfoPkey != null && InserInfoPkey.Count > 0)
                        {
                            InfoPkey = InserInfoPkey;
                        }
                        else
                        {
                            if (settingsPlugin != null)
                            {
                                InfoPkey = settingsPlugin.GetInfos().Result;
                            }
                        }
                        if (InfoPkey == null || InfoPkey.Count == 0)
                        {
                            //if read info failed, load default key as well
                            InfoPkey = new List<string>();
                            InfoPkey.Add(DDPM.SA.Obfuscation.InfoHash.Info_Hash);
                        }
                        string szInfo = string.Empty;
                        string jsonString = DDPMFileSecurity.VerifyDDPMMetadata(null, fileContent, InfoPkey, out szInfo);
                        if (!string.IsNullOrEmpty(szInfo) && settingsPlugin != null)
                        {
                            settingsPlugin.AddInfo(szInfo);//pass info to settings manager and judge if new to add
                        }
                        if (!string.IsNullOrEmpty(jsonString))
                        {
                            jsonString = jsonString.Replace("%1/", URL);
                            data = JsonSerializer.Deserialize<SWUpdateHelper>(jsonString);
                            if (data != null)
                            {
                                foreach (Software software in data.Softwares)
                                {
                                    string version =
                                    Regex.Replace(Convert.ToInt32(software.SoftwareVersion).ToString("D4"), @"(.{1})(.{1})(.{1})(.{1})", "$1.$2.$3.$4");
                                    software.ServerPath = software.ServerPath.Replace("%2", $"{software.SoftwareName}-Setup-v{version}");
                                    software.DdpmSwUpdaterServer_path = software.DdpmSwUpdaterServer_path.Replace("%21", $"DdpmSwUpdater");
                                }
                                info = $"{nameof(GetSWMetadata)} done";
                                logs?.DebugMsg_1(info);
                            }
                            else
                            {
                                info = $"{nameof(GetSWMetadata)} done but Deserialize fail";
                                logs?.DebugMsg_1(info);
                            }
                        }
                        else
                        {
                            info = $"{nameof(GetSWMetadata)} done but jsonString is null or empty.";
                            logs?.DebugMsg_1(info);
                        }
                    }
                    catch (JsonException ex)
                    {
                        data = new SWUpdateHelper();
                        info = $"{nameof(GetSWMetadata)} JSON Deserialize error:{ex.Message}";
                        logs?.DebugMsg_1(info);
                    }
                }
            }
            catch (Exception ex)
            {
                data = new SWUpdateHelper();
                info = $"{nameof(GetSWMetadata)} error:{ex.Message}";
                logs?.DebugMsg_1(info);
            }
            return data;
        }
        public static InterruptScreenRoot InterruptScreen_Metadata(bool isSkipCA, out string info, ISettingsManagerDev settingsPlugin, List<string> InserInfoPkey, Logs logs)
        {
            InterruptScreenRoot result = null;
            logs?.DebugMsg_1("[InterruptScreen_Metadata], start.");
            SetSWUServer();
            CertificateCheck certificateCheck = new CertificateCheck(logs);
            if (!isSkipCA)
            {
                if (!certificateCheck.CheckURLCACertificate(URL))
                {
                    info = $"{nameof(GetSWMetadata)} URL CA check fail";
                    logs?.DebugMsg_1(info);
                    return result;
                }
            }
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        client.Timeout = TimeSpan.FromSeconds(5);
                        HttpResponseMessage response = client.GetAsync(URL + "AppUpdates.json").Result;
                        response.EnsureSuccessStatusCode();
                        string fileContent = response.Content.ReadAsStringAsync().Result;
                        List<string> InfoPkey = new List<string>();
                        if (InserInfoPkey != null && InserInfoPkey.Count > 0)
                        {
                            InfoPkey = InserInfoPkey;
                        }
                        else
                        {
                            if (settingsPlugin != null)
                            {
                                InfoPkey = settingsPlugin.GetInfos().Result;
                            }
                        }
                        if (InfoPkey == null || InfoPkey.Count == 0)
                        {
                            //if read info failed, load default key as well
                            InfoPkey = new List<string>();
                            InfoPkey.Add(DDPM.SA.Obfuscation.InfoHash.Info_Hash);
                        }
                        string szInfo = string.Empty;
                        string jsonString = DDPMFileSecurity.VerifyDDPMMetadata(null, fileContent, InfoPkey, out szInfo);
                        if (!string.IsNullOrEmpty(szInfo) && settingsPlugin != null)
                        {
                            settingsPlugin.AddInfo(szInfo);//pass info to settings manager and judge if new to add
                        }
                        if (!string.IsNullOrEmpty(jsonString))
                        {
                            result = Newtonsoft.Json.JsonConvert.DeserializeObject<InterruptScreenRoot>(jsonString);
                            if (result != null)
                            {
                                foreach (FeaturesList interruptScreenRoot in result.featuresList)
                                {
                                    if (interruptScreenRoot != null && interruptScreenRoot.content != null)
                                    {
                                        interruptScreenRoot.content.image = DownloadImageAsByteArray($@"{URL}\{interruptScreenRoot.content.imageUrl}");
                                    }
                                }
                                info = $"{nameof(InterruptScreen_Metadata)} Pass";
                            }
                            else
                            {
                                info = $"{nameof(InterruptScreen_Metadata)} result is null";
                            }
                        }
                        else
                        {
                            info = $"{nameof(InterruptScreen_Metadata)} done but jsonString is null or empty.";
                            logs?.DebugMsg_1(info);
                        }
                    }
                    catch (JsonException ex)
                    {
                        info = $"{nameof(InterruptScreen_Metadata)} JSON Deserialize error:{ex.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                info = $"{nameof(GetSWMetadata)} error:{ex.Message}";
                logs?.DebugMsg_1($"[InterruptScreen_Metadata], Error : {ex.Message}");
            }
            return result;
        }
        private static BitmapImage LoadLocalImage(string path)
        {
            BitmapImage bitmap = new BitmapImage();
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    byte[] imageBytes = DownloadImageAsByteArray(path);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading image: {ex.Message}");
                }
            }
            return bitmap;
        }
        // 使用 HttpClient 從網站下載圖片並轉換為 byte[]
        private static byte[] DownloadImageAsByteArray(string url)
        {
            byte[] imageBytes = new byte[0];
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    imageBytes = client.GetByteArrayAsync(url).Result;
                }
            }
            catch
            {

            }
            return imageBytes;
        }
    }
}
