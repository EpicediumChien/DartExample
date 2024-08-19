using System;


namespace DDPM.Common
{
    public class ThumbprintHash
    {

#if RELEASE
        /* Default 
         99933f486c93fa39ea1f6b9d0d5c95a8c50511e9
ed23ea1e2b1d71e21e921d97de8ea5cb564e0ec0
21dfcd954923696a5548e851ff256370956b09e0
         */
        public static readonly byte[] DELL_Hash = new byte[]
        {
            //99933f486c93fa39ea1f6b9d0d5c95a8c50511e9
            //99 93 3f 48 6c 93 fa 39 ea 1f
            //6b 9d 0d 5c 95 a8 c5 05 11 e9
            0x99, 0x93, 0x3f, 0x48, 0x6c, 0x93, 0xfa, 0x39, 0xea, 0x1f, 
            0x6b, 0x9d, 0x0d, 0x5c, 0x95, 0xa8, 0xc5, 0x05, 0x11, 0xe9
        };

        public static readonly byte[] DELL_Hash1 = new byte[]
        {
            //ed23ea1e2b1d71e21e921d97de8ea5cb564e0ec0
            //ed 23 ea 1e 2b 1d 71 e2 1e 92
            //1d 97 de 8e a5 cb 56 4e 0e c0
            0xed, 0x23, 0xea, 0x1e, 0x2b, 0x1d, 0x71, 0xe2, 0x1e, 0x92, 
            0x1d, 0x97, 0xde, 0x8e, 0xa5, 0xcb, 0x56, 0x4e, 0x0e, 0xc0
        };

        public static readonly byte[] DELL_Hash2 = new byte[]
        {
            //21dfcd954923696a5548e851ff256370956b09e0
            //21 df cd 95 49 23 69 6a 55 48
            //e8 51 ff 25 63 70 95 6b 09 e0
            0x21, 0xdf, 0xcd, 0x95, 0x49, 0x23, 0x69, 0x6a, 0x55, 0x48, 
            0xe8, 0x51, 0xff, 0x25, 0x63, 0x70, 0x95, 0x6b, 0x09, 0xe0
        };

        public static readonly byte[] WST_Hash = new byte[]
        {
            //Wistron1 Thumbprint : 0c 38 4f 7d 61 27 68 94 6c 06 38 76 b7 42 85 95 61 56 c6 a1
            // 0c 38 4f 7d 61 27 68 94 6c 06
            // 38 76 b7 42 85 95 61 56 c6 a1
            0x0c, 0x38, 0x4f, 0x7d, 0x61, 0x27, 0x68, 0x94, 0x6c, 0x06, 
            0x38, 0x76, 0xb7, 0x42, 0x85, 0x95, 0x61, 0x56, 0xc6, 0xa1
        };

        public static readonly byte[] WST2_Hash = new byte[]
        {

            //Wistron2 Thumbprint : 841c87c9f5a679dcdba8a9c7f743847d157cd598
            //84 1c 87 c9 f5 a6 79 dc db a8
            //a9 c7 f7 43 84 7d 15 7c d5 98
            0x84, 0x1c, 0x87, 0xc9, 0xf5, 0xa6, 0x79, 0xdc, 0xdb, 0xa8,
            0xa9, 0xc7, 0xf7, 0x43, 0x84, 0x7d, 0x15, 0x7c, 0xd5, 0x98
        };
#endif
    }
}