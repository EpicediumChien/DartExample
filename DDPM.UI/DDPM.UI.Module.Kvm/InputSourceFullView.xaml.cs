using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using DDPM.UI.Common.Method;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.DdpmHomePlugin;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for Test1FullView.xaml
    /// </summary>
    public partial class InputSourceFullView : UserControl
    {
        private double gridTxt_Row1_DefaultFontSize = 20;
        private double gridTxt_Row2_DefaultFontSize = 64;
        private double gridTxt_Row3_DefaultFontSize = 16;

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
            gridTxt_Row1_DefaultFontSize = GridTxt_Row1.FontSize;
            gridTxt_Row2_DefaultFontSize = GridTxt_Row2.FontSize;
            gridTxt_Row3_DefaultFontSize = GridTxt_Row3.FontSize;
            LeftGrid.SizeChanged -= AdjustFontSizeForWWO;
            LeftGrid.SizeChanged += AdjustFontSizeForWWO;
        }

        ~InputSourceFullView()
        {
            LeftGrid.SizeChanged -= AdjustFontSizeForWWO;
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
                vm.UpdateArrow(true);
                vm.CancelSetUSBKVM();
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
                List<InputSourceObj> inputList = new List<InputSourceObj>();
                string inputSource1 = vm.PC1Inputs_Selected.Type;
                if (!string.IsNullOrEmpty(inputSource1))
                {
                    inputList.Add(new InputSourceObj(inputSource1));
                }
                string inputSource2 = vm.PC2Inputs_Selected.Type;
                if (!string.IsNullOrEmpty(inputSource2))
                {
                    inputList.Add(new InputSourceObj(inputSource2));
                }
                string inputSource3 = vm.PC3Inputs_Selected.Type;
                if (!string.IsNullOrEmpty(inputSource3))
                {
                    inputList.Add(new InputSourceObj(inputSource3));
                }
                string inputSource4 = vm.PC4Inputs_Selected.Type;
                if (!string.IsNullOrEmpty(inputSource4))
                {
                    inputList.Add(new InputSourceObj(inputSource4));
                }
                HomeDevice? selectedHomeDevice = vm.KvmModule?.SelectedHomeDevice;
                if (selectedHomeDevice != null && selectedHomeDevice.MonitorInfo != null)
                {
                    vm.UpdateHotkeyData(selectedHomeDevice.MonitorInfo, inputList);
                }
                //Return to DdpmHomePage
                //IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                //console?.ShowPluginById(DDPM.UI.Common.Constants.DdpmHomePluginId);
                DdpmCommonHelper.MyShowPluginManager?.ShowHomePage("GeHomeFirst");
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


        private void AdjustFontSizeForWWO(object sender, RoutedEventArgs e)
        {
            if (GridTxt_Row1 != null && GridTxt_Row2 != null && GridTxt_Row3 != null)
            {
                // Smaller
                Typeface typeface = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                double txtRowMinWidth = GetLongestWordPixelLength(GridTxt_Row1.Text, typeface, GridTxt_Row1.FontSize);
                if (txtRowMinWidth > LeftGrid.ActualWidth)
                {
                    GridTxt_Row1.FontSize -= 2;
                    txtRowMinWidth = GetLongestWordPixelLength(GridTxt_Row1.Text, typeface, GridTxt_Row1.FontSize);
                }
                else if (GridTxt_Row1.FontSize < gridTxt_Row1_DefaultFontSize)
                {
                    // Bigger
                    txtRowMinWidth = GetLongestWordPixelLength(GridTxt_Row1.Text, typeface, GridTxt_Row1.FontSize + 2);
                    if (txtRowMinWidth <= LeftGrid.ActualWidth)
                    {
                        GridTxt_Row1.FontSize += 2;
                    }
                }

                txtRowMinWidth = GetLongestWordPixelLength(GridTxt_Row2.Text, typeface, GridTxt_Row2.FontSize);
                if (txtRowMinWidth > LeftGrid.ActualWidth)
                {
                    GridTxt_Row2.FontSize -= 2;
                    txtRowMinWidth = GetLongestWordPixelLength(GridTxt_Row2.Text, typeface, GridTxt_Row2.FontSize);
                }
                else if (GridTxt_Row2.FontSize < gridTxt_Row2_DefaultFontSize)
                {
                    // Bigger
                    txtRowMinWidth = GetLongestWordPixelLength(GridTxt_Row2.Text, typeface, GridTxt_Row2.FontSize + 2);
                    if (txtRowMinWidth <= LeftGrid.ActualWidth)
                    {
                        GridTxt_Row2.FontSize += 2;
                    }
                }

                txtRowMinWidth = GetLongestWordPixelLength(GridTxt_Row3.Text, typeface, GridTxt_Row3.FontSize);
                if (txtRowMinWidth > LeftGrid.ActualWidth)
                {
                    GridTxt_Row3.FontSize -= 2;
                    txtRowMinWidth = GetLongestWordPixelLength(GridTxt_Row3.Text, typeface, GridTxt_Row3.FontSize);
                }
                else if (GridTxt_Row3.FontSize < gridTxt_Row3_DefaultFontSize)
                {
                    // Bigger
                    txtRowMinWidth = GetLongestWordPixelLength(GridTxt_Row3.Text, typeface, GridTxt_Row3.FontSize + 2);
                    if (txtRowMinWidth <= LeftGrid.ActualWidth)
                    {
                        GridTxt_Row3.FontSize += 2;
                    }
                }
            }
        }

        private double GetLongestWordPixelLength(string text, Typeface typeface, double fontSize)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            // Split the text into words
            string[] words = text.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            double maxPixelWidth = 0;

            foreach (string word in words)
            {
                double wordWidth = 0;

                if (typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface))
                {
                    foreach (char c in word)
                    {
                        if (glyphTypeface.CharacterToGlyphMap.TryGetValue(c, out ushort glyphIndex))
                        {
                            // Calculate width based on advance widths
                            double advanceWidth = glyphTypeface.AdvanceWidths[glyphIndex];
                            wordWidth += advanceWidth * fontSize;
                        }
                    }
                }

                maxPixelWidth = Math.Max(maxPixelWidth, wordWidth);
            }

            return maxPixelWidth;
        }

        private void RDWForWindowSize(object sender, RoutedEventArgs e)
        {
            // To be implement RWD
            vm.FullMode = Visibility.Visible;
        }
    }
}
