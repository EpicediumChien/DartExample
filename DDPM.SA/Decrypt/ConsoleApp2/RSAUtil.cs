using System.Security.Cryptography;
using System.Text;


namespace DdmLibrary.Utility.Securiry
{
    public class RSAUtil
    {
        private static string IdentityGuid   = "{85210b5f-2dc6-42f4-b89e-617dd1eec2e1}";
        public static string RSA_KEY_TYPE   = "ssh-rsa";
        public static int    RSA_KEY_LENGTH = 4096;

        public RSAUtil()
        {
        }

        private string FullContainerName(string keyGuid)
        {
            //Logger.LogLine(LogLevel.Debug, string.Format("{0} {1} IN, keyGuid={2}", this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name, keyGuid));

            var name = new StringBuilder();
            name.Append(keyGuid);
            return name.ToString();
        }

        public byte [] RequestPublicKey(bool useMachineLevel)
        {
            return RequestPublicKey(useMachineLevel, RSA_KEY_TYPE, RSA_KEY_LENGTH, FullContainerName(IdentityGuid));
        }

        public byte [] RequestPublicKey(bool useMachineLevel, string type, int bits, string containerName)
        {
            byte [] key = null;
            CspParameters parameters = null;

            try
            {
                if (type.Contains("rsa", StringComparison.OrdinalIgnoreCase))
                {
                    RSACryptoServiceProvider.UseMachineKeyStore = useMachineLevel;

                    parameters = new CspParameters
                    {
                        ProviderType     = 1, /* PROV_RSA_FULL */
                        KeyContainerName = containerName,
                    };

                    if (useMachineLevel)
                    {
                        parameters.Flags |= CspProviderFlags.UseMachineKeyStore | CspProviderFlags.UseNonExportableKey;
                    }

                    using (RSACryptoServiceProvider csp = new(bits, parameters))
                    {
                        key = csp.ExportRSAPublicKey();
                    }
                }
            }
            catch (Exception e)
            {          
            }

            return key;
        }
    }
}