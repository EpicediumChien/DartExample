using DdmLibrary.Utility.Securiry;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;


namespace DdmLibrary
{
    public static class Decryption
    {
        public static byte[] oldAESKey = Convert.FromBase64String("jalEEwY4hF0xyjrJOV5b9C3q8rO8rtuzJJdmtxM6LsI=");

        public static string Decrypt(string cipher, byte [] key = null)
        {
            // Decode
            Span<byte> encryptedData = Convert.FromBase64String(cipher).AsSpan();

            // Extract parameter sizes
            int nonceSize = BinaryPrimitives.ReadInt32LittleEndian(encryptedData.Slice(0, 4));
            int tagSize = BinaryPrimitives.ReadInt32LittleEndian(encryptedData.Slice(4 + nonceSize, 4));
            int cipherSize = encryptedData.Length - 4 - nonceSize - 4 - tagSize;

            // Extract parameters
            var nonce = encryptedData.Slice(4, nonceSize);
            var tag = encryptedData.Slice(4 + nonceSize + 4, tagSize);
            var cipherBytes = encryptedData.Slice(4 + nonceSize + 4 + tagSize, cipherSize);

            // Decrypt
            Span<byte> plainBytes = cipherSize < 1024
                                  ? stackalloc byte[cipherSize]
                                  : new byte[cipherSize];

            if (key == null)
            {
                key = oldAESKey;
            }
            using var aes = new AesGcm(key);
            aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

            // Convert plain bytes back into string
            return Encoding.UTF8.GetString(plainBytes);
        }

        public static byte [] Decrypt(byte [] cipher, byte [] key = null)
        {
            // Decode
            Span<byte> encryptedData = new(cipher);

            // Extract parameter sizes
            int nonceSize = BinaryPrimitives.ReadInt32LittleEndian(encryptedData.Slice(0, 4));
            int tagSize = BinaryPrimitives.ReadInt32LittleEndian(encryptedData.Slice(4 + nonceSize, 4));
            int cipherSize = encryptedData.Length - 4 - nonceSize - 4 - tagSize;

            // Extract parameters
            var nonce = encryptedData.Slice(4, nonceSize);
            var tag = encryptedData.Slice(4 + nonceSize + 4, tagSize);
            var cipherBytes = encryptedData.Slice(4 + nonceSize + 4 + tagSize, cipherSize);

            // Decrypt
            Span<byte> plainBytes = cipherSize < 1024
                                  ? stackalloc byte[cipherSize]
                                  : new byte[cipherSize];

            if (key == null)
            {
                key = oldAESKey;
            }
            using var aes = new AesGcm(key);
            aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

            // Convert plain bytes back into string
            //return Encoding.UTF8.GetString(plainBytes);

            byte [] b = new byte[plainBytes.Length];
            plainBytes.CopyTo(b);

            return b;
        }       

        public static byte [] GetAESKeyFromKeyContainer()
        {
            byte [] key = Convert.FromBase64String("sEknapg5hctYjbJa6Se6Fz9Mcjl0FIj4ogBG3X0ROeKX"); // A fake key

            var k = new RSAUtil().RequestPublicKey(true);
            if (k != null)
            {
                key = k;
            }

            k = new byte[oldAESKey.Length];
            Buffer.BlockCopy(key, 0, k, 0, oldAESKey.Length);

            return k;
        }
    }
}
