using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.UI.Common;
using System.Windows.Media;
using VcpCore.Common;

namespace DDPM.UI.Plugin.DdpmHomePlugin.Model
{
    public class HomeDeviceObj_Unused : ObservableObject
    {
        private eDeviceCategory _deviceCategory;
        private string _deviceName;
        private ImageSource? _deviceImage;
        private double _normalWidth = 400;

        private MonitorInfo? _monitorInfo;

        public HomeDeviceObj_Unused()
        {
        }

        public ImageSource? DeviceImage
        {
            get => _deviceImage;
            set => SetProperty(ref _deviceImage, value);
        }

        public string DeviceName
        {
            get => _deviceName;
            set => SetProperty<string>(ref _deviceName, value);
        }

        public eDeviceCategory DeviceCategory
        {
            get => _deviceCategory;
            set => SetProperty(ref _deviceCategory, value);
        }

        #region RWD Adjustment

        public double NormalWidth
        {
            get => _normalWidth;
            set
            {
                SetProperty(ref _normalWidth, value);
                OnPropertyChanged("HoverWidth");
                OnPropertyChanged("ItemWidth");
            }
        }

        public double HoverWidth
        {
            get
            {
                return NormalWidth * (double)1.00;
            }
        }

        public double ItemWidth
        {
            get
            {
                return NormalWidth * 1.1;
            }
        }

        #endregion RWD Adjustment

        public MonitorInfo? MonitorInfo
        {
            get => _monitorInfo;
            set => SetProperty(ref _monitorInfo, value);
        }

        public string FwVer
        {
            get => "(N/A)";
        }

        public string ServiceTag
        {
            get
            {
                if (MonitorInfo != null)
                {
                    return MonitorInfo.edid.ServiceTag;
                }
                else
                {
                    return "(null)";
                }
            }
        }

        public string MfgDate
        {
            get
            {
                if (MonitorInfo != null)
                {
                    DateTime dtMfg = new DateTime(MonitorInfo.edid.Year, MonitorInfo.edid.Month, 1);
                    string mfgDate = dtMfg.ToString("MMM yyyy");
                    return mfgDate;
                }
                else
                {
                    return "(N/A)";
                }
            }
        }
    }
}