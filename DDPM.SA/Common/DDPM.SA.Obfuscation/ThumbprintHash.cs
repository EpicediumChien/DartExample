namespace DDPM.SA.Obfuscation
{
    public class ThumbprintHash
    {
        /* Default
            99933f486c93fa39ea1f6b9d0d5c95a8c50511e9
            ed23ea1e2b1d71e21e921d97de8ea5cb564e0ec0
            21dfcd954923696a5548e851ff256370956b09e0
         */

        private static readonly byte[] DELL_Hash = new byte[]
        {
            //99933f486c93fa39ea1f6b9d0d5c95a8c50511e9
            //99 93 3f 48 6c 93 fa 39 ea 1f
            //6b 9d 0d 5c 95 a8 c5 05 11 e9
            0x99, 0x93, 0x3f, 0x48, 0x6c, 0x93, 0xfa, 0x39, 0xea, 0x1f,
            0x6b, 0x9d, 0x0d, 0x5c, 0x95, 0xa8, 0xc5, 0x05, 0x11, 0xe9
        };

        private static readonly byte[] DELL_Hash1 = new byte[]
        {
            //ed23ea1e2b1d71e21e921d97de8ea5cb564e0ec0
            //ed 23 ea 1e 2b 1d 71 e2 1e 92
            //1d 97 de 8e a5 cb 56 4e 0e c0
            0xed, 0x23, 0xea, 0x1e, 0x2b, 0x1d, 0x71, 0xe2, 0x1e, 0x92,
            0x1d, 0x97, 0xde, 0x8e, 0xa5, 0xcb, 0x56, 0x4e, 0x0e, 0xc0
        };

        private static readonly byte[] DELL_Hash2 = new byte[]
        {
            //21dfcd954923696a5548e851ff256370956b09e0
            //21 df cd 95 49 23 69 6a 55 48
            //e8 51 ff 25 63 70 95 6b 09 e0
            0x21, 0xdf, 0xcd, 0x95, 0x49, 0x23, 0x69, 0x6a, 0x55, 0x48,
            0xe8, 0x51, 0xff, 0x25, 0x63, 0x70, 0x95, 0x6b, 0x09, 0xe0
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

        private static readonly byte[] QDA1_Hash = new byte[]
        {
            //Qisda NKVM Thumbprint : ca7356d58f31d1a4f1e94c37e73c5bd2f894d5ac
            //ca 73 56 d5 8f 31 d1 a4 f1 e9
            //4c 37 e7 3c 5b d2 f8 94 d5 ac
            0xca, 0x73, 0x56, 0xd5, 0x8f, 0x31, 0xd1, 0xa4, 0xf1, 0xe9,
            0x4c, 0x37, 0xe7, 0x3c, 0x5b, 0xd2, 0xf8, 0x94, 0xd5, 0xac
        };

        private static readonly byte[] QDA2_Hash = new byte[]
        {
            //Qisda NKVM Thumbprint : 4c1234547c6d0ab6f84a17e9153e1312e31b5f33
            //4c 12 34 54 7c 6d 0a b6 f8 4a
            //17 e9 15 3e 13 12 e3 1b 5f 33
            0x4c, 0x12, 0x34, 0x54, 0x7c, 0x6d, 0x0a, 0xb6, 0xf8, 0x4a,
            0x17, 0xe9, 0x15, 0x3e, 0x13, 0x12, 0xe3, 0x1b, 0x5f, 0x33
        };


        //d3 11 37 9e ee 0e 4a 4a d0 b1 39 43 a2 39 06 72 54 92 dd 29
        // Dell CI/CD cer thumbprint.
        private static readonly byte[] DELL_CICD_Hash = new byte[]
        {
            //d311379eee0e4a4ad0b13943a23906725492dd29
            0xd3, 0x11, 0x37, 0x9e, 0xee, 0x0e, 0x4a, 0x4a, 0xd0, 0xb1,
            0x39, 0x43, 0xa2, 0x39, 0x06, 0x72, 0x54, 0x92, 0xdd, 0x29
        };

        public static readonly byte[][] certificateHash = {
            ThumbprintHash.DELL_Hash,
            ThumbprintHash.DELL_Hash1,
            ThumbprintHash.DELL_Hash2,
            ThumbprintHash.WST_Hash,
            ThumbprintHash.WST2_Hash,
            ThumbprintHash.QDA1_Hash,
            ThumbprintHash.QDA2_Hash,
            ThumbprintHash.DELL_CICD_Hash
        };
    }
}