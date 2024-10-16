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

      public static byte[][] certificateHash = {
          ThumbprintHash_CICD.DELL_Hash
      };
  }
}
