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
        public EventHandler? SaveButtonClick;

        public EventHandler? CancelButtonClick;
        public string CustomName;
        public List<string> CustomNames;

        #endregion Input/Output

        public SaveCustomWindow()
        {
            InitializeComponent();
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
                CancelButtonClick(this, e);
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            CustomName = cbNames.Text;
            if (SaveButtonClick != null)
            {
                SaveButtonClick(this, e);
            }
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
    }
}