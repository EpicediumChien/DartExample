using Newtonsoft.Json;
using System.Text;


namespace DdmLibrary.Utility
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class DDMAppSettings
    {
        public double Version { get; set; } // ���Ȼݤj�� 0 �B�p�� 20�C�Y���ӼW���ܤj�� 20�A�h�ݧ�� isAnyIllegal
        

        [JsonConstructor]
        public DDMAppSettings()
        {
            Version = 1.0;
        }

        public static bool restoreDDMAppSettings(ref DDMAppSettings appSettings, string filePath)
        {
            byte[] content = default;

            try
            {
                content = File.ReadAllBytes(filePath);
            }
            catch
            {
                return false;
            }

            //Decrypted with AESKey in new setting file
            var key = Decryption.GetAESKeyFromKeyContainer();
            var decryptAES = Decryption.Decrypt(content, key);
            content = decryptAES;
            string JsonConetent = Encoding.UTF8.GetString(content, 0, content.Length);
            appSettings = JsonConvert.DeserializeObject<DDMAppSettings>(JsonConetent);

            return true;
        }
    }
}
