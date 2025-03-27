using DDPM.SA.Common.Method;
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
        public static SWUpdateHelper GetSWMetadata(bool isSkipCA, out string info, ISettingsManagerSA settingsPlugin, List<string> InserInfoPkey, Logs logs)
        {
            SWUpdateHelper data = new SWUpdateHelper();
            string SW_URL = Download.GetTestServerURL() + GlobalDefinitions.SW_URL_Folder;
            CertificateCheck certificateCheck = new CertificateCheck(logs);
            if (!isSkipCA && !certificateCheck.CheckURLCACertificate(SW_URL))
            {
                info = $"{nameof(GetSWMetadata)} URL CA check fail";
                logs?.DebugMsg_1(info);
                return data;                
            }
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        client.Timeout = TimeSpan.FromSeconds(60);
                        HttpResponseMessage response = client.GetAsync(SW_URL + "SWMetaData.json").Result;
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
                            InfoPkey.AddRange(DDPM.SA.Obfuscation.InfoHash.Info_Hash);
                        }
                        string szInfo = string.Empty;
                        string jsonString = DDPMFileSecurity.VerifyDDPMMetadata(null, fileContent, InfoPkey, out szInfo);
                        if (!string.IsNullOrEmpty(szInfo) && settingsPlugin != null)
                        {
                            settingsPlugin.AddInfo(szInfo);//pass info to settings manager and judge if new to add
                        }
                        if (!string.IsNullOrEmpty(jsonString))
                        {
                            jsonString = jsonString.Replace("%1/", SW_URL);
                            data = JsonSerializer.Deserialize<SWUpdateHelper>(jsonString);
                            if (data != null)
                            {
                                foreach (Software software in data.Softwares)
                                {
                                    //software.SoftwareVersion = software.SoftwareVersion;
                                    software.ServerPath = software.ServerPath.Replace("%2", $"{software.SoftwareName}-Setup_{software.SoftwareVersion}");
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
        public static bool CompareVersions(string oldVersion, string newVersion, Logs logs)
        {
            logs?.DebugMsg_1($"{nameof(CompareVersions)} start");
            bool isNeedUpdate = false;
            logs?.DebugMsg_1($"{nameof(CompareVersions)} oldVersion IsNullOrEmpty : {(string.IsNullOrEmpty(oldVersion) ? "Yes" : "No")}");
            logs?.DebugMsg_1($"{nameof(CompareVersions)} newVersion IsNullOrEmpty : {(string.IsNullOrEmpty(newVersion) ? "Yes" : "No")}");
            if (!string.IsNullOrEmpty(oldVersion) && !string.IsNullOrEmpty(newVersion) &&
                oldVersion.Contains(".") && newVersion.Contains("."))
            {
                logs?.DebugMsg_1($"{nameof(CompareVersions)} oldVersion : {oldVersion}");
                logs?.DebugMsg_1($"{nameof(CompareVersions)} newVersion : {newVersion}");
                string[] oldVersion_Array = oldVersion.Split('.');
                string[] newVersion_Array = newVersion.Split('.');
                logs?.DebugMsg_1($"{nameof(CompareVersions)} oldVersion_Array Is Null : {(oldVersion_Array == null ? "Yes" : "No")}");
                logs?.DebugMsg_1($"{nameof(CompareVersions)} newVersion_Array Is Null : {(newVersion_Array == null ? "Yes" : "No")}");
                if (oldVersion_Array != null && newVersion_Array != null)
                {
                    logs?.DebugMsg_1($"{nameof(CompareVersions)} oldVersion_Array.Length : {oldVersion_Array.Length}");
                    logs?.DebugMsg_1($"{nameof(CompareVersions)} newVersion_Array.Length : {newVersion_Array.Length}");
                    if (oldVersion_Array.Length == 4 && 
                        newVersion_Array.Length == 4 &&
                        oldVersion_Array.Length == newVersion_Array.Length)
                    {                        
                        for (int i = 0; i < oldVersion_Array.Length; i++)
                        {
                            if (int.TryParse(newVersion_Array[i], out int newVersion_int) && int.TryParse(oldVersion_Array[i], out int oldVersion_int))
                            {
                                logs?.DebugMsg_1($"{nameof(CompareVersions)} oldVersion_int : {oldVersion_int}");
                                logs?.DebugMsg_1($"{nameof(CompareVersions)} newVersion_int : {newVersion_int}");
                                if (newVersion_int > oldVersion_int)
                                {
                                    isNeedUpdate = true;
                                    break;
                                }
                                else if (oldVersion_int > newVersion_int)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                logs?.DebugMsg_1($"{nameof(CompareVersions)} int.TryParse Error");
                                logs?.DebugMsg_1($"{nameof(CompareVersions)} int.TryParse oldVersion_Array[i] : {oldVersion_Array[i]}");
                                logs?.DebugMsg_1($"{nameof(CompareVersions)} int.TryParse newVersion_Array[i] : {newVersion_Array[i]}");
                                break;
                            }
                        }                        
                    }
                }
            }
            logs?.DebugMsg_1($"{nameof(CompareVersions)} done");
            return isNeedUpdate;
        }
        public static InterruptScreenRoot InterruptScreen_Metadata(bool isSkipCA, out string info, ISettingsManagerDev settingsPlugin, List<string> InserInfoPkey, Logs logs)
        {
            InterruptScreenRoot result = null;
            logs?.DebugMsg_1("[InterruptScreen_Metadata], start.");
            string SW_URL = Download.GetTestServerURL() + GlobalDefinitions.SW_URL_Folder;
            CertificateCheck certificateCheck = new CertificateCheck(logs);
            if (!isSkipCA && !certificateCheck.CheckURLCACertificate(SW_URL))
            {
                info = $"{nameof(GetSWMetadata)} URL CA check fail";
                logs?.DebugMsg_1(info);
                return result;
            }
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        client.Timeout = TimeSpan.FromSeconds(60);
                        HttpResponseMessage response = client.GetAsync(SW_URL + "AppUpdates.json").Result;
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
                            InfoPkey.AddRange(DDPM.SA.Obfuscation.InfoHash.Info_Hash);
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
                                        interruptScreenRoot.content.image = DownloadImageAsByteArray($@"{SW_URL}\{interruptScreenRoot.content.imageUrl}");
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
#if DEBUG
                    Console.WriteLine($"Error loading image: {ex.Message}");
#endif
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
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"[DownloadImageAsByteArray] exception: {ex.Message}");
#endif
            }
            return imageBytes;
        }
    }
}
