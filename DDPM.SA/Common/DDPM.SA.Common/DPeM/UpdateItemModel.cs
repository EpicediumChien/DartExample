using DPeMPublic.Common.Enums;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace DDPM.SA.Common
{
    [Serializable]
    public class UpdateItemInfo : INotifyPropertyChanged
    {
        public string UpdateType;
        public string UpdateSeverity;
        public string _newVersion;
        public string Description;

        public string CurrentVersion;
        public string DeviceId;
        public int DeviceIndex;
        public string DeviceModelNumber;
        public string DeviceName;
        public string DevicePath;
        public DeviceType DeviceType;
        public int FrimwareUpdatePath;
        public string InstallPath;
        public int InstanceId;
        public int Priority;
        public string ServerPath;
        public string SupplierID;
        public string SHA256;
        //public string SHA512;
        public string Thumbprint;
        public event PropertyChangedEventHandler PropertyChanged;

        public string NewVersion
        {
            get => _newVersion;
            set
            {
                _newVersion = value;
                OnPropertyChanged();
            }
        }

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine("");
            if (!string.IsNullOrEmpty(NewVersion))
            {
                sb.AppendLine("*******  Available Updates");

                sb.AppendLine("----------");
                sb.AppendLine($"UpdateType                : {UpdateType}");
                sb.AppendLine($"UpdateSeverity            : {UpdateSeverity}");
                sb.AppendLine($"New Version               : {NewVersion}");
                sb.AppendLine($"Description               : {Description}");
                sb.AppendLine("----------");

                sb.Append("******* End");
            }
            else
            {
                sb.AppendLine("*******  DPeM Already Updated");
            }
            return sb.ToString();
        }
    }
}