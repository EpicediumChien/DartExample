#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using Dell.Client.Framework.Common.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace NGA.Common.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Object"/>
    /// </summary>
    public static class ObjectExtension
    {
        /// <summary>
        /// This method returns hash-map, by default it sets the <see cref="Object"/> property name as key
        /// If the DisplayName attribute is set on property then it sets the key
        /// </summary>
        /// <param name="source">Reference of the object passed to this method</param>
        /// <param name="bindingAttr">Binding flags</param>
        /// <returns><see cref="Dictionary{TKey,TValue}"/> Object</returns>
        /// <exception cref="ArgumentException">This exception is thrown when there is duplicate display name or property name matching display name</exception>
        public static Dictionary<string, object?> AsDictionary(this object source,
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            Dictionary<string, object?> dictionary = new();
            foreach (var property in source.GetType().GetProperties(bindingAttr))
            {
                // Default the key to property name
                var key = property.Name;

                // Read the display name attribute set on the property
                var displayNameAttribute = property.GetAttribute<DisplayNameAttribute>();

                // If the display name attribute is set then set the key to DisplayName
                // Also check to see DisplayName contains some text
                if (displayNameAttribute != null && !string.IsNullOrWhiteSpace(displayNameAttribute.DisplayName))
                    key = displayNameAttribute.DisplayName;

                var propertyValue = property.GetValue(source, null);
                dictionary.Add(key, propertyValue);
            }

            return dictionary;
        }
    }
}