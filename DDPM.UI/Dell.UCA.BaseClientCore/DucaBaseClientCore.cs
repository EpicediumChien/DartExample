#region LicenceHeader

//
// Copyright © 2023, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using Dell.Client.Framework.UX.WPF.ResourceManager.Enums;
using Microsoft;
using System.Diagnostics;
using System.Windows;

namespace NGA.BaseClientCore
{
    /// <summary>
    /// Abstract class for ClientCore Base
    /// This class contains common functionality that exist between ThickClient and Systray
    /// </summary>
    public abstract class DucaBaseClientCore : Application, IDisposable
    {
        #region private members

        private const string CultureCodeJapanese = "ja";
        private const string CultureCodeChinese = "zh";
        private const string CultureCodeKorean = "ko";

        private bool _disposed;

        #endregion

        private ResourceManager? _resourceManager;

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="uniqueId">Unique Identifier for application</param>
        /// <exception cref="ArgumentException">This exception is Thrown if <paramref name="uniqueId"/> is empty</exception>
        protected DucaBaseClientCore(Guid uniqueId)
        {
            Requires.NotEmpty(uniqueId, nameof(uniqueId));
        }

        #endregion

        #region public methods

        /// <summary>
        /// Abstract method for Configuring DCF console config
        /// </summary>
        /// <returns>Returns <see cref="IConsoleConfig"/> instance</returns>
        public abstract IConsoleConfig GetConsoleConfig();

        /// <summary>
        /// Creates the resource manager for the appliation. This is the place where you would create the resource manager and set the initial light or dark theme.
        /// </summary>
        /// <returns>The created <see cref="ResourceManager"/></returns>
        /// <example>
        /// This example shows how to override this method to create a resource manager with a predefined light theme.
        /// <code>
        ///     public override ResourceManager CreateResourceManager()
        ///     {
        ///         return new ResourceManager(PreDefinedColorType.PreDefinedLightUI);
        ///     }
        /// </code>
        /// </example>
        public abstract ResourceManager CreateResourceManager();

        /// <summary>
        /// Loads the default core library theme resources into the application
        /// </summary>
        public virtual ResourceManager LoadResources()
        {
            _resourceManager = CreateResourceManager();
            SetFontBasedOnLocale(_resourceManager);
            return _resourceManager;
        }

        /// <summary>
        /// Sets the appropriate based on the culture information (i.e., locale).
        ///
        /// This application supports the following languages (in order of locale):
        ///
        ///     Locale  Language                     Font
        ///     ------  ---------------------------  --------------
        ///     de-DE   German (Germany)             Roboto
        ///     en-US   English (United States)      Roboto
        ///     fr-FR   French (France)              Roboto
        ///     ja-JP   Japanese (Japan)             System Default
        ///     pt-BR   Brazilian Portuguese         Roboto
        ///     es-ES   Spanish (Spain)              Roboto
        ///     zh-CN   Chinese (China)              System Default
        ///     ko-KR   Korean                       System Default
        ///
        /// The chart above also shows the font we want to use for each language.
        ///
        /// So in this method, if the current local is one of the above, we set the font
        /// according to the font assignment for the given language.
        ///
        /// If the the locale does not match any of the above, we set the font as if the
        /// language were English.
        /// </summary>
        /// <param name="resourceManager"></param>
        public virtual void SetFontBasedOnLocale(ResourceManager resourceManager)
        {
            // Get the culture/locale information.
            var cultureInfo = Thread.CurrentThread.CurrentUICulture;
            Debug.WriteLine(cultureInfo.Name);

            // For most languages, we want to use the Roboto font.
            var useSystemDefaultFont = false;
            const FontThemeType fontTheme = FontThemeType.PreDefinedRobotoFont;

            var languagesWithSystemDefaultFont = new List<string>() { CultureCodeChinese, CultureCodeJapanese, CultureCodeKorean };

            // Check current culture is part of specific language group hierarchy
            if (languagesWithSystemDefaultFont.Contains(cultureInfo.Name, StringComparer.OrdinalIgnoreCase)
                || languagesWithSystemDefaultFont.Contains(cultureInfo.Parent.Name, StringComparer.OrdinalIgnoreCase)
                || languagesWithSystemDefaultFont.Contains(cultureInfo.Parent.Parent.Name, StringComparer.OrdinalIgnoreCase))
            {
                useSystemDefaultFont = true;
            }

            if (useSystemDefaultFont)
            {
                if (ResourceManager.CurrentlyInstalledFontThemeType.HasValue)
                {
                    // Uninstall the currently installed font theme so that the system default will be used.
                    resourceManager.RemoveInstalledFontTheme().StageAndCommitResources();
                }
            }
            else
            {
                if (fontTheme != ResourceManager.CurrentlyInstalledFontThemeType)
                {
                    // Update the installed/active font to the given font.
                    resourceManager.AddFontTheme(fontTheme).StageAndCommitResources();
                }
            }
        }

        /// <summary>
        /// Dispose unmanaged resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Disposing managed resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _resourceManager?.Dispose();

            _disposed = true;
        }
    }
}