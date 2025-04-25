using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.MonitorAudio
{
    public class MonitorAudioModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView = new MonitorAudioRightView();
        private MonitorAudioViewModel vm = new MonitorAudioViewModel();

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public MonitorAudioModule(IModuleOwner? moduleOwner = null)
        {
            this.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;

            _rightView.DataContext = vm;
            vm.MyModule = this;
            vm.Invoke_RefreshData();
        }

        public string ModuleName { get => Constants.ModuleName_MonitorAudio; } //"MonitorAudioModule"

        public UserControl? GetLeftView()
        {
            return (UserControl?)_leftView;
        }

        public UserControl GetRightView()
        {
            return _rightView;
        }

        public HomeDevice? SelectedHomeDevice { get; set; }

        #region ModuleOwner

        public IModuleOwner? ModuleOwner
        {
            get => vm.ModuleOwner;
            set => vm.ModuleOwner = value;
        }

        #endregion ModuleOwner

        #region Event Handlers

        public void OnSelectedHomeDeviceChanged()
        {
            Trace.WriteLine("MonitorAudioModule.OnSelectedHomeDeviceChanged");
            isSelectChanged = true;
            if (IsModuleActive)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        //Handle new device coming
        private void InitNewViewModel()
        {
            vm.Invoke_RefreshData();
        }

        public void OnActivated()
        {
            Trace.WriteLine("MonitorAudioModule.OnActivated");
            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        public void OnDeactivated()
        {
            
        }

        #endregion Event Handlers
    }
}