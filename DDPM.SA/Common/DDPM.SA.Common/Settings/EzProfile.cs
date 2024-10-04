using DdmLibrary.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Settings
{
    public class ProfileDDPM
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Layout { get; set; }
        public bool Auto { get; set; }
        public long? AutoStartTime { get; set; }
        public bool StartUpLaunch { get; set; }
        public List<EAAppInfoDDPM> AppInfos { get; set; }
    }
    public class EAProfileDDPM
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int Layout { get; set; }

        public List<EAAppInfoDDPM> AppInfos { get; set; }

        //// 新增欄位
        public bool IsManualLaunch { get; set; } // ManulRB

        public bool IsAutoLaunch { get; set; }   // AutoRB

        public string SelectedHour { get; set; } // HourCB

        public string SelectedMinute { get; set; } // MinuteCB

        public string SelectedAMPM { get; set; } // AMPMCB

        public bool IsLaunchAtStartup { get; set; } // StartupCB

        public EAProfileDDPM(int id, string name, int layout, List<EAAppInfoDDPM> apps, bool isManualLaunch, bool isAutoLaunch, string selectedHour, string selectedMinute, string selectedAMPM, bool isLaunchAtStartup)
        {
            ID = id;
            Name = name;
            Layout = layout;
            AppInfos = apps == null ? new List<EAAppInfoDDPM>() : apps.ConvertAll(app => new EAAppInfoDDPM(app.Name, app.Path, app.IsUWP, app.AppUserModelID, app.Param));
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
}
