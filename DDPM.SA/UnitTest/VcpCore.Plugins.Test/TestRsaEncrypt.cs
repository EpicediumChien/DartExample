using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace VcpCore.Plugins.Test
{
    public class TestRsaEncrypt
    {
        [Test]
        public void TestGetKeyPair()
        {
            var keyPair = RsaEncrypt.GetKeyPair();
            Assert.IsNotNull(keyPair.Key);
            Assert.IsNotNull(keyPair.Value);
        }
        [Test]
        public void TestEncrypt()
        {
            string content = "Encryption key";
            //encryptKey 替换为有效的 RSA 密钥 XML 字符串
            string encryptKey = "<RSAKeyValue><Modulus>y/Xcxw5ZSkeWEnfIZU0WCyTTVXOUGachVWcJOvYKyHB9cbrhPRLUv9P2g8r8HcM/9UPSFbhbUt1l0Gd8IEJDZC+brD3cjaa5vKOfQSpsg08dRghP/52/erLNq7MvMnOzOcQWnyCg07/xDd4DnSmSr7Fx3BfvGo8iQ6ycB+OR9NE=</Modulus><Exponent>AQAB</Exponent><P>1dkAx6BHBN1V6zGM/tkgHIUjd8TTMVTa1cI1Tv3s60/jx9yVkmhsAyM52YTqyg53MLk/jZVMCvCv1i2Nzp4Ayw==</P><Q>9CnvwgQdbKbKkNjGC27oP+nhjVIJbJHDVwajsi6KJmeub3tQSOE4N0FIyz8GESxYkQY89SDH8nRfIreM56+5Uw==</Q><DP>o9a9bTSUFNsLL8Xn830gyBkNQn7PG9WaT/maZCnR8btklcSf5+sPDhxX/xqB1Ere8LqNQYloGF2tKlf+dJXDnw==</DP><DQ>JVG3nL8GRFImCgeoFZ1JEGPOHsyYNij9Y3LXWGe2o/Ia/l0pw0nxTrjCyJYEdmGB1ADRFmKBTTSuSd8mQU9hkw==</DQ><InverseQ>i+0GBq3G2NgKQOyDcdO/dh0uZBs6S3stmCiq9iwQdoc/fVyeNNOnwe0YiKNgVMaoJpFd1tpsZdwfmDMWXlt3Ew==</InverseQ><D>ZlojSOEyfcwey3XA4tUkUsNQKnmtwJHWcH0cbLI8BwosaX5WucdRbFJ6Svj6PBVXa0V1j+DMM3FXPpYv/CBEoIRLeSafaE6DcLGMU1Nd+zT9rTJo4d9yfHfkw7cBF/e7ygEfVOovLnHebZ91DIrkHkSzRrQjweAxCtmE2tD0hGE=</D></RSAKeyValue>";
            string encryptedContent = RsaEncrypt.Encrypt(content, encryptKey);
            Assert.IsNotNull(encryptedContent);
        }

        [Test]
        public void TestDecrypt()
        {
            string content = "Decryption key";
            //decryptKey 替换为有效的 RSA 密钥 XML 字符串
            string decryptKey = "<RSAKeyValue><Modulus>y/Xcxw5ZSkeWEnfIZU0WCyTTVXOUGachVWcJOvYKyHB9cbrhPRLUv9P2g8r8HcM/9UPSFbhbUt1l0Gd8IEJDZC+brD3cjaa5vKOfQSpsg08dRghP/52/erLNq7MvMnOzOcQWnyCg07/xDd4DnSmSr7Fx3BfvGo8iQ6ycB+OR9NE=</Modulus><Exponent>AQAB</Exponent><P>1dkAx6BHBN1V6zGM/tkgHIUjd8TTMVTa1cI1Tv3s60/jx9yVkmhsAyM52YTqyg53MLk/jZVMCvCv1i2Nzp4Ayw==</P><Q>9CnvwgQdbKbKkNjGC27oP+nhjVIJbJHDVwajsi6KJmeub3tQSOE4N0FIyz8GESxYkQY89SDH8nRfIreM56+5Uw==</Q><DP>o9a9bTSUFNsLL8Xn830gyBkNQn7PG9WaT/maZCnR8btklcSf5+sPDhxX/xqB1Ere8LqNQYloGF2tKlf+dJXDnw==</DP><DQ>JVG3nL8GRFImCgeoFZ1JEGPOHsyYNij9Y3LXWGe2o/Ia/l0pw0nxTrjCyJYEdmGB1ADRFmKBTTSuSd8mQU9hkw==</DQ><InverseQ>i+0GBq3G2NgKQOyDcdO/dh0uZBs6S3stmCiq9iwQdoc/fVyeNNOnwe0YiKNgVMaoJpFd1tpsZdwfmDMWXlt3Ew==</InverseQ><D>ZlojSOEyfcwey3XA4tUkUsNQKnmtwJHWcH0cbLI8BwosaX5WucdRbFJ6Svj6PBVXa0V1j+DMM3FXPpYv/CBEoIRLeSafaE6DcLGMU1Nd+zT9rTJo4d9yfHfkw7cBF/e7ygEfVOovLnHebZ91DIrkHkSzRrQjweAxCtmE2tD0hGE=</D></RSAKeyValue>";
            string decryptedContent = RsaEncrypt.Encrypt(content, decryptKey);
            Assert.IsNotNull(decryptedContent);
        }

    }
}
