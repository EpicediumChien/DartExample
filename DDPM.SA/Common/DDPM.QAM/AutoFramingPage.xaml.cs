using DDPM.SA.Resources.Helper;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DDPM.QAM
{
    /// <summary>
    /// Interaction logic for AutoFramingPage.xaml
    /// </summary>
    public partial class AutoFramingPage : UserControl
    {
        public AutoFramingPage()
        {
            InitializeComponent();
            if (DdpmCommonHelper.QAMPageViewModel != null)
            {
                DataContext = DdpmCommonHelper.QAMPageViewModel;
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is QAMPageViewModel vm)
            {
                vm.isStatusChangeByDDPM = false;
                vm.SetNoneProfile(); //Derek 2025/01/16
                //DdpmCommonHelper.DeviceManagerSA?.WriteLog($"ToggleButton_Click -> {vm.isStatusChagneByDDPM}");
                vm.SetAutoFramingStatus();

                //Derek 20250111
                var textBlock = (TextBlock)MyToggleButton.Template.FindName("SwitchText", MyToggleButton);
                if (textBlock != null)
                {
                    textBlock.Text = vm.AutoFramingStatus ? LangHelper.Instance["ON"] : LangHelper.Instance["OFF"];
                }
            }    
        }

        private bool IsTextTruncated()
        {
            Typeface typeface = new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch);
            FormattedText formattedText = new FormattedText(tb.Text, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, typeface, tb.FontSize, Brushes.Black);

            if (formattedText.Width > tb.ActualWidth)
                return true;

            return false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //if (IsTextTruncated())
            //    tt.Visibility = Visibility.Visible;
            //else 
            //    tt.Visibility = Visibility.Hidden;
        }
    }
}
