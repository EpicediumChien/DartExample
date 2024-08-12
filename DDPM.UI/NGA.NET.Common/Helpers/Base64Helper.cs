#region LicenseHeader
//
// ©Copyright 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using System.Text;

namespace NGA.NET.Common.Helpers
{
    /// <summary>
    /// Base64Helper provides string to Base64 encoding and decoding APIs
    /// </summary>
    internal static class Base64Helper
    {
        /// <summary>
        /// Encodes string text to Base64
        /// </summary>
        /// <param name="plainText">string text</param>
        /// <returns>Base64 encoded string</returns>
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Decodes from Base64 to string text
        /// </summary>
        /// <param name="value">Base64 encoded text</param>
        /// <returns>Base64 decoded string text</returns>
        /// <exception cref="FormatException"><paramref name="value"/> is an invalid format.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null or <paramref name="value"/> decoded to a null byte[]</exception>
        /// <exception cref="DecoderFallbackException"></exception>
        /// <exception cref="ArgumentException"><paramref name="value"/> caused an argument exception when converting to UTF8</exception>
        public static string Base64Decode(string value)
        {
            var valueBytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(valueBytes);
        }
    }
}
