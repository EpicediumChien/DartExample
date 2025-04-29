using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Interfaces.ViewModels;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DDPM.UI.Module.DisplayWebcam
{
    public class DisplayWebcamModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl? _rightView = null;

        private WebCameraViewModel vm;
        private bool isSelectChanged = false;
        public string ModuleName { get => Constants.ModuleName_DisplayWebcam; }

        public HomeDevice? SelectedHomeDevice { get; set; }

        public DisplayWebcamModule(WebCameraViewModel webCameraViewModel)
        {
            this.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
            _leftView = new DisplayWebcamLeftView(webCameraViewModel);
            //_leftView.DataContext = vm;

        }
        public IModuleOwner? ModuleOwner
        {
            get;
            set;
        }
        public bool IsModuleActive { get; set; } = false;

        public UserControl? GetLeftView()
        {
            return (UserControl?)_leftView;
        }

        public UserControl GetRightView()
        {
            return (UserControl?)_rightView;
        }

        public void OnActivated()
        {
            Trace.WriteLine("DisplayOthersModule.OnActivated");


            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        private void InitNewViewModel()
        {
            this.SelectedHomeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
            //vm.Invoke_RefreshData();
        }
        public void OnDeactivated()
        {
            Trace.WriteLine("DisplayOthersModule.OnDeactivated");
        }

        public void OnSelectedHomeDeviceChanged()
        {
            Trace.WriteLine("DisplayOthersModule.OnSelectedHomeDeviceChanged");
            isSelectChanged = true;
            if (IsModuleActive)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }
    }
}
