using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using DDPM.UI.Common.Method;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.DdpmHomePlugin;
using Dell.Client.Framework.UX.WPF;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for Test1FullView.xaml
    /// </summary>
    public partial class InputSourceFullView : UserControl
    {
        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        public InputSourceFullView()
        {
            InitializeComponent();
        }

        private void OpenMKFullView(object sender, RoutedEventArgs e)
        {
            ConnectMKFullView connectMKFullView = new ConnectMKFullView();
            connectMKFullView.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(connectMKFullView);
            if (vm != null)
            {
                if (vm.ToProgressValue > 3 || vm.ToProgressValue < 0)
                {
                    vm.ToProgressValue = 1;
                }
                vm.FromProgressValue = vm.ToProgressValue;
                vm.ToProgressValue = vm.ToProgressValue + 1;
            }
        }

        private void CloseUSBKVM(object sender, RoutedEventArgs e)
        {
            if (vm != null)
            {
                vm.FromProgressValue = 0;
                vm.ToProgressValue = 1;
            }
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }

        private void SaveInput(object sender, RoutedEventArgs e)
        {
            if (vm != null)
            {
                vm.FinishtoSetPCs();
                //Return to DdpmHomePage
                IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                console?.ShowPluginById(DDPM.UI.Common.Constants.DdpmHomePluginId);
            }
        }

        private void UXTextBox_TextChanged1(object sender, TextChangedEventArgs e)
        {
            TextString textString = new TextString();
            TextBox tb = sender as TextBox;
            string inputText = tb.Text;
            //1 Check if the input is blank or empty
            if (String.IsNullOrWhiteSpace(inputText))
            {
                //TextBox.Text will fill in the InputSOurceKey
                if (vm != null)
                {
                    tb.Text = vm.pcsList["PC1"].InputType;
                    tb.SelectAll();
                }
            }
            //Not blank, will check char
            else if (!textString.CheckChar(tb.Text))
            {
                //Ignore char input if check failed
                return;
            }
            if (vm != null)
            {
                //Robert_Lin, 2024-11-21, If user cleanup content of TextBox, will auto fill in InputSourceKey
                //1 If the TextBox.Text is empty or blank, then will fill with inputsource key
                //2 Not empty, will call CheckChar,
                //2.1 If CheckChar pass, will accet the InputName, and save to settings file
                //vm.items[(int)tb.Tag].InputName = tb.Text;
                vm.pcsList["PC1"].InputName = tb.Text;
            }
        }

        private void UXTextBox_TextChanged2(object sender, TextChangedEventArgs e)
        {
            TextString textString = new TextString();
            TextBox tb = sender as TextBox;
            string inputText = tb.Text;
            //1 Check if the input is blank or empty
            if (String.IsNullOrWhiteSpace(inputText))
            {
                //TextBox.Text will fill in the InputSOurceKey
                if (vm != null)
                {
                    tb.Text = vm.pcsList["PC2"].InputType;
                    tb.SelectAll();
                }
            }
            //Not blank, will check char
            else if (!textString.CheckChar(tb.Text))
            {
                //Ignore char input if check failed
                return;
            }
            if (vm != null)
            {
                //Robert_Lin, 2024-11-21, If user cleanup content of TextBox, will auto fill in InputSourceKey
                //1 If the TextBox.Text is empty or blank, then will fill with inputsource key
                //2 Not empty, will call CheckChar,
                //2.1 If CheckChar pass, will accet the InputName, and save to settings file
                //vm.items[(int)tb.Tag].InputName = tb.Text;
                vm.pcsList["PC2"].InputName = tb.Text;
            }
        }

        private void UXTextBox_TextChanged3(object sender, TextChangedEventArgs e)
        {
            TextString textString = new TextString();
            TextBox tb = sender as TextBox;
            string inputText = tb.Text;
            //1 Check if the input is blank or empty
            if (String.IsNullOrWhiteSpace(inputText))
            {
                //TextBox.Text will fill in the InputSOurceKey
                if (vm != null)
                {
                    tb.Text = vm.pcsList["PC3"].InputType;
                    tb.SelectAll();
                }
            }
            //Not blank, will check char
            else if (!textString.CheckChar(tb.Text))
            {
                //Ignore char input if check failed
                return;
            }
            if (vm != null)
            {
                //Robert_Lin, 2024-11-21, If user cleanup content of TextBox, will auto fill in InputSourceKey
                //1 If the TextBox.Text is empty or blank, then will fill with inputsource key
                //2 Not empty, will call CheckChar,
                //2.1 If CheckChar pass, will accet the InputName, and save to settings file
                //vm.items[(int)tb.Tag].InputName = tb.Text;
                vm.pcsList["PC3"].InputName = tb.Text;
            }
        }

        private void UXTextBox_TextChanged4(object sender, TextChangedEventArgs e)
        {
            TextString textString = new TextString();
            TextBox tb = sender as TextBox;
            string inputText = tb.Text;
            //1 Check if the input is blank or empty
            if (String.IsNullOrWhiteSpace(inputText))
            {
                //TextBox.Text will fill in the InputSOurceKey
                if (vm != null)
                {
                    tb.Text = vm.pcsList["PC4"].InputType;
                    tb.SelectAll();
                }
            }
            //Not blank, will check char
            else if (!textString.CheckChar(tb.Text))
            {
                //Ignore char input if check failed
                return;
            }
            if (vm != null)
            {
                //Robert_Lin, 2024-11-21, If user cleanup content of TextBox, will auto fill in InputSourceKey
                //1 If the TextBox.Text is empty or blank, then will fill with inputsource key
                //2 Not empty, will call CheckChar,
                //2.1 If CheckChar pass, will accet the InputName, and save to settings file
                //vm.items[(int)tb.Tag].InputName = tb.Text;
                vm.pcsList["PC4"].InputName = tb.Text;
            }
        }

        private void UXTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextString textString = new TextString();
            e.Handled = !textString.CheckChar(e.Text);
        }
    }
}
