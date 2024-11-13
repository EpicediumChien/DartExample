using Dell.Client.Framework.UX.WPF.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DDPM.UI.Common
{
    /// <summary>
    /// Interaction logic for VbarItem.xaml
    /// </summary>
    public partial class VbarItem : System.Windows.Controls.UserControl
    {
        private SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);
        private VbarItemViewModel vm = new VbarItemViewModel();
        private Canvas? canvas = null;

        public VbarItem(int id, ImageSource icon, string text, Canvas? iconCanvas = null)
        {
            InitializeComponent();
            vm.Id = id;
            vm.Icon = icon;
            vm.Text = text;
            //vm.Command = command;
            this.DataContext = vm;
            if (iconCanvas != null)
            {
                iconCanvas.Tag = (SolidColorBrush)System.Windows.Application.Current.Resources["DefaultTheme_PathColor"];
                canvas = iconCanvas;
                CanvasContainer.Content = canvas;
            }
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

            SelectBarItem();

            if (ClickCommand != null)
                ClickCommand?.Execute(this);
        }

        private void bdRoot_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ClickCommand != null)
                ClickCommand?.Execute(this);
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
        public Visibility TooltipVisibility
        {
            get => vm.TooltipVisibility;
            set => vm.TooltipVisibility = value;
        }

        public void RenewBarItem()
        {
            IconName.ClearValue(TextBlock.ForegroundProperty);
            if (canvas != null)
            {
                canvas.Tag = (SolidColorBrush)System.Windows.Application.Current.Resources["DefaultTheme_PathColor"];
            }

        }

        public void SelectBarItem()
        {
            IconName.Foreground = whiteBrush;
            if (canvas != null) 
            {
                canvas.Tag = whiteBrush;
            }
        }
    }
}