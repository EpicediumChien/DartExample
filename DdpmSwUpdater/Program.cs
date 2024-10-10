using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DdpmSwUpdater;
using Newtonsoft.Json.Linq;
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
        try
        {
            string exePath = Assembly.GetExecutingAssembly().Location;
            string folderPath = Path.GetDirectoryName(exePath);
            string folderName = Path.GetFileName(folderPath);
            DirectoryInfo parentFolder = Directory.GetParent(folderPath);
            string parentFolderName = parentFolder != null ? parentFolder.Name : folderName;
            DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, "SOFTWARE\\Dell Display and Peripheral Manager", "DdpmSwUpdater", parentFolderName);
        }
        catch (Exception ex) { LogManage.LogMessage(ex.ToString()); }
        LaunchInstaller launchInstaller = new LaunchInstaller();
        SWUErrorCode ret = launchInstaller.LaunchUpdate().Result;
        LogManage.LogMessage($"DdpmSwUpdater End Resule:{ret.ToString()}");
    }
}