#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Exceptions;
using Dell.Client.Framework.Security;
using Dell.Client.Framework.Security.Interfaces;
using Microsoft;
using System.Xml.Linq;

namespace NGA.Common.Helpers
{
    /// <summary>
    /// TelemetryFileTracker class
    /// </summary>
    public static class TelemetryFileTracker
    {
        private const string UserChoicesFile = "userchoices.xml";

        private const string AdditionPath = "oobe\\info\\";
        private const string CustomerInfoNode = "customerinfo";
        private const string ValueNode = "value";
        private const string CsupFileName = "csup.txt";
        private const string DataFormat = "MM-dd-yyyy";
        private static readonly DateTime ReferenceDate = new(2021, 11, 24);
        private static Log _log;
        /// <summary>
        /// Parse system files to identify user consent for telemetry preferences.
        /// </summary>
        /// <returns> return TelemetryParseResponse Enum</returns>
        /// Returns TelemetryParseResponse.Consent when all 3 condition satisfied
        /// 1. Userchoices.xml file exist,
        /// 2. Value = true in customerInfo node
        /// 3. Date >= 11-24-2021 in csup.txt
        /// Return TelemetryParseResponse.Unknown in following scenarios
        /// 1. if any of he files are missing
        /// 2. if any exception occurs
        /// 3. if reference date in Csup.txt is older
        /// Returns TelemetryParseResponse.NoConsent when
        /// 1. In Userchoices.xml file, CustomerInfo node value is set False and Csup files has newer data
        public static TelemetryParseResponse ParseTelemetryFiles(ILog log, IFilesystem? fileSystem = null)
        {
            log.Trace($"{nameof(ParseTelemetryFiles)} entry");

            Requires.NotNull(log, nameof(log));
            fileSystem = fileSystem ?? Filesystem.Instance;

            var response = TelemetryParseResponse.Unknown;

            try
            {
                var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
                var folderPath = Path.Combine(systemPath, AdditionPath);
                var filePath = Path.Combine(folderPath, UserChoicesFile);

                //check file exists
                if (!fileSystem.FileExists(filePath))
                {
                    log.Info($"{nameof(ParseTelemetryFiles)} {UserChoicesFile} not found");
                    return response;
                }

                //0905 Elsa Add Security
                string FileInfo;
                if (!DDPMFileSecurity.IsFilePathValid(filePath, out FileInfo))
                {
                    _log.Info($"{nameof(ParseTelemetryFiles)} {FileInfo}");
                    return response;
                }

                string xmlText = ReadFile(log, fileSystem, filePath);
                var xFile = XDocument.Parse(xmlText);

                //parse the file and get the Value node
                var result = xFile.Descendants(CustomerInfoNode)
                    .Select(x => x.Element(ValueNode)?.Value).First();

                if (string.IsNullOrWhiteSpace(result))
                {
                    log.Info($"{nameof(ParseTelemetryFiles)} {CustomerInfoNode} node not found");
                    return response;
                }

                var customerValue = bool.Parse(result);

                var csupResponse = ParseCsupFile(log, fileSystem);

                if (!csupResponse)
                {
                    response = TelemetryParseResponse.Unknown;
                }
                else
                {
                    response = customerValue ? TelemetryParseResponse.Consent : TelemetryParseResponse.NoConsent;
                }
            }
            catch (Exception ex)
            {
                response = TelemetryParseResponse.Unknown;
                var message = $"Exception occurred in {nameof(ParseTelemetryFiles)}" + ex.Message;
                log?.Error(ex, message);
            }

            return response;
        }

        private static bool ParseCsupFile(ILog log, IFilesystem fileSystem)
        {
            //Check csup file
            var winPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            var csupFile = Path.Combine(winPath, CsupFileName);

            if (!fileSystem.FileExists(csupFile))
            {
                log.Info($"{nameof(ParseTelemetryFiles)} {CsupFileName} not found");
                return false;
            }

            var text = ReadFile(log, fileSystem, csupFile);

            var date = DateTime.ParseExact(text.Trim(), DataFormat,
                System.Globalization.CultureInfo.InvariantCulture);

            var response = date.Date.CompareTo(ReferenceDate.Date) > 0;

            return response;
        }

        private static string ReadFile(ILog log, IFilesystem fileSystem, string file)
        {
            log.Trace($"{nameof(ReadFile)} {file} entry");

            var result = PathHelper.ValidateFilePath(file, PathCheckOption.IgnoreFileExists);
            if (result != PathCheckErrorCodes.SUCCESS)
            {
                _log.Info($"{nameof(ReadFile)} --is not a valid path");
                throw new ArgumentException($"{nameof(file)} is not a valid path. Received the following " +
    $"error code while validating: {result}", nameof(file));
            }

            var redirectionReturnCode = PathHelper.CheckPathRedirection(file);
            if (redirectionReturnCode != PathRedirectionReturn.PathIsNormal && redirectionReturnCode != PathRedirectionReturn.PathDoesNotExist)
            {
                throw new RedirectionDetectionException($"Redirection detected along the path {file}. " +
                                                        $"Received the following return code: {redirectionReturnCode}. Redirection is a potential security risk");
            }

            byte[] data;
            // Opens a stream to the path chosen in the open file dialog
            using (var stream = fileSystem.FileStreamCreate(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var size = (int)stream.Length;
                data = new byte[size];
                stream.Read(data, 0, size);
            }

            var text = System.Text.Encoding.UTF8.GetString(data);
            return text;
        }
    }

    /// <summary>
    /// Enum responses for Telemetry file parser
    /// </summary>
    public enum TelemetryParseResponse
    {
        /// <summary>
        /// Unknown response
        /// </summary>
        Unknown,

        /// <summary>
        /// User consent found
        /// </summary>
        Consent,

        /// <summary>
        /// User non-consent found
        /// </summary>
        NoConsent
    }
}