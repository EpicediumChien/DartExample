using DDPM.SA.Common;
using DDPM.SA.Common.Security;
using DDPM.UI.Common;
using DDPM.UI.Resources.Helper;
using DPeMPublic.Common.Enums;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        private readonly Microsoft.Win32.OpenFileDialog? openFileDialog;
        private readonly System.Windows.Forms.FolderBrowserDialog? folderBrowserDialog;
        private AdvancedAction _deviceCat;
        private bool IsForPen = false;

        public string Parameter { get; private set; } = "";

        public ActionParameterModalDialog(AdvancedAction deviceCat, double width, double height, string parameter = "", bool isForPen = false)
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
                    if (isForPen)
                    {
                        DdpmCommonHelper.DeviceManagerSA!.DeviceChanged += DeviceManagerSA_DeviceChanged;
                        IsForPen = true;
                        Task<bool> task = DdpmCommonHelper.DeviceManagerSA!.StartKeyCapturePen();
                        _ = task.Result;
                    }
                    else
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
                    txtKeystroke.PreviewTextInput += TxtKeystroke_PreviewTextInput;
                    txtDescription.Text = Strings.OpenWebPageDesc;
                    spKeystroke.Visibility = Visibility.Visible;
                    txtKeystroke.Watermark = LangHelper.Instance["URL"];
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

        private void DeviceManagerSA_DeviceChanged(object? sender, SA.Common.DeviceChangedEventArgs e)
        {
            if (e.type == DeviceChangedType.Peripherals_SettingsChange)
            {
                if (e.changedProperty.Split("|")[0] == "PenKeyCaptureProgressDataChanged")
                {
                    var txt = e.changedProperty.Substring(33);
                    if (txt.Length > 30)
                    {
                        Task<bool> task1 = DdpmCommonHelper.DeviceManagerSA!.FinishKeyCapturePen();
                        _ = task1.Result;
                        Task<string> task2 = DdpmCommonHelper.DeviceManagerSA!.KeyCaptureData();
                        var keystroke = task2.Result;
                    }
                    Application.Current.Dispatcher.Invoke(() => {
                        txtKeystroke.Text = e.changedProperty.Substring(33);
                    });
                }
            }
        }

        private void TxtKeystroke_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9a-zA-Z _.~:@/?&=#%+[\]!$()*,.;-]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void CancelClick(object sender, MouseButtonEventArgs e)
        {
            if (IsForPen)
                StopPenCapture();

            DialogResult = false;
            Close();
        }

        private void ClearClick(object sender, MouseButtonEventArgs e)
        {
            txtKeystroke.Text = "";

            btnClear.IsEnabled = false;
            if (IsForPen)
                RestartPenCapture();
        }

        private void SaveClick(object sender, MouseButtonEventArgs e)
        {
            //if (_deviceCat == AdvancedAction.OpenWebPage)
            //{
            //    if (!InputHelper.InputValidation_WebURL(txtKeystroke.Text, out string info))
            //    {
            //        MessageBox.Show(LangHelper.Instance["InvalidURL"]);
            //        return;
            //    }
            //}
            if (IsForPen)
            StopPenCapture();

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
                status += status == "" ? "Ctrl" : " + Ctrl";
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

        private void StopPenCapture()
        {
            Task<bool> task1 = DdpmCommonHelper.DeviceManagerSA!.FinishKeyCapturePen();
            _ = task1.Result;
            Task<string> task2 = DdpmCommonHelper.DeviceManagerSA!.KeyCaptureData();
            var keystroke = task2.Result;
            DdpmCommonHelper.DeviceManagerSA!.DeviceChanged -= DeviceManagerSA_DeviceChanged;
        }
        private void RestartPenCapture()
        {
            Task<bool> task = DdpmCommonHelper.DeviceManagerSA!.FinishKeyCapturePen();
            _ = task.Result;
            task = DdpmCommonHelper.DeviceManagerSA!.StartKeyCapturePen();
            _ = task.Result;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (IsForPen)
                StopPenCapture();
        }
    }
}