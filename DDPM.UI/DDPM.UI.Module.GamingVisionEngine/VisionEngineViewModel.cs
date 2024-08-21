using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.Common;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using VcpCore.Common;

[assembly: InternalsVisibleTo("DDPM.UI.Module.Gaming.Tests")]

namespace DDPM.UI.Module.GamingVisionEngine
{
    internal class VisionEngineViewModel : ObservableObject
    {
        public IModuleOwner? ModuleOwner { get; set; }
        public VisionEngineModule MyModule { get; set; }

        /// <summary>
        /// Array contents[Night, Clear, Bino, Chroma, Crosshair]
        /// </summary>
        private bool[] _isCheck_VisionEngine = new bool[5];
        /// <summary>
        /// Array contents[Night, Clear, Bino, Chroma, Crosshair]
        /// </summary>
        public bool[] IsCheck_VisionEngine
        {
            get => _isCheck_VisionEngine;
            set
            {
                SetProperty(ref _isCheck_VisionEngine, value);
            }
        }

        #region UI Enable Flags

        private bool _isBusy = false;

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        #endregion UI Enable Flags

        public void Invoke_RefreshData()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_RefreshData;
            bw.RunWorkerCompleted += RunWorkerCompleted_RefreshData;
            bw.RunWorkerAsync();
            IsBusy = true;
        }

        private void DoWork_RefreshData(object sender, DoWorkEventArgs e)
        {
            try
            {

                GamingDisplayPropertiesInfo displayPropertiesInfo = DdpmCommonHelper.DeviceManagerSA.GetGamingProperties(MyModule.SelectedHomeDevice.MonitorInfo).Result;

                MyModule.GetRightView().Dispatcher.Invoke((Action)(() =>
                {

                }));

                RefreshUI();
            }
            catch (Exception)
            {
            }
        }

        private void RunWorkerCompleted_RefreshData(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            //Handling the result and final process
        }

        public void RefreshUI()
        {

        }
    }

    internal class UI_Properties
    {
        public Properties Properties { get; set; }

        public string DisplayText
        {
            get
            {
                return $"{Properties.Resolutions_Width}x{Properties.Resolutions_High}, {Properties.Frequency}Hz {(Properties.isRecommended ? "(Recommended)" : "")}";
            }
        }
    }
    internal class UI_GameEnhancementMode
    {
        public Gaming_GameEnhancementMode GameEnhancementMode { get; set; }

        public string DisplayText
        {
            get
            {
                return $"{GameEnhancementMode.ToString().Replace("__", "/").Replace("_", " ")}";
            }
        }
    }
    internal class UI_ResponseTime
    {
        public Gaming_ResponseTime ResponseTime { get; set; }

        public string DisplayText
        {
            get
            {
                return $"{ResponseTime.ToString().Replace("__", "/").Replace("_", " ")}";
            }
        }
    }
    internal class UI_DarkStabilizer
    {
        public Gaming_DarkStabilizer DarkStabilizer { get; set; }

        public string DisplayText
        {
            get
            {
                return $"{DarkStabilizer.ToString().Replace("__", "/").Replace("_", " ")}";
            }
        }
    }
    internal class UI_HDRType
    {
        public Gaming_HDRType HDRType { get; set; }

        public string DisplayText
        {
            get
            {
                return $"{HDRType.ToString().Replace("__", "/").Replace("_", " ")}";
            }
        }
    }
}