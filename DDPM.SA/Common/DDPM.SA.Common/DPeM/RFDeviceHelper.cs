using System;
using System.Collections.Generic;
using System.Text;

namespace DDPM.SA.Common
{
    [Serializable]
    public class RFDeviceHelper
    {
        public List<DongleInfo> dongleInfo { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            int index = 1;
            foreach (var item in dongleInfo)
            {
                PrintDeciveName(sb, item, index++);
            }
            return sb.ToString();
        }

        private static void PrintDeciveName(StringBuilder sb, DongleInfo item, int index)
        {
            sb.AppendLine($"{index}.----------");
            sb.AppendLine($"{nameof(item.ID)}                         : {item.ID}");
            sb.AppendLine($"{nameof(item.DeviceType)}                 : {item.DeviceType}");
            sb.AppendLine("----------");
        }

        public string PrintRFDeviceInfo(int index)
        {
            StringBuilder sb = new StringBuilder();

            if (index > 0 && dongleInfo.Count >= index)
            {
                index--;
                var rfInfo = dongleInfo[index];
                if (rfInfo.IsMultipleDongleFound)
                {
                    sb.AppendLine("Multiple dongle detected");
                    sb.AppendLine("Please unplug all Dell Universal dongles and plug in the one you would like to pair.");
                }
                else if (rfInfo.MaxPairingSlots <= rfInfo.PairedDeviceCount)
                {
                    sb.AppendLine("All Slots ocupied please unpair any device then try again.");
                }
                else
                {
                    sb.AppendLine($"{nameof(rfInfo.ID)}                                   : {rfInfo.ID.ToString()}");
                    sb.AppendLine($"{nameof(rfInfo.DeviceType)}                              : {rfInfo.DeviceType.ToString()}");
                    sb.AppendLine($"Type the command with parameters Example : CommandName:parameter1,parameter2...");
                    sb.AppendLine($"StartPairing:ID");
                    sb.AppendLine($"StopPairing:ID");
                }
            }

            return sb.ToString();
        }
    }
}