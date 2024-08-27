using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace DDPM.SA.Common
{
    [Serializable]
    public class UpdateHelper
    {
        public List<UpdateItemInfo> UpdateItems;

        public override string ToString()
        {
            if (UpdateItems != null && UpdateItems.Count > 0)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("SrNo");
                dt.Columns.Add("UpdateType");
                dt.Columns.Add("DeviceName");
                dt.Columns.Add("NewVersion");
                int index = 0;

                foreach (UpdateItemInfo item in UpdateItems)
                {
                    DataRow drr = dt.NewRow();
                    drr["SrNo"] = ++index;
                    drr["UpdateType"] = item.UpdateType;
                    drr["DeviceName"] = item.DeviceName;
                    if (item.NewVersion.Contains("."))
                    {
                        drr["NewVersion"] = item.NewVersion.ToString();
                    }
                    else
                    {
                        if (item.NewVersion.Length >= 4)
                        {
                            drr["NewVersion"] = Regex.Replace(item.NewVersion.ToString(), ".{1}", "$0.").Substring(0, (item.NewVersion.ToString().Length * 2) - 1);
                        }
                        else
                        {
                            string version = item.NewVersion.ToString().PadLeft(4, '0');
                            drr["NewVersion"] = Regex.Replace(version.ToString(), ".{1}", "$0.").Substring(0, (version.ToString().Length * 2) - 1);

                        }
                        //int Version = int.Parse(item.NewVersion, System.Globalization.NumberStyles.HexNumber);
                    }
                    dt.Rows.Add(drr);
                }
                List<string> columns = new List<string>();
                foreach (DataColumn Dc in dt.Columns)
                {
                    columns.Add(Dc.ColumnName.ToString());
                }
                dt.Print();
                return "";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"There are no available updates at this time.");
                return sb.ToString();
            }
        }

        public string PrintFWUpdateInfo(int index)
        {
            StringBuilder sb = new StringBuilder();

            if (index > 0)
            {
                UpdateItemInfo item = UpdateItems[index - 1];

                sb.AppendLine($"{nameof(item.DeviceName)} (get)                          : {item.DeviceName.ToString()}");
                sb.AppendLine($"{nameof(item.DeviceModelNumber)} (get)                   : {item.DeviceModelNumber}");
                sb.AppendLine($"{nameof(item.UpdateType)} (get)                          : {item.UpdateType.ToString()}");
                sb.AppendLine($"{nameof(item.UpdateSeverity)}  (get)                     : {item.UpdateSeverity}");
                if (item.NewVersion.Contains("."))
                {
                    sb.AppendLine($"{nameof(item.NewVersion)} (get)                          : {item.NewVersion}");
                }
                else
                {
                    sb.AppendLine($"{nameof(item.NewVersion)} (get)                          : {Regex.Replace(Convert.ToInt32(item.NewVersion).ToString("D4"), ".{1}", "$0.").Substring(0, (Convert.ToInt32(item.NewVersion).ToString("D4").Length * 2) - 1)}");
                }
                sb.AppendLine($"{nameof(item.Description)}  (get)                        : {item.Description}");
                if (item.CurrentVersion.Contains("."))
                {
                    sb.AppendLine($"{nameof(item.CurrentVersion)} (get)                      : {item.CurrentVersion}");
                }
                else
                {
                    sb.AppendLine($"{nameof(item.CurrentVersion)} (get)                      : {Regex.Replace(Convert.ToInt32(item.CurrentVersion).ToString("D4"), ".{1}", "$0.").Substring(0, (Convert.ToInt32(item.CurrentVersion).ToString("D4").Length * 2) - 1)}");
                }

                sb.AppendLine($"{nameof(item.DeviceId)} (get)                            : {item.DeviceId}");
                sb.AppendLine($"{nameof(item.DeviceIndex)} (get)                         : {item.DeviceIndex}");
                sb.AppendLine($"{nameof(item.DevicePath)}  (get)                         : {item.DevicePath}");
                sb.AppendLine($"{nameof(item.DeviceType)} (get)                          : {item.DeviceType.ToString()}");
                sb.AppendLine($"{nameof(item.FrimwareUpdatePath)} (get)                  : {item.FrimwareUpdatePath.ToString()}");
                sb.AppendLine($"{nameof(item.InstallPath)}  (get)                        : {item.InstallPath}");
                sb.AppendLine($"{nameof(item.InstanceId)} (get)                          : {item.InstanceId}");
                sb.AppendLine($"{nameof(item.Priority)}  (get)                           : {item.Priority}");
                sb.AppendLine($"{nameof(item.ServerPath)} (get)                          : {item.ServerPath}");
                sb.AppendLine($"{nameof(item.SupplierID)} (get)                          : {item.SupplierID}");
            }

            return sb.ToString();
        }

        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
    }
}