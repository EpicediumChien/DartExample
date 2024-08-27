#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// IDTPProxyPlugin.cs created on 8/13/2024T3:37 PM
//

#endregion

using System;
using System.Threading.Tasks;
using Dell.Client.Framework.Common;

namespace DDPM.SA.Common
{
    public interface IDTPProxyPlugin : IFrameworkPlugin
    {

        Task<int> GetDpiValue(string itemID);

        Task SetDPIValue(string itemID, int newValue);
        Task SetEraserDoublePressSetting(string itemID, byte[] newValue);
        Task SetEraserLongPressSetting(string itemID, byte[] newValue);
        Task SetEraserSinglePressSetting(string itemID, byte[] newValue);
        Task SetIsSideBottomButtonHoverClick(string itemID, bool newValue);
        Task SetIsSideTopButtonHoverClick(string itemID, bool newValue);
        Task SetMenuSinglePressSetting(string itemID, byte[] newValue);
        Task SetMenuCenterRightClickSetting(string itemID, byte[] newValue);
        Task SetSideBottomSwitchSinglePressSetting(string itemID, byte[] newValue);
        Task SetSideTopSwitchSinglePressSetting(string itemID, byte[] newValue);
        Task SetTiltSensitivity(string itemID, int newValue);
        Task SetTipSensitivity(string itemID, int newValue);
    }
}