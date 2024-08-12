using System;
using System.Collections.Generic;
using static VcpCore.Common.User32;
using static VcpCore.Common.dxva2;

namespace DDPM.SA.Common
{
    [Serializable]

    public class InstalledAppInfo
    {
        public string AppName;

        public string IconName;

        public string AppInstallPath;

        public DateTime lastModifyTime;

        public bool isDesktopApp;

        public string AppUserModelID;

        public InstalledAppInfo()
        {
            AppName = "";
            IconName = "";
            AppInstallPath = "";
            lastModifyTime = DateTime.Now;
            isDesktopApp = true;
            AppUserModelID = "";
        }

        public InstalledAppInfo(string name, string pth, string iconName, DateTime time, bool bDesktopApp, string appUserModelID)
        {
            AppName = name;
            IconName = iconName;
            AppInstallPath = pth;
            lastModifyTime = time;
            isDesktopApp = bDesktopApp;
            AppUserModelID = appUserModelID;
        }
    }

}
