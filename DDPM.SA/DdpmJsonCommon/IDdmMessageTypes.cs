using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DdpmJsonCommon
{
    public interface IDDM_MESSAGE
    {
        /// <summary>
        /// 說明這個 command 的 type, 以下列的 class name 為名稱
        /// </summary>
        string type { get; }

        byte[] Checksum { get; set; }

        /// <summary>
        /// Helper method to convert this object to json string
        /// </summary>
        /// <returns></returns>
        string ToJson();

        /// <summary>
        /// Calculate checksum of this object
        /// </summary>
        /// <returns></returns>
        byte[] CalculateChecksum();

        /// <summary>
        /// Calculate checksum of this object and update Checksum property
        /// </summary>
        void UpdateChecksum();

        /// <summary>
        /// Is Checksum equal to calculated checksum from CalculateChecksum
        /// </summary>
        /// <returns></returns>
        bool IsChecksumValid();
    }

    public interface IDDM_COMMAND : IDDM_MESSAGE
    {
        /// <summary>
        /// correlation id.
        /// command 和 response 有 id, 表示是否為同一對 command/response.
        /// ex: NKVM 送出 command 的 id 是 426, 那 DDPM 回應此 command 的 response 的 id 也是 426
        /// </summary>
        int cid { get; set; }
    }

    public interface IDDM_RESPONSE : IDDM_MESSAGE
    {
        /// <summary>
        /// correlation id.
        /// command 和 response 有 id, 表示是否為同一對 command/response.
        /// ex: NKVM 送出 command 的 id 是 426, 那 DDPM 回應此 command 的 response 的 id 也是 426
        /// </summary>
        int cid { get; set; }

        /// <summary>
        /// 表示執行指令有無成功
        /// </summary>
        bool Success { get; }
    }

    public interface IDDM_EVENT : IDDM_MESSAGE
    {
    }

    // Base class implementing IDDM_MESSAGE
    public abstract class DDM_MESSAGE : IDDM_MESSAGE
    {
        public string type => GetType().Name;
        public byte[] Checksum { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public byte[] CalculateChecksum()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new ChecksumContractResolver()
            };
            var json = JsonConvert.SerializeObject(this, settings);

            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
            }
        }

        public void UpdateChecksum()
        {
            Checksum = CalculateChecksum();
        }

        public bool IsChecksumValid()
        {
            var currentChecksum = CalculateChecksum();
            return Checksum?.SequenceEqual(currentChecksum) ?? false;
        }

        // Custom contract resolver to ignore the Checksum property during serialization
        private class ChecksumContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                var props = base.CreateProperties(type, memberSerialization);
                return props.Where(p => p.PropertyName != "Checksum").ToList();
            }
        }
    }

    public class DDM_COMMAND : DDM_MESSAGE, IDDM_COMMAND
    {
        public int cid { get; set; }

        public DDM_COMMAND()
        {
            cid = Constants.INVALID_COMMAND_CID;
        }
    }

    public class DDM_RESPONSE : DDM_MESSAGE, IDDM_RESPONSE
    {
        public int cid { get; set; }
        public bool Success { get; set; }

        public DDM_RESPONSE()
        {
            cid = Constants.INVALID_COMMAND_CID;
            Success = false;
        }
    }

    public class DDM_EVENT : DDM_MESSAGE, IDDM_EVENT
    {
    }

}
