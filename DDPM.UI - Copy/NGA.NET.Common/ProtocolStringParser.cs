#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Dell.Client.Framework.Common;
using Microsoft;
using NGA.NET.Common.Helpers;
using System.Text;
using System.Text.Json;

namespace NGA.NET.Common
{
    /// <summary>
    /// Handles the parsing of the protocol string
    /// </summary>
    public static class ProtocolStringParser
    {
        /// <summary>
        /// Parses the protocol string looking for a <see cref="ShowPluginCommand"/>
        /// </summary>
        /// <param name="protocolString">The protocol string to parse. This can either be appURI:base64 or just base64</param>
        /// <param name="log">An optional log parameter</param>
        /// <returns>The commands found in the <paramref name="protocolString"/></returns>
        public static ShowPluginCommand ParseShowPluginCommand(string protocolString, ILog log = null)
        {
            if (string.IsNullOrWhiteSpace(protocolString))
            {
                log?.Trace($"{nameof(ParseShowPluginCommand)} - Empty {nameof(protocolString)}");
                return null;
            }

            var base64EncodedParam = protocolString;

            if (protocolString.Contains(':'))
            {
                var base64EncodedText = protocolString.Split(':');
                base64EncodedParam = base64EncodedText.Length > 0 ? base64EncodedText[1] : string.Empty;
            }

            if (string.IsNullOrEmpty(base64EncodedParam))
            {
                log?.Trace($"{nameof(ParseShowPluginCommand)} - Empty {nameof(base64EncodedParam)}");
                return null;
            }

            var root = GetRootFromEncodedString(base64EncodedParam, log);
            if (root == null)
            {
                log?.Trace($"{nameof(ParseShowPluginCommand)} - Invalid root version");
                return null;
            }

            var paramModel = GetParamModelFromRoot(root);
            if (paramModel == null)
            {
                log?.Trace($"{nameof(ParseShowPluginCommand)} - Invalid command version for ShowPlugin");
                return null;
            }

            var showPluginCommand = GetShowPluginCommand(paramModel);
            if (showPluginCommand == null)
            {
                log?.Trace($"{nameof(ParseShowPluginCommand)} - Invalid open Plugin Command");
                return null;
            }

            return showPluginCommand;
        }

        private static Root GetRootFromEncodedString(string base64EncodedParam, ILog log = null)
        {
            try
            {
                Requires.NotNullOrWhiteSpace(base64EncodedParam, nameof(base64EncodedParam));

                var rootObject = JsonSerializer.Deserialize(Base64Helper.Base64Decode(base64EncodedParam), typeof(Root));
                if (rootObject is Root { ModelVersion: Versions.ModelVersion1 } root
                    && root.ModelJson != null)
                {
                    return root;
                }
            }
            catch (FormatException ex)
            {
                log?.Error(ex, $"{nameof(GetRootFromEncodedString)} - {nameof(FormatException)}");
            }
            catch (JsonException ex)
            {
                log?.Error(ex, $"{nameof(GetRootFromEncodedString)} - {nameof(JsonException)}");
            }
            catch (NotSupportedException ex)
            {
                log?.Error(ex, $"{nameof(GetRootFromEncodedString)} - {nameof(NotSupportedException)}");
            }
            catch (ArgumentNullException ex)
            {
                log?.Error(ex, $"{nameof(GetRootFromEncodedString)} - {nameof(ArgumentNullException)}");
            }
            catch (DecoderFallbackException ex)
            {
                log?.Error(ex, $"{nameof(GetRootFromEncodedString)} - {nameof(DecoderFallbackException)}");
            }
            catch (ArgumentException ex)
            {
                log?.Error(ex, $"{nameof(GetRootFromEncodedString)} - {nameof(ArgumentException)}");
            }

            return null;
        }

        private static ParamModel GetParamModelFromRoot(Root root, ILog log = null)
        {
            try
            {
                if (root == null! || root.ModelJson == null)
                    return null;

                var paramModelObject = JsonSerializer.Deserialize(root.ModelJson, typeof(ParamModel));
                if (paramModelObject is ParamModel paramModel
                    && paramModel.CommandType == CommandType.ShowPlugin
                    && paramModel.CommandVersion is Versions.ShowPluginCommandVersion1 or Versions.ShowPluginCommandVersion2
                    && paramModel.CommandJson != null)
                {
                    return paramModel;
                }
            }
            catch (JsonException ex)
            {
                log?.Error(ex, $"{nameof(GetParamModelFromRoot)} - {nameof(JsonException)}");
            }
            catch (NotSupportedException ex)
            {
                log?.Error(ex, $"{nameof(GetParamModelFromRoot)} - {nameof(NotSupportedException)}");
            }
            catch (ArgumentNullException ex)
            {
                log?.Error(ex, $"{nameof(GetParamModelFromRoot)} - {nameof(ArgumentNullException)}");
            }

            return null;
        }

        private static ShowPluginCommand GetShowPluginCommand(ParamModel paramModel, ILog log = null)
        {
            try
            {
                if (paramModel == null! || paramModel.CommandJson == null)
                    return null;

                var openPluginCommandObject = JsonSerializer.Deserialize(paramModel.CommandJson, typeof(ShowPluginCommand));
                if (openPluginCommandObject is ShowPluginCommand openPluginCommand)
                {
                    return openPluginCommand;
                }
            }
            catch (JsonException ex)
            {
                log?.Error(ex, $"{nameof(GetParamModelFromRoot)} - {nameof(JsonException)}");
            }
            catch (NotSupportedException ex)
            {
                log?.Error(ex, $"{nameof(GetParamModelFromRoot)} - {nameof(NotSupportedException)}");
            }
            catch (ArgumentNullException ex)
            {
                log?.Error(ex, $"{nameof(GetParamModelFromRoot)} - {nameof(ArgumentNullException)}");
            }

            return null;
        }
    }
}