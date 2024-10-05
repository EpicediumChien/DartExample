using DdmLibrary.Utility;
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

        public bool Auto { get; set; }// Auto 為 true 代表自動啟動，false 代表手動啟動

        public long? AutoStartTime { get; set; }// AutoStartTime 為自動啟動的時間，單位為秒

        public bool StartUpLaunch { get; set; }

        public string Model { get; set; }

        public string ServiceTag { get; set; }

        public List<EAAppInfoDDPM> AppInfos { get; set; }

        public EAProfileDDPM()
        {
        }

        public EAProfileDDPM(int id, string name, int layout, bool Auto, long? autoStartTime, bool startUpLaunch, string model, string serviceTag, List<EAAppInfoDDPM> apps)
        {
            ID = id;
            Name = name;
            Layout = layout;
            AutoStartTime = autoStartTime;
            StartUpLaunch = startUpLaunch;
            Model = model;
            ServiceTag = serviceTag;
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
