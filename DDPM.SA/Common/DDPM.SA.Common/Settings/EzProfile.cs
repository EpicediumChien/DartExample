using DdmLibrary.Utility;
using DDPM.SA.Common.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Settings
{
    public class EAProfileDDPM
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int Layout { get; set; }

        public List<EAAppInfoDDPM> AppInfos { get; set; }

        public EAProfileDDPM()
        {
        }

        public EAProfileDDPM(int id, string name, int layout, List<EAAppInfoDDPM> apps)
        {
            ID = id;
            Name = name;
            Layout = layout;
            AppInfos = apps == null ? new List<EAAppInfoDDPM>() : apps.ConvertAll(app => new EAAppInfoDDPM(app.Name, app.Path, app.IsUWP, app.AppUserModelID, app.Param));

        }

        //Robert_Lin, 2025-1-9 Added
        public bool IsCustomLayout
        {
            get { return Layout >= EAEMConstants.EAID_FirstCustom; }
        }
    }

    public class EAAppInfoDDPM
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public bool IsUWP { get; set; }

        public string AppUserModelID { get; set; }

        public string Param { get; set; }

        public EAAppInfoDDPM()
        {
        }

        public EAAppInfoDDPM(string name, string path, bool isUWP, string appUserModelID, string param)
        {
            Name = name;
            Path = path;
            IsUWP = isUWP;
            AppUserModelID = appUserModelID;
            Param = param;
        }
    }

    public class EasyArrangementDDPM
    {
        public const int maxMRU = 5;

        public List<DesktopDDPM> Desktops { get; set; }

        public EasyArrangementDDPM()
        {
            Desktops = new List<DesktopDDPM>();
        }

        //Robert_Lin, 2025-4-8 support multiple partitions
        /// <summary>
        /// Return the index to Desktops, or -1 if not found.
        /// </summary>
        /// <param name="desktopId"></param>
        /// <returns></returns>
        public int FindIndexOfDesktop(string desktopId)
        {
            if ((Desktops == null) || (Desktops.Count <= 0))
                return -1;

            for (int idx = 0; idx < Desktops.Count; idx++)
            {
                if (string.IsNullOrEmpty(Desktops[idx].ID))
                    continue;

                if (Desktops[idx].ID.Equals(desktopId))
                {
                    return idx;
                }
            }
            return -1;
        }
    }

    public class DesktopDDPM
    {
        public string ID { get; set; }

        public int Index { get; set; }

        public int ActiveLayout { get; set; }

        public List<int> LayoutMRU { get; set; }

        public List<int> ProfileMRU { get; set; }

        public List<EzProfileDDPM> Profiles { get; set; }

        public List<EzProfileSettingDDPM> ProfileSettings { get; set; }

        public DesktopDDPM(string id, int activelayout)
        {
            ID = id;
            ActiveLayout = activelayout;
            LayoutMRU = new List<int>();
            LayoutMRU.Add(1);
            LayoutMRU.Add(2);
            LayoutMRU.Add(3);
            LayoutMRU.Add(4);
            LayoutMRU.Add(8);
            ProfileMRU = new List<int>();
            Profiles = new List<EzProfileDDPM>();
            ProfileSettings = new List<EzProfileSettingDDPM>();
        }
    }

    public class EzProfileSettingDDPM
    {
        public int ID { get; set; }

        public bool Auto { get; set; }

        public long? AutoStartTime { get; set; }

        public bool StartUpLaunch { get; set; }

        public EzProfileSettingDDPM(int id, bool auto, long autostarttime, bool startuplaunch)
        {
            ID = id;
            Auto = auto;
            AutoStartTime = autostarttime;
            StartUpLaunch = startuplaunch;
        }
    }

    public class EzProfileDDPM
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int Layout { get; set; }

        public bool Auto { get; set; }

        public long? AutoStartTime { get; set; }

        public bool StartUpLaunch { get; set; }

        public List<EAAppInfoDDPM> AppInfos { get; set; }

        public EzProfileDDPM(int id, string name, int layout, bool auto, long autostarttime, bool startuplaunch, List<EAAppInfoDDPM> apps)
        {
            ID = id;
            Name = name;
            Layout = layout;
            Auto = auto;
            AutoStartTime = autostarttime;
            StartUpLaunch = startuplaunch;
            AppInfos = ((apps == null) ? new List<EAAppInfoDDPM>() : apps.ConvertAll((EAAppInfoDDPM app) => new EAAppInfoDDPM(app.Name, app.Path, app.IsUWP, app.AppUserModelID, app.Param)));
        }
    }
}