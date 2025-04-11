using DDPM.SA.Common;
using DDPM.SA.Common.Security;
using DDPM.UI.Common;
using DDPM.UI.Resources.Helper;
using DPeMPublic.Common.Enums;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Windows.Threading;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// ActionParameterModalDialog.xaml 的互動邏輯
    /// </summary>
    public partial class ActionParameterModalDialog : System.Windows.Window
    {
        private readonly string Caption = "";

        private readonly Microsoft.Win32.OpenFileDialog? openFileDialog;
        private readonly System.Windows.Forms.FolderBrowserDialog? folderBrowserDialog;

        private readonly string guid;
        private readonly string pType;
        private readonly AdvancedAction action;
        private readonly DispatcherTimer timer;

        public string Parameter { get; private set; } = "";

        public ActionParameterModalDialog(AdvancedAction deviceCat, double width, double height, string parameter = "", string type = "", string Guid = "")
        {
            InitializeComponent();
            this.Width = width;
            this.Height = height;
            Activated += ActionParameterModalDialog_Activated;
            Deactivated += ActionParameterModalDialog_Deactivated;

            txtCaption.Text = Caption;
            guid = Guid;
            pType = type;
            action = deviceCat;

            switch (deviceCat)
            {
                case AdvancedAction.AssignKeystroke:
                    Caption = Strings.AssignKeystroke;
                    txtDescription.Text = Strings.AssignKeystrokeDesc;
                    txtKeystroke.Text = parameter;
                    btnClear.IsEnabled = parameter != "";
                    spKeystroke.Visibility = Visibility.Visible;
                    //if (DdpmCommonHelper.DeviceManagerSA == null)
                    //{
                    //    DdpmCommonHelper.WriteUILog($"Assign KeyStroke Error: DeviceManagerSA is null!");
                    //    return;
                    //}
                    //if (pType == "PEN")
                    //{
                    //    Task<bool> task = DdpmCommonHelper.DeviceManagerSA.StartKeyCapturePen();
                    //    _ = task.Result;
                    //}
                    //else if (pType == "KB")
                    //{
                    //    if (!DdpmCommonHelper.DeviceManagerSA.StartKeyboardKeystrokeRecording(guid).Result)
                    //    {
                    //        DdpmCommonHelper.WriteUILog($"Assign KeyStroke Error: Can't StartKeyboardKeystrokeRecording!");
                    //        return;
                    //    }
                    //    //this.PreviewKeyDown += Keystroke_PreviewKeyDown;
                    //}
                    //else
                    //{
                    //    if (!DdpmCommonHelper.DeviceManagerSA.StartMouseKeystrokeRecording(guid).Result)
                    //    {
                    //        DdpmCommonHelper.WriteUILog($"Assign KeyStroke Error: Can't StartMouseKeystrokeRecording!");
                    //        return;
                    //    }
                    //    //this.PreviewKeyDown += Keystroke_PreviewKeyDown;
                    //}
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
            txtAlert.Text = LangHelper.Instance["InputValidationTooltip.3"];

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            timer.Tick += Timer_Tick;
            ;
            Unloaded += ActionParameterModalDialog_Unloaded;
            ;
        }

        private void ActionParameterModalDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            bdAlert.Visibility = Visibility.Collapsed;
            timer.Stop();
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
                        _ = DdpmCommonHelper.DeviceManagerSA?.FinishKeyCapturePen();
                        Task<string>? task2 = DdpmCommonHelper.DeviceManagerSA?.KeyCaptureData();
                        var keystroke = task2?.Result ?? "";
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        txtKeystroke.Text = e.changedProperty.Substring(33);
                    });
                }
                else if (e.changedProperty == "KeyStrokeDisplayDataChanged")
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        txtKeystroke.Text = e.device_peripherals.Message;
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
            if (pType == "PEN")
                StopPenCapture();
            else
            {
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    if (pType == "KB")
                        _ = DdpmCommonHelper.DeviceManagerSA?.StopKeyboardKeystrokeRecording(guid);
                    else
                        _ = DdpmCommonHelper.DeviceManagerSA?.StopMouseKeystrokeRecording(guid);
                }
            }

            DialogResult = false;
            Close();
        }

        private void ClearClick(object sender, MouseButtonEventArgs e)
        {
            txtKeystroke.Text = "";

            btnClear.IsEnabled = false;
            if (pType == "PEN")
                RestartPenCapture();
        }

        private void SaveClick(object sender, MouseButtonEventArgs e)
        {
            StopCapture();
            DialogResult = true;
            Close();
        }

        private void KeystrokeTextChanged(object sender, TextChangedEventArgs e)
        {
            btnClear.IsEnabled = false;
            btnSave.IsEnabled = false;
            var txt = txtKeystroke.Text.Trim();
            if (txt == "")
                return;

            if (txt.Equals("ALT + Z", StringComparison.InvariantCultureIgnoreCase) || txt.Equals("Z + ALT", StringComparison.InvariantCultureIgnoreCase))
            {
                MessageModalDialog messageModalDialog;
                System.Windows.Window mainWindow = System.Windows.Application.Current.MainWindow;
                messageModalDialog = new(LangHelper.Instance["hotkey.7"], LangHelper.Instance["Hotkey.10"], "", "");
                if (mainWindow != null)
                {
                    messageModalDialog.Owner = mainWindow;
                    messageModalDialog.Left = mainWindow.Left + (mainWindow!.ActualWidth - 417) / 2;
                    messageModalDialog.Top = mainWindow.Top + 300;
                }
                messageModalDialog.ShowDialog();
                txtKeystroke.Text = "";
                return;
            }

            btnClear.IsEnabled = true;
            btnSave.IsEnabled = true;
            Parameter = txt;
        }

        private void Keystroke_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DdpmCommonHelper.DeviceManagerSA == null)
                return;

            string ketStroke;
            if (pType == "KB")
                ketStroke = DdpmCommonHelper.DeviceManagerSA.GetKeyboardKeystrokeDisplayData(guid).Result;
            else
                ketStroke = DdpmCommonHelper.DeviceManagerSA.GetMouseKeystrokeDisplayData(guid).Result;
            txtKeystroke.Text = ketStroke;
            e.Handled = true;
            return;
            //if (e.SystemKey == Key.Escape && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            //{
            //}
            //var key = (e.Key == Key.System ? e.SystemKey : e.Key);
            //if (key == Key.LWin)
            //{ e.Handled = true; return; }
            //string status = "";

            //if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            //{
            //    status += status == "" ? "Ctrl" : " + Ctrl";
            //}
            //if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
            //{
            //    status += status == "" ? "Alt" : " + Alt";
            //}
            //if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            //{
            //    status += status == "" ? "Shift" : " + Shift";
            //}
            //if ((Keyboard.Modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
            //{
            //    status += status == "" ? "Windows" : " + Windows";
            //}

            //var keyName = (int)key switch
            //{
            //    2 => "",         //Backspace
            //    3 => "Tab",         //Tab
            //    6 => "",         //Enter
            //    7 => "",         //Pause
            //    8 => "",         //Caps Lock
            //    13 => "Esc",        //Escape
            //    18 => "Space",        //Space
            //    20 => "Page Down",
            //    > 33 and < 44 => key.ToString().Replace("D", " "),
            //    > 73 and < 84 => key.ToString().Replace("Pad", " "),
            //    84 => "Num *",
            //    85 => "Num +",
            //    87 => "Num -",
            //    88 => "Num .",
            //    89 => "Num /",
            //    91 => "F2",
            //    92 => "F3",
            //    93 => "F4",
            //    94 => "F5",
            //    95 => "F6",
            //    97 => "F8",
            //    99 => "F10",
            //    140 => ";",
            //    141 => "=",
            //    142 => ",",
            //    143 => "-",
            //    144 => ".",
            //    145 => "/",
            //    146 => "`",
            //    149 => "[",
            //    150 => "\\",
            //    151 => "]",
            //    152 => "'",
            //    >= 90 => "",
            //    _ => key.ToString()
            //};

            //txtKeystroke.Text = status == "" ? keyName : status + (keyName == "" ? "" : $" + {keyName}");
            ////txtKeystroke.Text = key.ToString();
            //e.Handled = true;
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

        private void ShowAlert()
        {
            bdAlert.Visibility = Visibility.Visible;
            timer.Stop();
            timer.Start();
        }

        private void BrowseClick(object sender, MouseButtonEventArgs e)
        {
            if (folderBrowserDialog == null)
            {
                if (openFileDialog!.ShowDialog() == true)
                {
                    if (openFileDialog.FileName.Length > 260 || !Utility.IsPathValid(openFileDialog.FileName))
                    {
                        ShowAlert();
                        txtOpen.Text = "";
                        return;
                    }
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
            if (DdpmCommonHelper.DeviceManagerSA == null)
                return;

            _ = DdpmCommonHelper.DeviceManagerSA.FinishKeyCapturePen();
            Task<string> task = DdpmCommonHelper.DeviceManagerSA.KeyCaptureData();
            var keystroke = task.Result;
            DdpmCommonHelper.DeviceManagerSA.DeviceChanged -= DeviceManagerSA_DeviceChanged;
        }
        private static void RestartPenCapture()
        {
            if (DdpmCommonHelper.DeviceManagerSA == null)
                return;

            Task<bool> task = DdpmCommonHelper.DeviceManagerSA.FinishKeyCapturePen();
            _ = task.Result;
            task = DdpmCommonHelper.DeviceManagerSA.StartKeyCapturePen();
            _ = task.Result;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged -= DeviceManagerSA_DeviceChanged;
            }
            StopCapture();
        }
        private void StopCapture()
        {
            if (pType == "PEN")
                StopPenCapture();
            else
            {
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    if (pType == "KB")
                        DdpmCommonHelper.DeviceManagerSA?.StopKeyboardKeystrokeRecording(guid);
                    else if (pType == "MOUSE")
                        DdpmCommonHelper.DeviceManagerSA?.StopMouseKeystrokeRecording(guid);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged += DeviceManagerSA_DeviceChanged;
            }
        }

        private void ActionParameterModalDialog_Deactivated(object? sender, EventArgs e)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null && action == AdvancedAction.AssignKeystroke)
            {
                StopCapture();
            }
        }

        private void ActionParameterModalDialog_Activated(object? sender, EventArgs e)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null && action == AdvancedAction.AssignKeystroke)
            {
                if (pType == "PEN")
                {
                    Task<bool> task = DdpmCommonHelper.DeviceManagerSA.StartKeyCapturePen();
                    _ = task.Result;
                }
                else if (pType == "KB")
                {
                    if (!DdpmCommonHelper.DeviceManagerSA.StartKeyboardKeystrokeRecording(guid).Result)
                    {
                        DdpmCommonHelper.WriteUILog($"Assign KeyStroke Error: Can't StartKeyboardKeystrokeRecording!");
                    }
                }
                else
                {
                    if (!DdpmCommonHelper.DeviceManagerSA.StartMouseKeystrokeRecording(guid).Result)
                    {
                        DdpmCommonHelper.WriteUILog($"Assign KeyStroke Error: Can't StartMouseKeystrokeRecording!");
                    }
                }
            }
        }
    }
}