using DDPM.SA.Common.Security;
using DDPM.UI.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// ActionParameterModalDialog.xaml 的互動邏輯
    /// </summary>
    public partial class ActionParameterModalDialog : Window
    {
        private readonly string Caption = "";
        //private readonly string Cancel = "Cancel";
        //private readonly string Clear = "Clear";
        //private readonly string Save = "Save";
        //private readonly string Browse = "Browse";

        private readonly Microsoft.Win32.OpenFileDialog? openFileDialog;
        private readonly System.Windows.Forms.FolderBrowserDialog? folderBrowserDialog;
        private AdvancedAction _deviceCat;

        public string Parameter { get; private set; } = "";

        public ActionParameterModalDialog(AdvancedAction deviceCat, double width, double height, string parameter = "")
        {
            InitializeComponent();
            this.Width = width;
            this.Height = height;

            txtCaption.Text = Caption;
            _deviceCat = deviceCat;

            switch (deviceCat)
            {
                case AdvancedAction.AssignKeystroke:
                    Caption = Strings.AssignKeystroke;
                    txtDescription.Text = Strings.AssignKeystrokeDesc;
                    txtKeystroke.Text = parameter;
                    btnClear.IsEnabled = parameter != "";
                    spKeystroke.Visibility = Visibility.Visible;
                    this.PreviewKeyDown += Keystroke_PreviewKeyDown;
                    break;

                case AdvancedAction.OpenFile:
                    Caption = Strings.OpenFile;
                    txtDescription.Text = Strings.OpenFileDesc;
                    txtOpen.Text = parameter;
                    spOpen.Visibility = Visibility.Visible;
                    btnClear.Visibility = Visibility.Collapsed;
                    txtWaterMark.Text = Strings.OpenFileWaterMark;
                    openFileDialog = new();
                    openFileDialog.FileName = parameter;
                    break;

                case AdvancedAction.OpenFolder:
                    Caption = Strings.OpenFolder;
                    txtDescription.Text = Strings.OpenFolderDesc;
                    txtOpen.Text = parameter;
                    spOpen.Visibility = Visibility.Visible;
                    btnClear.Visibility = Visibility.Collapsed;
                    txtWaterMark.Text = Strings.OpenFolderWaterMark;
                    folderBrowserDialog = new();
                    break;

                case AdvancedAction.OpenWebPage:
                    Caption = Strings.OpenWebPage;
                    txtKeystroke.Text = parameter;
                    txtDescription.Text = Strings.OpenWebPageDesc;
                    spKeystroke.Visibility = Visibility.Visible;
                    txtKeystroke.IsEnabled = true;
                    break;
            }
            txtTitleBar.Text = Caption;
            txtCaption.Text = Caption;
            btnCancel.Caption = Strings.Cancel;
            btnClear.Caption = Strings.Clear;
            btnSave.Caption = Strings.Save;
            btnBrowse.Caption = Strings.Browse;
        }

        private void CancelClick(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ClearClick(object sender, MouseButtonEventArgs e)
        {
            txtKeystroke.Text = "";

            btnClear.IsEnabled = false;
        }

        private void SaveClick(object sender, MouseButtonEventArgs e)
        {
            if (_deviceCat == AdvancedAction.OpenWebPage)
            {
                if (!InputHelper.InputValidation_WebURL(txtKeystroke.Text, out string info))
                {
                    MessageBox.Show("Invalid URL");
                    return;
                }
            }
            DialogResult = true;
            Close();
        }

        private void KeystrokeTextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtKeystroke.Text == "")
            {
                btnClear.IsEnabled = false;
                btnSave.IsEnabled = false;
            }
            else
            {
                btnClear.IsEnabled = true;
                btnSave.IsEnabled = true;
                Parameter = txtKeystroke.Text;
            }
        }

        private void Keystroke_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void Keystroke_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var key = (e.Key == Key.System ? e.SystemKey : e.Key);
            if (key == Key.LWin)
            { e.Handled = true; return; }
            string status = "";

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                status += status == "" ? "Control" : " + Control";
            }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            {
                status += status == "" ? "Alt" : " + Alt";
            }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                status += status == "" ? "Shift" : " + Shift";
            }
            //if((Keyboard.Modifiers & ModifierKeys.Windows) == ModifierKeys.Windows) {
            //  status += status == "" ? "Windows" : " + Windows";
            //}

            var keyName = (int)key switch
            {
                2 => "",         //Backspace
                3 => "",         //Tab
                6 => "",         //Enter
                7 => "",         //Pause
                8 => "",         //Caps Lock
                13 => "",        //Escape
                18 => "",        //Space
                20 => "Page Down",
                > 33 and < 44 => key.ToString().Replace("D", " "),
                > 73 and < 84 => key.ToString().Replace("Pad", " "),
                84 => "Num *",
                85 => "Num +",
                87 => "Num -",
                88 => "Num .",
                89 => "Num /",
                140 => ";",
                141 => "=",
                142 => ",",
                143 => "-",
                144 => ".",
                145 => "/",
                146 => "`",
                149 => "[",
                150 => "\\",
                151 => "]",
                152 => "'",
                >= 90 => "",
                _ => key.ToString()
            };

            txtKeystroke.Text = status == "" ? keyName : status + $" + {keyName}";
            //txtKeystroke.Text = key.ToString();
            e.Handled = true;
        }

        private void Clear_txtOpen(object sender, MouseButtonEventArgs e)
        {
            txtOpen.Text = "";
        }

        private void OpenTextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtOpen.Text == "")
            {
                btnSave.IsEnabled = false;
                txtWaterMark.Visibility = Visibility.Visible;
            }
            else
            {
                btnSave.IsEnabled = true;
                Parameter = txtOpen.Text;
                txtWaterMark.Visibility = Visibility.Collapsed;
            }
        }

        private void BrowseClick(object sender, MouseButtonEventArgs e)
        {
            if (folderBrowserDialog == null)
            {
                if (openFileDialog!.ShowDialog() == true)
                {
                    txtOpen.Text = openFileDialog.FileName;
                }
            }
            else
            {
                folderBrowserDialog.Description = "Select a folder";
                folderBrowserDialog.ShowNewFolderButton = true;

                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string folderPath = folderBrowserDialog.SelectedPath;
                    txtOpen.Text = folderPath;
                }
            }
        }
    }
}