using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Input;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// OpenRunModalDialog.xaml 的互動邏輯
    /// </summary>
    public partial class OpenRunModalDialog : Window
    {
        private readonly Microsoft.Win32.OpenFileDialog? openFileDialog;

        public string Parameter { get; private set; } = "";
        //public int ID { get; private set; } = 0;
        private int id;

        List<string> OpenRunApps;

        public OpenRunModalDialog(double width, double height, List<string> openRunApps, string parameter = "")
        {
            InitializeComponent();
            this.Width = width;
            this.Height = height;
            //ID = id;
            Parameter = parameter;
            OpenRunApps = openRunApps;
            id = OpenRunApps.IndexOf(parameter);
            if (parameter.Contains('\\'))
            {
                spOpen.Visibility = Visibility.Visible;
                btnBrowse.Visibility = Visibility.Collapsed;
                FilePath.Visibility = Visibility.Visible;
                txtBrowse.Text = $"{Strings.SelectedFile} : \"{parameter}\"";
            }
            if (id > 6)
            {
                svAction.ScrollToVerticalOffset(id * 32);
            }

            txtTitleBar.Text = Strings.OpenRun;
            txtCaption.Text = Strings.OpenRun;
            txtDescription.Text = Strings.OpenRunDesc;
            openFileDialog = new();
            openFileDialog.FileName = parameter;
            btnCancel.Caption = Strings.Cancel;
            btnSave.Caption = Strings.Save;
            btnBrowse.Caption = Strings.SelectAFile;

            //OpenRunItems.ItemsSource = Actions.OpenRunActionsList;
            OpenRunItems.ItemsSource = OpenRunApps;
            btnSave.IsEnabled = id >= 0;
        }

        private void CancelClick(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveClick(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BrowseClick(object sender, MouseButtonEventArgs e)
        {
            if (openFileDialog!.ShowDialog() == true)
            {
                Parameter = openFileDialog.FileName;
                txtBrowse.Text = $"{Strings.SelectedFile} : \"{Parameter}\"";
                FilePath.Visibility = Visibility.Visible;
                btnBrowse.Visibility = Visibility.Collapsed;
                btnSave.IsEnabled = true;
            }
        }

        private void ClearBrowse(object sender, MouseButtonEventArgs e)
        {
            btnBrowse.Visibility = Visibility.Visible;
            FilePath.Visibility = Visibility.Collapsed;
            btnSave.IsEnabled = false;
        }

        private void ActionRadioButton_Click(object sender, RoutedEventArgs e)
        {
            var rb = (UXRadioButton)sender;
            //var id = int.Parse(rb.Name.Replace("Radio", ""));
            //if (id == ID)
            //{ return; }
            var name = rb.Content.ToString()!;
            if (name == Parameter)
            { return; }

            //ID = id;
            if (name.Length > 2 && name.Substring(name.Length - 3, 3) == "...")
            {
                spOpen.Visibility = Visibility.Visible;
                if (id == 0 || 0==0)
                {
                    btnSave.IsEnabled = false;
                    btnBrowse.Visibility = Visibility.Visible;
                    FilePath.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btnSave.IsEnabled = true;
                    btnBrowse.Visibility = Visibility.Collapsed;
                    FilePath.Visibility = Visibility.Visible;
                }
            }
            else
            {
                //Parameter = "";
                //spOpen.Visibility = Visibility.Collapsed;
                //btnSave.IsEnabled = true;
                Parameter = name;
                spOpen.Visibility = Visibility.Collapsed;
                btnSave.IsEnabled = true;
            }
        }

        private int idx = 0;
        private void ActionButtonLoaded(object sender, RoutedEventArgs e)
        {
            //int id;
            if (sender is UXRadioButton rb)
            {
                //id = (int)((UXRadioButton)sender).DataContext;
                //rb.Name = $"Radio{id}";
                //rb.Content = Actions.OpenRunActions[id];
                //rb.IsChecked = ID == id;
                var name = ((UXRadioButton)sender).DataContext.ToString();
                //rb.Name = $"{name}";
                rb.Content = name;
                rb.IsChecked = name == Parameter || (idx == 0 && Parameter.Contains('\\'));
            }
            idx++;
        }
    }
}