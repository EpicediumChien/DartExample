namespace DDPM.UI.Plugin.DdpmHomePlugin.Interfaces
{
    internal interface IDeviceInfoService
    {
        /// <summary>
        /// Get the DeviceInfo
        /// </summary>
        /// <returns>Returns instance of <see cref="IDeviceInfo"/></returns>
        IDeviceInfo? GetDeviceInfo();
    }
}