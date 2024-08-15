using DPeMPublic.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DDPM.SA.Common
{
    [Serializable]
    public class DongleInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Guid ID { get; set; }

        public DeviceType DeviceType { get; set; }
        public bool IsMultipleDongleFound { get; set; }
        public int PairedDeviceCount { get; set; }
        public int MaxPairingSlots { get; set; }
        public List<Guid> LogicalDeviceIDs { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}