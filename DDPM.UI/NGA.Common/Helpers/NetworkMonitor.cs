#region LicenseHeader
//
// ©Copyright 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using NGA.Resources;

namespace NGA.Common.Helpers {
  /// <summary>
  /// Implementation of Network Monitor interface
  /// </summary>
  public class NetworkMonitor : INetworkMonitor,IDisposable
    {
        private bool _isNetworkOnline;
        private bool _disposed;
        private static readonly Lazy<NetworkMonitor> Lazy = new(() => new NetworkMonitor());

        #region INetworkMonitor

        /// <summary>
        /// EventHandler for Network Change event
        /// </summary>
        public event EventHandler<NetworkMonitorEventArgs>? NetworkAvailableAndChanged;

        #endregion INetworkMonitor

        #region public methods

        /// <summary>
        /// Singleton instance of NetworkMonitor
        /// </summary>
        public static NetworkMonitor Instance => Lazy.Value;

        /// <summary>
        /// Get Internet Connectivity status
        /// </summary>
        /// <returns></returns>
        public bool IsInternetConnected()
        {
            var isInternetConnected = false;

            //Check Internet connectivity only when Network Connectivity Status is true
            if (_isNetworkOnline)
            {
                var internetConnectivityTask = CheckForInternetConnection();
                isInternetConnected = internetConnectivityTask.Result;
            }

            return isInternetConnected;
        }

        /// <summary>
        /// Dispose method
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region private methods

        /// <summary>
        /// private constructor
        /// </summary>
        private NetworkMonitor()
        {
            // Determine the initial network availability
            _isNetworkOnline = NetworkInterface.GetIsNetworkAvailable();

            // Subscribe to Network Availability Change events
            NetworkChange.NetworkAvailabilityChanged += AvailabilityChangedCallback;

            // Subscribe to Network Address Change events
            NetworkChange.NetworkAddressChanged += AddressChangedCallback;
        }

        /// <summary>
        /// EventHandler when IP address of Network Interface changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddressChangedCallback(object? sender, EventArgs e)
        {
            OnNetworkChange();
        }

        /// <summary>
        /// EventHandler when Availability of Network Changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AvailabilityChangedCallback(object? sender, NetworkAvailabilityEventArgs e)
        {
            _isNetworkOnline = e.IsAvailable;
            OnNetworkChange();
        }

        /// <summary>
        /// Network change event handler.
        /// this method is triggered periodically after timespan elapsed
        /// </summary>
       
        private void OnNetworkChange()
        {
            var isInternetConnected = false;
           
            //Check Internet connectivity only when Network Connectivity Status is true
            if (_isNetworkOnline)
            {
                var internetConnectivityTask  =  CheckForInternetConnection();
                isInternetConnected = internetConnectivityTask.Result;
            }

            NetworkMonitorEventArgs networkMonitorEventArgs = new(isInternetConnected);
            Task.Run(() =>
            {
                NetworkAvailableAndChanged?.Invoke(this, networkMonitorEventArgs);
            });
        }

        /// <summary>
        /// Checks for internet connectivity
        /// </summary>
        /// <returns></returns>
        private static async Task<bool> CheckForInternetConnection()
        {
            HttpClient httpClient = 
                new()
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };

            try
            {
                var response = await httpClient.GetAsync(new Uri(Resources.Resources.Url_Dell));
                if (response.StatusCode == HttpStatusCode.OK)
                    return true;
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Disposing managed resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                NetworkChange.NetworkAvailabilityChanged -= AvailabilityChangedCallback;
                NetworkChange.NetworkAddressChanged -= AddressChangedCallback;
            }

            _disposed = true;
        }

        #endregion
    }
}
