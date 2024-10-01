using DDPM.SA.Common;
using MiniInstaller;
using System.Diagnostics;
using System.Reflection;

internal class Program
{

    private static void Main(string[] args)
    {
        LogManage.SetPath();
        LaunchInstaller launchInstaller = new LaunchInstaller();
        SWUErrorCode ret = launchInstaller.LaunchUpdate().Result;
        LogManage.LogMessage(ret.ToString());
    }
}