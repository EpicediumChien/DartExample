namespace DDPM.SA.Obfuscation
{
    public class ThumbprintHash_CICD
    {
        private static readonly byte[] DELL_Hash = new byte[]
        {
          //841C87C9F5A679DCDBA8A9C7F743847D157CD598
          0x84, 0x1C, 0x87, 0xC9, 0xF5, 0xA6, 0x79, 0xDC, 0xDB, 0xA8,
          0xA9, 0xC7, 0xF7, 0x43, 0x84, 0x7D, 0x15, 0x7C, 0xD5, 0x98
        };

        private static readonly byte[] WST_Hash = new byte[]
        {
            //Wistron1 Thumbprint : 0c 38 4f 7d 61 27 68 94 6c 06 38 76 b7 42 85 95 61 56 c6 a1
            // 0c 38 4f 7d 61 27 68 94 6c 06
            // 38 76 b7 42 85 95 61 56 c6 a1
            0x0c, 0x38, 0x4f, 0x7d, 0x61, 0x27, 0x68, 0x94, 0x6c, 0x06,
            0x38, 0x76, 0xb7, 0x42, 0x85, 0x95, 0x61, 0x56, 0xc6, 0xa1
        };

        private static readonly byte[] WST2_Hash = new byte[]
        {
            //Wistron2 Thumbprint : 841c87c9f5a679dcdba8a9c7f743847d157cd598                                    
            //84 1c 87 c9 f5 a6 79 dc db a8
            //a9 c7 f7 43 84 7d 15 7c d5 98
            0x84, 0x1c, 0x87, 0xc9, 0xf5, 0xa6, 0x79, 0xdc, 0xdb, 0xa8,
            0xa9, 0xc7, 0xf7, 0x43, 0x84, 0x7d, 0x15, 0x7c, 0xd5, 0x98
        };

        public static readonly byte[][] certificateHash = {
            WST_Hash,
            WST2_Hash,
            DELL_Hash
        };
    }
}
