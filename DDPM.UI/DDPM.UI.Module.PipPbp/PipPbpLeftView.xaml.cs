using CommunityToolkit.Mvvm.Input;
using DDPM.UI.Common.Models;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.PipPbp
{
    /// <summary>
    /// Interaction logic for PipPbpLeftView.xaml
    /// </summary>
    public partial class PipPbpLeftView : UserControl
    {
        private PipPbpViewModel vm
        {
            get { return (PipPbpViewModel)DataContext; }
        }

        public PipPbpLeftView()
        {
            InitializeComponent();
        }

        private void fullViewTestButton_Click(object sender, RoutedEventArgs e)
        {
            ShowF1ViewCommand();
        }

        private void Test1FullView_CloseFullViewCommand()
        {
            //vm.ModuleOwner?.CloseFullView();

            if ((vm.ModuleOwner != null) && (vm.ModuleOwner?.HomeDevices != null))
            {
                foreach (HomeDevice d in vm.ModuleOwner.HomeDevices)
                {
                }
            }
        }

        private void ShowF1ViewCommand()
        {
            Test1FullView test1FullView = new Test1FullView();
            test1FullView.DataContext = vm;
            vm.ModuleOwner?.OpenFullView(test1FullView);
        }

        private void ShowF2ViewCommand()
        {
            Test2FullView test2FullView = new Test2FullView();
            test2FullView.DataContext = vm;
            vm.ModuleOwner?.OpenFullView(test2FullView);
        }

        private void HandleCloseFullViewCommand()
        {
            vm.ModuleOwner?.CloseFullView();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            vm.GotoNextCommand = new RelayCommand(ShowF2ViewCommand);
            vm.GotoPrevCommand = new RelayCommand(() =>
            {
                ShowF1ViewCommand();
            });
            vm.CloseFullViewCommand = new RelayCommand(() => { HandleCloseFullViewCommand(); });
        }
    }
}