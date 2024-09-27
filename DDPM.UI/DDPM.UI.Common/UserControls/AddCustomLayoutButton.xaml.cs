using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common.UserControls
{
    /// <summary>
    /// Interaction logic for AddCustomLayoutButton.xaml
    /// </summary>
    public partial class AddCustomLayoutButton : UserControl
    {
        public AddCustomLayoutButton()
        {
            InitializeComponent();
        }

        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(AddCustomLayoutButton));

        private void addButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (ClickCommand != null)
                ClickCommand?.Execute(this);
        }
    }
}
