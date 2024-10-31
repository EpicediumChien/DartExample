using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DdpmSwUpdater;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Media.Animation;
using Windows.Devices.Geolocation;
internal class Program
{
    private static void Main(string[] args)
    {
        LogManage.SetPath();
        string registryKey = @"SOFTWARE\Dell Display and Peripheral Manager";
        try
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string folderPath = Path.GetDirectoryName(exePath);
            string folderName = Path.GetFileName(folderPath);
            DirectoryInfo parentFolder = Directory.GetParent(folderPath);
            string parentFolderName = parentFolder != null ? parentFolder.Name : folderName;
            DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey, "DdpmSwUpdater", parentFolderName);
        }
        catch (Exception ex) { LogManage.LogMessage(ex.ToString()); }
        LaunchInstaller launchInstaller = new LaunchInstaller();
        SWUErrorCode ret = launchInstaller.LaunchUpdate().Result;
        LogManage.LogMessage($"DdpmSwUpdater End Resule:{ret.ToString()}");
        try
        {
            object o = DDPMRegistryHelper.ReadRegistryKey(RegistryHive.LocalMachine, registryKey, "SW_Available_date");
            LogManage.LogMessage($"DdpmSwUpdater ,ReadRegistryData SW_Available_date o = {o}.");
            if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
            {
                string UpdateVersion = LogManage.Version;
                string Results = ret == SWUErrorCode.NoError ? "success" : "failed";
                string FailureMessage = ret.ToString();
                string SW_Update_date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                string ErrorCode = ((int)ret).ToString();
                LogManage.LogMessage($"WriteRegistryKey UpdateVersion : {UpdateVersion}");
                LogManage.LogMessage($"WriteRegistryKey Results : {Results}");
                LogManage.LogMessage($"WriteRegistryKey FailureMessage : {FailureMessage}");
                LogManage.LogMessage($"WriteRegistryKey SW_Update_date : {SW_Update_date}");
                LogManage.LogMessage($"WriteRegistryKey ErrorCode : {ErrorCode}");
                DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey, nameof(UpdateVersion), UpdateVersion);
                DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey, nameof(Results), Results);
                DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey, nameof(FailureMessage), FailureMessage);
                DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey, nameof(SW_Update_date), SW_Update_date);
                DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey, nameof(ErrorCode), ErrorCode);
            }
        }
        catch (Exception ex) { LogManage.LogMessage(ex.ToString()); }
    }
}