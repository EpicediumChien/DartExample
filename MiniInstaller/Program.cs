using DDPM.SA.Common;
using MiniInstaller;
using System.Diagnostics;
using System.Reflection;

internal class Program
{

    private static void Main(string[] args)
    {
        LaunchInstaller launchInstaller = new LaunchInstaller();
        SWUErrorCode ret = launchInstaller.LaunchUpdate().Result;
        Debug.WriteLine(ret);
    }
}