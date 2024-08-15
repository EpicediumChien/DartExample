using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DDPM.UI.Common
{
    /// <summary>
    /// Interaction logic for VbarItem.xaml
    /// </summary>
    public partial class VbarItem : System.Windows.Controls.UserControl
    {
        private VbarItemViewModel vm = new VbarItemViewModel();

        public VbarItem(int id, ImageSource icon, string text)
        {
            InitializeComponent();
            vm.Id = id;
            vm.Icon = icon;
            vm.Text = text;
            //vm.Command = command;
            this.DataContext = vm;
        }

        public int Id => vm.Id;
        public string Text => vm.Text;

        private void rootGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (vm.IsLandingMode)
            {
                VisualStateManager.GoToState(this, "LandingHover", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Hover", false);
            }
        }

        private void rootGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (vm.IsLandingMode)
            {
                VisualStateManager.GoToState(this, "LandingNormal", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Normal", false);
            }
        }

        //public event RoutedEventHandler? Click;

        public ICommand? ClickCommand { get; set; }

        private void rootGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bdRoot.Focus();

            //Robert_Lin 2024-5-30, unused, Please remove to avoid duplicate event issue
            //Use ClickCommand instead. Click may always null
            //if (Click != null)
            //{
            //    Click(this, new RoutedEventArgs());
            //    return;
            //}

            if (ClickCommand != null)
                ClickCommand?.Execute(this);

            //vm.IsSelected = true;
        }

        private void bdRoot_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ClickCommand != null)
                ClickCommand?.Execute(this);
            //vm.IsSelected = true;
        }

        public void SetLadningMode(bool isLandingMode)
        {
            vm.IsLandingMode = isLandingMode;
            if (!isLandingMode)
                bdRoot.Width = 64;
            // << 240530 Added by Hess to push back VBar
            else bdRoot.Width = 212;
            // >>
        }

        public bool IsSelected
        {
            get => vm.IsSelected;
            set => vm.IsSelected = value;
        }
    }
}