namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for SplitCtrl0A.xaml
    /// </summary>
    public partial class PxPSplitCtrl0A : System.Windows.Controls.UserControl
    {
        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        public PxPSplitCtrl0A()
        {
            InitializeComponent();
        }

        private void LastInput_ButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.ChangePC(false);
            }
        }

        private void NextInput_ButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.ChangePC(true);
            }
        }
    }
}