using System;
using System.Text;

namespace DDPM.SA.Common
{
    [Serializable]
    public class ClientInfo
    {
        public string status { get; set; }
        public string ServiceStatus { get; set; }
        public string ApiVersion { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new();

            sb.AppendLine("----------");
            sb.AppendLine($"{nameof(status)}        : {status}");
            sb.AppendLine($"{nameof(ServiceStatus)} : {ServiceStatus}");
            sb.AppendLine($"{nameof(ApiVersion)}    : {ApiVersion}");

            return sb.ToString();
        }
    }
}