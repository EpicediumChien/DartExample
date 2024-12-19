namespace DDPM.SA.Obfuscation
{
    public class ThumbprintHash_NKVM
    {
        private static readonly byte[] QDA1_Hash = new byte[]
        {
            //Qisda NKVM Thumbprint : ca7356d58f31d1a4f1e94c37e73c5bd2f894d5ac
            0xca, 0x73, 0x56, 0xd5, 0x8f, 0x31, 0xd1, 0xa4, 0xf1, 0xe9,
            0x4c, 0x37, 0xe7, 0x3c, 0x5b, 0xd2, 0xf8, 0x94, 0xd5, 0xac
        };

        private static readonly byte[] QDA2_Hash = new byte[]
        {
            //Qisda NKVM Thumbprint : 4c1234547c6d0ab6f84a17e9153e1312e31b5f33
            0x4c, 0x12, 0x34, 0x54, 0x7c, 0x6d, 0x0a, 0xb6, 0xf8, 0x4a,
            0x17, 0xe9, 0x15, 0x3e, 0x13, 0x12, 0xe3, 0x1b, 0x5f, 0x33
        };

        private static readonly byte[] QDA3_Hash = new byte[] //2024/12/18 add
        {
            //Qisda NKVM Thumbprint : 83533ef29bd2360a38b0c7cee32d71ac55be3eb7
            0x83, 0x53, 0x3e, 0xf2, 0x9b, 0xd2, 0x36, 0x0a, 0x38, 0xb0,
            0xc7, 0xce, 0xe3, 0x2d, 0x71, 0xac, 0x55, 0xbe, 0x3e, 0xb7
        };

        public static readonly byte[][] certificateHash = {
            ThumbprintHash_NKVM.QDA1_Hash,
            ThumbprintHash_NKVM.QDA2_Hash,
            ThumbprintHash_NKVM.QDA3_Hash
        };
    }
}