using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.Brightness
{
    public class BrightnessModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl? _rightView = new BrightnessRightView();
        private BrightnessViewModel? vm;
        private DDPMSettings? _settings;

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        //Robert_Lin 2024-5-30, remove argument from all Module's ctor
        //public BrightnessModule(HomeDevice? homeDevice)
        public BrightnessModule(IModuleOwner? moduleOwner = null)
        {
            if (vm == null)
                vm = new BrightnessViewModel(); //BrightnessViewModel.GetInstance(); //do not use getinstance here due to all monitors share the same view model

            if (vm != null && _rightView != null)
            {
                vm.ModuleOwner = DdpmCommonHelper.ModuleOwner;
                vm.SelectedHomeDevice = SelectedHomeDevice = vm.ModuleOwner.SelectedHomeDevice;
                vm.MyModule = this;
                _rightView.DataContext = vm;
                vm.Invoke_RefreshBrightnessPage();
                //vm.Invoke_RefreshHotkeySettings();
            }
        }

        public string ModuleName { get => Constants.ModuleName_Brightness; } // "BrightnessModule"

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
            Trace.WriteLine("BrightnessModule.OnSelectedHomeDeviceChanged");
            //vm.SetIsBusy(vm.IsBusy);
            isSelectChanged = true;
            if (IsModuleActive)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
            vm.SetIsBusy(vm.IsBusyALS);
        }

        private void InitNewViewModel()
        {
            vm = new BrightnessViewModel();

            if (vm != null && _rightView != null)
            {
                vm.ModuleOwner = DdpmCommonHelper.ModuleOwner;
                vm.SelectedHomeDevice = SelectedHomeDevice = vm.ModuleOwner.SelectedHomeDevice;
                vm.MyModule = this;
                _rightView.DataContext = vm;
                vm.Invoke_RefreshBrightnessPage();
            }
        }

        public void OnActivated()
        {
            Trace.WriteLine("BrightnessModule.OnActivated");
            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }

            if (vm is not null)
            {
                BackgroundWorker bw = new BackgroundWorker()
                {
                    WorkerReportsProgress = false,
                    WorkerSupportsCancellation = false
                };

                bw.DoWork += vm.DoWork_RefreshManualValue;
                bw.RunWorkerCompleted += vm.RunWorkerCompleted_RefreshManualValue;
                bw.RunWorkerAsync();

                //vm?.UpdateHDRStatus();
            }           

            if (_rightView == null)
                return;
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("BrightnessModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}