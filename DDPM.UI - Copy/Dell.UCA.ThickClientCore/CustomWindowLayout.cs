#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using Microsoft;
using NGA.ThickClient.Interfaces;
using System.Windows.Automation;
using SystemWindows = System.Windows;

namespace NGA.ThickClientCore
{
    /// <summary>
    /// Defines the custom window layout for NGA ThickClient
    /// </summary>
    public sealed class CustomWindowLayout : ICustomWindowLayout, IDisposable
    {
        private readonly IShowPluginManager _showPluginManager;
        private readonly IWindowLayout _windowLayout;
        private bool _disposedValue;

        /// <summary>
        /// This creates a custom window layout
        /// </summary>
        /// <param name="windowLayout">WindowLayout object</param>
        /// <param name="gearMenu">GearMenu object</param>
        /// <param name="showPluginManager">Show Plugin Manager Object</param>
        public CustomWindowLayout(IWindowLayout windowLayout, IGearMenu gearMenu, IShowPluginManager showPluginManager)
        {
            Requires.NotNull(windowLayout, nameof(windowLayout));
            _windowLayout = windowLayout;
            Requires.NotNull(showPluginManager, nameof(showPluginManager));
            _showPluginManager = showPluginManager;
            Requires.NotNull(gearMenu, nameof(gearMenu));

            if (windowLayout.Masthead != null)
            {
                windowLayout.Masthead.IconVisible = true;
                windowLayout.Masthead.IconEnabled = true;
                windowLayout.Masthead.IconClick += Masthead_IconClick;
                windowLayout.Masthead.TitleVisible = false;
                windowLayout.Masthead.MinimizeButtonVisible = true;
                windowLayout.Masthead.MinimizeButtonEnabled = true;
                windowLayout.Masthead.MaximizeButtonVisible = false;
                windowLayout.Masthead.MaximizeButtonEnabled = false;
                windowLayout.Masthead.CloseButtonVisible = true;
                windowLayout.Masthead.CloseButtonEnabled = true;
                windowLayout.Masthead.NarrateIconButton = Resources.Resources.ReturnToHomePage_Text;
                windowLayout.Masthead.NarrateMinimizeButton = Resources.Resources.Minimize;
                windowLayout.Masthead.NarrateMaximizeButton = Resources.Resources.Maximize;
                windowLayout.Masthead.NarrateCloseButton = Resources.Resources.Close;
                windowLayout.Masthead.NarrateRestoreButton = Resources.Resources.Restore;
                windowLayout.Masthead.CustomContentAreaRightVisible = true;
                IconComboBox.Style = (SystemWindows.Style)SystemWindows.Application.Current.TryFindResource("IconComboBoxStyle");

                //Robert_Lin, 2024-6-26, to remove gear menu
                // windowLayout.Masthead.InsertCustomContent(IconComboBox);

                windowLayout.Masthead.Height = 64;
                if (windowLayout.Frame != null)
                {
                    windowLayout.Frame.NarrateBackButton = Resources.Resources.NarratePageBackButton;

                    windowLayout.Frame.NarrateForwardButton = Resources.Resources.NarratePageForwardButton;
                }

                AutomationProperties.SetName(IconComboBox, Resources.Resources.CarrotMenuItem_Text);
                AutomationProperties.SetName(NotificationIcon, Resources.Resources.AdditionalNotifications_Text);

                //Add gear menu items to IconComboBox
                //Robert_Lin, 2024-6-26, to remove gear menu
                //IconComboBox.ItemsSource = gearMenu.GearMenuItems;
            }
        }

        /// <summary>
        /// Handles the Icon Click event in the masthead
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Masthead_IconClick(object? sender, EventArgs e)
        {
            _showPluginManager.ShowHomePage();
        }

        /// <inheritdoc/>
        public UXBell NotificationIcon { get; } = new() { Height = 32, Width = 32, IsEnabled = false };

        /// <inheritdoc/>
        public UXIconComboBox IconComboBox { get; } = new();

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                /*
                 * In the future split this statement but for now SQ complains
                 * All hail the SQ :)
                 */
                if (disposing && _windowLayout.Masthead != null)
                {
                    _windowLayout.Masthead.IconClick -= Masthead_IconClick;
                }

                _disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes of the object
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}