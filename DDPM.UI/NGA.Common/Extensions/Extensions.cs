#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using System.Linq;
using Microsoft;

namespace NGA.Common.Extensions
{
    /// <summary>
    /// StringExtension class
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Generic extension to verify whether the source value exists in the supplied list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="list"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns><see cref="bool"/></returns>
        public static bool In<T>(this T source, params T[] list)
        {
            if (list == null || list.Length == 0)
                throw new ArgumentNullException(nameof(list));

            return list.Contains(source);
        }

        /// <summary>
        /// String extension to validate whether it contains any text
        /// It throws <see cref="ArgumentNullException"/> if the string value is null or whitespace
        /// otherwise it would return the string value as is 
        /// </summary>
        /// <param name="inputStr"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string RequiresNotNullOrWhiteSpace(this string? inputStr, string? parameterName)
        {
            Requires.NotNullOrWhiteSpace(inputStr!, parameterName);
            return inputStr;
        }

        /// <summary>
        /// Generic extension to check if it is not null
        /// It throws <see cref="ArgumentNullException"/> if null otherwise returns the value as is
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T RequiresNotNull<T>(this T? value, string? parameterName) where T : class
        {
            Requires.NotNull(value!, parameterName);
            return value;
        }
    }
}