using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using System.Diagnostics;
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
            //Hide window from Alt+tab
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);
        }

        #endregion Init

        public void SetInputArg(EAArgs arg, Screen scr)
        {
            this.Dispatcher.Invoke(() =>
            {
                Trace.WriteLine($"  * EAArgs.CustomName=[{arg.CustomName}]");

                cbNames.Items.Clear();
                string selectedName = arg.CustomName;
                if ((arg.CustomNames != null) && (arg.CustomNames.Count > 0))
                {
                    int addCount = 0;
                    foreach (string name in arg.CustomNames)
                    {
                        string addName = name;
                        //Check length of name
                        if (addName.Length > EAEMConstants.MaxCustomNameLenth)
                            addName = addName.Substring(0, EAEMConstants.MaxCustomNameLenth);
                        cbNames.Items.Add((string)addName);
                        addCount++;
                        if (addCount >= EAEMConstants.MaxCustomItems)
                            break;
                    }
                }
                else //CustomNames is empty
                {
                    //Add one item to ComboBox
                    if (String.IsNullOrWhiteSpace(selectedName))
                    {
                        selectedName = "Custom Layout (1)";
                    }
                    cbNames.Items.Add(selectedName);
                }
                cbNames.SelectedValue = selectedName;

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
            //If it's empty
            if (String.IsNullOrWhiteSpace(cbNames.Text))
                return;
            //Trunk the string if it too long
            string retName = cbNames.Text;
            if (retName.Length > EAEMConstants.MaxCustomNameLenth)
                retName = retName.Substring(0, EAEMConstants.MaxCustomNameLenth);
            CustomName = retName;
            if (SaveButtonClick != null)
            {
                SaveButtonClick(this, CustomName);
            }
        }

        private void cbNames_Loaded(object sender, RoutedEventArgs e)
        {
            //Reference: https://stackoverflow.com/questions/1572887/how-to-set-maxlength-for-combobox-in-wpf
            System.Windows.Controls.ComboBox cb = (System.Windows.Controls.ComboBox)sender;
            if (cb != null)
            {
                var partEditableTextBox = (System.Windows.Controls.TextBox)cb.Template.FindName("PART_EditableTextBox", cb);
                if (partEditableTextBox != null)
                {
                    partEditableTextBox.MaxLength = EAEMConstants.MaxCustomNameLenth;
                }
            }
        }
    }
}