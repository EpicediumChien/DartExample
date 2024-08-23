using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Method
{
    public static class Algorithm
    {
        public static string ReverseString(string s)
        {
            char[] array = s.ToCharArray();
            Array.Reverse(array);
            return new string(array);
        }
        public static string BinaryToHex(string binaryString)
        {
            int length = binaryString.Length;
            int padding = (4 - (length % 4)) % 4;
            binaryString = new string('0', padding) + binaryString;

            string hexString = "";
            for (int i = 0; i < binaryString.Length; i += 4)
            {
                string fourBits = binaryString.Substring(i, 4);
                int hexValue = Convert.ToInt32(fourBits, 2);
                hexString += hexValue.ToString("X");
            }
            return hexString;
        }
    }
}
