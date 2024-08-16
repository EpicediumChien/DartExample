using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DDPM.SA.Common.Settings
{
    public class DDPMSettings
    {
        private static readonly object Lock = new object();

        public DDPMAppSettings AppSettings { get; set; }

        public DDPMUserSettings UserSettings { get; set; }

        public DDPMSettings(DDPMAppSettings appsettings, DDPMUserSettings usersettings)//, DDMMonitorSettings monitorsettings)
        {
            AppSettings = appsettings;
            UserSettings = usersettings;
            //MonitorSettings = monitorsettings;
        }

        public static bool exportSettingstoFile(string path, DDPMAppSettings appsettings, DDPMUserSettings usersettings)//, DDMMonitorSettings monitorsettings)
        {
            try
            {
                DDPMSettings value = new DDPMSettings(appsettings, usersettings);//, monitorsettings);
                byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
                uint value2 = calCheckSum(bytes, bytes.Length);
                //if (!MISCLibrary.IsSymbolic(path))
                {
                    using (FileStream fileStream = File.Create(path))
                    {
                        fileStream.Write(bytes, 0, bytes.Length);
                        fileStream.Write(BitConverter.GetBytes(value2), 0, 4);
                        fileStream.Flush();
                        fileStream.Close();
                    }
                    return true;
                }
            }
            catch (Exception)// ex)
            {
            }
            return false;
        }

        public static DDPMSettings getSettingsforImport(string path, ref DDPMAppSettings appsettings, ref DDPMUserSettings usersettings)//, ref DDPMMonitorSettings monitorsettings)
        {
            DDPMSettings dDMSettings = null;
            try
            {
                if (File.Exists(path))// && !MISCLibrary.IsSymbolic(path))
                {
                    int num = (int)new FileInfo(path).Length;
                    if (num >= 4)
                    {
                        byte[] array = File.ReadAllBytes(path);
                        if (calCheckSum(array, num - 4).Equals(BitConverter.ToUInt32(array, num - 4)))
                        {
                            dDMSettings = JsonConvert.DeserializeObject<DDPMSettings>(Encoding.UTF8.GetString(array, 0, array.Length - 4));
                            appsettings = dDMSettings.AppSettings;
                            usersettings = dDMSettings.UserSettings;
                            return dDMSettings;
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
            catch (Exception)// ex)
            {
            }
            return null;
        }

        public static uint calCheckSum(byte[] content, int count)
        {
            uint num = 0u;
            for (int i = 0; i < count; i++)
            {
                num += content[i];
            }
            return num;
        }

        private static byte[] GetSHA256(byte[] message, int offset, int count)
        {
            using SHA256 sHA = SHA256.Create();
            return sHA.ComputeHash(message, offset, count);
        }
    }
}