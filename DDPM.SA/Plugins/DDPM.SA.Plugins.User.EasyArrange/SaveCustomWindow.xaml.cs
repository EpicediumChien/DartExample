using DDPM.SA.Common;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace DDPM.SA.Plugins.User.EasyArrange
{
    /// <summary>
    /// Interaction logic for SaveCustomWindow.xaml
    /// </summary>
    public partial class SaveCustomWindow : Window
    {
        #region Input/Output

        //Setup before calling Show()
        public EventHandler<string>? SaveButtonClick;

        public EventHandler<string>? CancelButtonClick;
        public string CustomName;
        public List<string> CustomNames;

        #endregion Input/Output

        #region Init

        public SaveCustomWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Add CustomNames to the ComboBox.Items
            if (CustomNames != null)
            {
                foreach (string name in CustomNames)
                {
                    cbNames.Items.Add(name);
                }
                //Determine the selection
                cbNames.SelectedIndex = CustomNames.IndexOf(CustomName);
            }
        }

        #endregion Init

        public void SetInputArg(EAArgs arg, Screen scr)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (arg.CustomName != null)
                {
                    foreach (string name in arg.CustomNames)
                    {
                        cbNames.Items.Add((string)name);
                    }
                }
                else
                {
                    cbNames.Items.Add(arg.CustomName);
                }
                cbNames.SelectedValue = arg.CustomName;

                //Calculate the position/size of EditWindow
                double dpiX = 1.000;
                var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                if (dpiXProperty != null)
                {
                    var varX = (int)dpiXProperty.GetValue(null, null);
                    dpiX = (double)varX / (double)96;
                }

                Left = scr.WorkingArea.Left / (double)dpiX;
                Top = scr.WorkingArea.Top / (double)dpiX;

                Show();
                Topmost = true;
            });
        }

        private void rootGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((e.ChangedButton == MouseButton.Left) && (e.ClickCount == 1))
            {
                this.DragMove();
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (CancelButtonClick != null)
            {
                CancelButtonClick(this, "");
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            CustomName = cbNames.Text;
            if (SaveButtonClick != null)
            {
                SaveButtonClick(this, CustomName);
            }
        }
    }
}