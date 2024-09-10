#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// IDTPProxyPlugin.cs created on 8/13/2024T3:37 PM
//

#endregion

using Dell.Client.Framework.Common;
using System.Threading.Tasks;

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

        Task SetMenuCenterRightClickSetting(string itemID, bool newValue);

        Task SetSideBottomSwitchSinglePressSetting(string itemID, byte[] newValue);

        Task SetSideTopSwitchSinglePressSetting1(string itemID, byte[] newValue);
        Task SetSideTopSwitchSinglePressSetting2(string itemID, string newValue);

        Task SetTiltSensitivity(string itemID, int newValue);

        Task SetTipSensitivity(string itemID, int newValue);

        // webcam
        Task<int> GetBrightnessValue(string itemID);

        Task SetBrightnessValue(string itemID, int newValue);

        Task<string> GetCameraFirmwareVersion(string itemID);

        Task<bool> CheckIsPropertyFOVSupported(string itemID);

        Task<int> GetFieldOfViewValue(string itemID);

        Task<bool> CheckIsPropertyHDRSupported(string itemID);

        Task<bool> GetIsHDROnValue(string itemID);

        Task SetIsHDROnValue(string itemID, bool newValue);

        Task<bool> CheckIsPropertyAntiFlickerSupported(string itemID);

        Task<int> GetAntiFlickerValue(string itemID);

        Task SetAntiFlickerValue(string itemID, int newValue);

        Task<bool> CheckIsPropertyAutoFramingSupported(string itemID);

        Task<bool> GetIsAutoFramingOnValue(string itemID);

        Task SetIsAutoFramingOnValue(string itemID, bool newValue);
        Task SetIsMicEnumerationOn(string Guid, bool newValue);

    }
}