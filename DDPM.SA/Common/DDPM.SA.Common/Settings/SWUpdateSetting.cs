using DDPM.SA.Common.Security;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        public static SWUpdateHelper GetSWMetadata(bool isSkipCA, out string info)
        {
            SWUpdateHelper data = new SWUpdateHelper();
            SetSWUServer();
            CertificateCheck certificateCheck = new CertificateCheck();
            if (!isSkipCA)
            {
                if (!certificateCheck.CheckURLCACertificate(URL))
                {
                    info = $"{nameof(GetSWMetadata)} URL CA check fail";
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
                        string jsonString = response.Content.ReadAsStringAsync().Result;
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
                                    software.MiniInstallerServer_path = software.MiniInstallerServer_path.Replace("%21", $"MiniInstaller");
                                }
                                info = $"{nameof(GetSWMetadata)} done";
                            }
                            else
                            {
                                info = $"{nameof(GetSWMetadata)} done but Deserialize fail";
                            }
                        }
                        else
                        {
                            info = $"{nameof(GetSWMetadata)} done but jsonString is null or empty";
                        }
                    }
                    catch (JsonException ex)
                    {
                        info = $"{nameof(GetSWMetadata)} JSON Deserialize error:{ex.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                info = $"{nameof(GetSWMetadata)} error:{ex.Message}";
            }
            return data;
        }
    }
}
