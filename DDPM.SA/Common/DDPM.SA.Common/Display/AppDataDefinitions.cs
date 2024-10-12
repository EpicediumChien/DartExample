using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace DDPM.SA.Common
{
    public class AppData : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string strPropName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(strPropName));
        }

        #endregion INotifyPropertyChanged Members

        private string appicon;

        public string AppIcon
        {
            get { return appicon; }
            set
            {
                appicon = value;
                OnPropertyChanged("AppIcon");
            }
        }

        private string appname;

        public string AppName
        {
            get { return appname; }
            set
            {
                appname = value;
                OnPropertyChanged("AppName");
            }
        }

        private int apppresetidx;

        public int AppPresetIdx
        {
            get { return apppresetidx; }
            set
            {
                if (apppresetidx != value)
                    apppresetidx = value;
                OnPropertyChanged("AppPresetIdx");
            }
        }

        private string apppath;

        public string AppPath
        {
            get { return apppath; }
            set
            {
                apppath = value;
                OnPropertyChanged("AppPath");
            }
        }

        private Visibility isdeleteable;

        public Visibility IsDeleteAble
        {
            get { return isdeleteable; }
            set
            {
                isdeleteable = value;
                OnPropertyChanged("IsDeleteAble");
            }
        }

        private List<string> supportpreset;

        public List<string> SupportPreset
        {
            get { return supportpreset; }
            set
            {
                supportpreset = value;
                OnPropertyChanged("SupportPreset");
            }
        }
    }

    public class Test_AddAppCollectionData
    {
        public ObservableCollection<AppData> AppsList { get; set; }
        public List<ColorPresetSettings> _monitorConfigs { get; set; }

        private static Test_AddAppCollectionData INSTANCE = null;

        public static Test_AddAppCollectionData GetInstance()
        {
            if (INSTANCE == null)
            {
                INSTANCE = new Test_AddAppCollectionData();
                INSTANCE.AppsList = new ObservableCollection<AppData>();
                INSTANCE._monitorConfigs = new List<ColorPresetSettings>();
            }

            return INSTANCE;
        }
    }

    public class Bind_AddFullPage_AppCollectionData
    {
        public string AppName { get; set; } = string.Empty;
        public string AppUserModelID { get; set; } = string.Empty;
        public string AppType { get; set; } = string.Empty;
        public string AppPath { get; set; } = String.Empty;
        public DateTime InstalledDate { get; set; }
        public string AppIcon { get; set; } = String.Empty;
    }

    //
    //For ListView ItemResource used
    //
    public class AppCollectionData
    {
        public string AppName { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
        public string AppUserModelID { get; set; } = string.Empty;
        public string AppType { get; set; } = string.Empty;
        public string AppPath { get; set; } = String.Empty;
        public DateTime InstalledDate { get; set; }

        public static string app_list_to_message_string(List<AppCollectionData> apps)
        {
            /*
             * public string AppName { get; set; } = string.Empty;
             * public string AppUserModelID { get; set; } = string.Empty;
             * public string AppType { get; set; } = string.Empty;
             * public string AppPath { get; set; } = String.Empty;
             * public DateTime InstalledDate { get; set; }
             */
            string tmp = string.Empty;
            if (apps != null && apps.Count > 0)
            {
                foreach (AppCollectionData app in apps)
                {
                    tmp += app.AppName + "|,";
                    tmp += app.IconName + "|,";
                    tmp += app.AppUserModelID + "|,";
                    tmp += app.AppType + "|,";
                    tmp += app.AppPath + "|,";
                    tmp += app.InstalledDate.ToString("yyyy-MM-dd");
                    tmp += "|]";
                }
                if (!string.IsNullOrEmpty(tmp))
                    return tmp;
            }
            return string.Empty;
        }

        public static List<AppCollectionData> message_string_to_app_list(string message)
        {
            if (string.IsNullOrEmpty(message))
                return null;
            string[] apps = message.Split("|]");
            if (apps.Length <= 0)
                return null;

            List<AppCollectionData> app_list = new List<AppCollectionData>();
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US"); ;
            DateTimeStyles styles = DateTimeStyles.AssumeLocal;
            DateTime dateResult;
            foreach (string app in apps)
            {
                if (string.IsNullOrEmpty(app) || app.Length < 2)
                    continue;

                string[] items = app.Split("|,");
                if (items.Length != 6)
                    continue;

                AppCollectionData item = new AppCollectionData();
                item.AppName = items[0];
                item.IconName = items[1];
                item.AppUserModelID = items[2];
                item.AppType = items[3];
                item.AppPath = items[4];
                if (DateTime.TryParse(items[5], culture, styles, out dateResult))
                    item.InstalledDate = dateResult;
                else
                    item.InstalledDate = DateTime.Now;

                app_list.Add(item);
            }

            return app_list;
        }
    }

    public enum ColorPresetRunType
    {
        Manual = 0,
        Auto = 1
    }

    public enum ColorManagementStatus
    {
        Off = 0,
        On = 1
    }

    public enum ColorManagementRunType
    {
        Off = 0,
        Bymonitor = 1,
        Byhost = 2
    }


    internal class AppDataDefinitions
    {
    }
}