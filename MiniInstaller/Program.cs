using DDPM.SA.Common;
using MiniInstaller;
using System.Diagnostics;
using System.Reflection;

internal class Program
{

    private static void Main(string[] args)
    {
        SWUpdateInfo SWUpdateInfo = new SWUpdateInfo()
        {
            SoftwareName = "DDPM",
            InstallPaths = @"D:\SW_Update\DDPM-Setup-v2.0.0.40-Debug.exe",
            TheLatestVersion = "2.0.0.40"
        };
        LaunchInstaller launchInstaller = new LaunchInstaller();
        SWUErrorCode ret = launchInstaller.Install(SWUpdateInfo).Result;
        Debug.WriteLine(ret);
    }
}