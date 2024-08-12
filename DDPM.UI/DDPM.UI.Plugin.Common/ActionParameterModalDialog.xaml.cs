using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DDPM.UI.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Plugin.Common {
  /// <summary>
  /// ActionParameterModalDialog.xaml 的互動邏輯
  /// </summary>
  public partial class ActionParameterModalDialog : Window {
    readonly string Caption = "";
    readonly string Cancel = "Cancel";
    readonly string Clear = "Clear";
    readonly string Save = "Save";
    readonly string Browse = "Browse";

    readonly string AssignKeystroke = "Assign Keystroke";
    readonly string OpenFile = "Open File";
    readonly string OpenFolder = "Open Folder";
    readonly string OpenWebPage = "Open Web Page";
    readonly string AssignKeystrokeDesc = "Enter a key combination to create a shortcut";
    readonly string OpenFileDesc = "Click the browse button to select a file";
    readonly string OpenFolderDesc = "Click the browse button to select a folder";
    readonly string OpenWebPageDesc = "Type the URL to the web page in the box below";
    readonly string OpenFileWaterMark = "File Name";
    readonly string OpenFolderWaterMark = "Folder Name";

    readonly Microsoft.Win32.OpenFileDialog? openFileDialog;
    readonly System.Windows.Forms.FolderBrowserDialog? folderBrowserDialog;

    public string Parameter { get; private set; } = "";
    public ActionParameterModalDialog(AdvancedAction deviceCat, double width, double height, string parameter = "") {
      InitializeComponent();
      this.Width = width;
      this.Height = height;
      

      txtCaption.Text = Caption;
      switch(deviceCat) {
        case AdvancedAction.AssignKeystroke:
          Caption = AssignKeystroke;
          txtDescription.Text = AssignKeystrokeDesc;
          txtKeystroke.Text = parameter;
          btnClear.IsEnabled = parameter != "";
          spKeystroke.Visibility = Visibility.Visible;
          this.PreviewKeyDown += Keystroke_PreviewKeyDown;
          break;
        case AdvancedAction.OpenFile:
          Caption = OpenFile;
          txtDescription.Text = OpenFileDesc;
          txtOpen.Text = parameter;
          spOpen.Visibility = Visibility.Visible;
          btnClear.Visibility = Visibility.Collapsed;
          txtWaterMark.Text = OpenFileWaterMark;
          openFileDialog = new();
          openFileDialog.FileName = parameter;
          break;
        case AdvancedAction.OpenFolder:
          Caption = OpenFolder;
          txtDescription.Text = OpenFolderDesc;
          txtOpen.Text = parameter;
          spOpen.Visibility = Visibility.Visible;
          btnClear.Visibility = Visibility.Collapsed;
          txtWaterMark.Text = OpenFolderWaterMark;
          folderBrowserDialog = new();
          break;
        case AdvancedAction.OpenWebPage:
          Caption = OpenWebPage;
          txtKeystroke.Text = parameter;
          txtDescription.Text = OpenWebPageDesc;
          spKeystroke.Visibility = Visibility.Visible;
          txtKeystroke.IsEnabled = true;
          break;
      }
      txtTitleBar.Text = Caption;
      txtCaption.Text = Caption;
      btnCancel.Caption = Cancel;
      btnClear.Caption = Clear;
      btnSave.Caption = Save;
      btnBrowse.Caption = Browse;
    }

    private void CancelClick(object sender, MouseButtonEventArgs e) {
      DialogResult = false;
      Close();
    }
    private void ClearClick(object sender, MouseButtonEventArgs e) {
      txtKeystroke.Text = "";

      btnClear.IsEnabled = false;
    }
    private void SaveClick(object sender, MouseButtonEventArgs e) {
      DialogResult = true;
      Close();
    }

    private void KeystrokeTextChanged(object sender, TextChangedEventArgs e) {
      if (txtKeystroke.Text == "") {
        btnClear.IsEnabled = false;
        btnSave.IsEnabled = false;
      }
      else {
        btnClear.IsEnabled = true;
        btnSave.IsEnabled = true;
        Parameter = txtKeystroke.Text;
      }
    }
    private void Keystroke_PreviewKeyUp(object sender, KeyEventArgs e) {
      e.Handled = true;
    }
    private void Keystroke_PreviewKeyDown(object sender, KeyEventArgs e) {
      var key = (e.Key == Key.System ? e.SystemKey : e.Key);
      if(key == Key.LWin) { e.Handled = true; return; }
      string status = "";

      if((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
        status += status == "" ? "Control" : " + Control";
      }
      if((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) {
        status += status == "" ? "Alt" : " + Alt";
      }
      if((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
        status += status == "" ? "Shift" : " + Shift";
      }
      //if((Keyboard.Modifiers & ModifierKeys.Windows) == ModifierKeys.Windows) {
      //  status += status == "" ? "Windows" : " + Windows";
      //}

      var keyName = (int)key switch {
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

    private void Clear_txtOpen(object sender, MouseButtonEventArgs e) {
      txtOpen.Text = "";
    }

    private void OpenTextChanged(object sender, TextChangedEventArgs e) {
      if(txtOpen.Text == "") {
        btnSave.IsEnabled = false;
        txtWaterMark.Visibility = Visibility.Visible;
      }
      else {
        btnSave.IsEnabled = true;
        Parameter = txtOpen.Text;
        txtWaterMark.Visibility = Visibility.Collapsed;
      }
    }

    private void BrowseClick(object sender, MouseButtonEventArgs e) {
      if(folderBrowserDialog == null) {
        if(openFileDialog!.ShowDialog() == true) {
          txtOpen.Text = openFileDialog.FileName;
        }
      }
      else {
        folderBrowserDialog.Description = "Select a folder";
        folderBrowserDialog.ShowNewFolderButton = true;

        if(folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
          string folderPath = folderBrowserDialog.SelectedPath;
          txtOpen.Text = folderPath;
        }
      }
    }
  }
}
