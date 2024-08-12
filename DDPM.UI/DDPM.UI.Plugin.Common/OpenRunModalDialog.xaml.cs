using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Plugin.Common {
  /// <summary>
  /// OpenRunModalDialog.xaml 的互動邏輯
  /// </summary>
  public partial class OpenRunModalDialog : Window {
    readonly Microsoft.Win32.OpenFileDialog? openFileDialog;

    public string Parameter { get; private set; } = "";
    public int ID { get; private set; } = 0;
    public OpenRunModalDialog(double width, double height, int id = 0, string parameter = "") {
      InitializeComponent();
      this.Width = width;
      this.Height = height;
      ID = id;
      Parameter = parameter;
      if(id == 1) {
        spOpen.Visibility = Visibility.Visible;
        btnBrowse.Visibility = Visibility.Collapsed;
        FilePath.Visibility = Visibility.Visible;
      }
      if(id > 7) {
        svAction.ScrollToVerticalOffset(id * 32);
      }

      txtTitleBar.Text = Strings.OpenRun;
      txtCaption.Text = Strings.OpenRun;
      txtDescription.Text = Strings.OpenRunDesc;
      txtBrowse.Text = $"{Strings.SelectedFile} : \"{parameter}\"";
      openFileDialog = new();
      openFileDialog.FileName = parameter;
      btnCancel.Caption = Strings.Cancel;
      btnSave.Caption = Strings.Save;
      btnBrowse.Caption = Strings.SelectAFile;

      OpenRunItems.ItemsSource = Actions.OpenRunActionsList;
      btnSave.IsEnabled = id > 0;
    }

    private void CancelClick(object sender, MouseButtonEventArgs e) {
      DialogResult = false;
      Close();
    }
    private void SaveClick(object sender, MouseButtonEventArgs e) {
      DialogResult = true;
      Close();
    }

    private void BrowseClick(object sender, MouseButtonEventArgs e) {
      if(openFileDialog!.ShowDialog() == true) {
        Parameter = openFileDialog.FileName;
        txtBrowse.Text = $"{Strings.SelectedFile} : \"{Parameter}\"";
        FilePath.Visibility = Visibility.Visible;
        btnBrowse.Visibility = Visibility.Collapsed;
        btnSave.IsEnabled = true;
      }
    }

    private void ClearBrowse(object sender, MouseButtonEventArgs e) {
      btnBrowse.Visibility = Visibility.Visible;
      FilePath.Visibility = Visibility.Collapsed;
      btnSave.IsEnabled = false;
    }

    private void ActionRadioButton_Click(object sender, RoutedEventArgs e) {
      var rb = (UXRadioButton)sender;
      var id = int.Parse(rb.Name.Replace("Radio", ""));
      if(id == ID) { return; }

      ID = id;
      if(id == 1) {
        spOpen.Visibility =Visibility.Visible;
        if(Parameter == "") {
          btnSave.IsEnabled = false;
          btnBrowse.Visibility = Visibility.Visible;
          FilePath.Visibility = Visibility.Collapsed;
        }
        else {
          btnSave.IsEnabled = true;
          btnBrowse.Visibility = Visibility.Collapsed;
          FilePath.Visibility = Visibility.Visible;
        }
      }
      else {
        Parameter = "";
        spOpen.Visibility = Visibility.Collapsed;
        btnSave.IsEnabled = true;
      }
    }

    private void ActionButtonLoaded(object sender, RoutedEventArgs e) {
      int id;
      if(sender is UXRadioButton rb) {
        id = (int)((UXRadioButton)sender).DataContext;
        rb.Name = $"Radio{id}";
        rb.Content = Actions.OpenRunActions[id];
        rb.IsChecked = ID == id;
      }
    }
  }
}
