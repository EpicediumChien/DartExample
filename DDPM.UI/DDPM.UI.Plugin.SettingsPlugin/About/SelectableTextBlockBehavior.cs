using Dell.Client.Framework.UX.WPF.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using DDPM.UI.Common;

//Add this class to solve PIMS-313975
namespace DDPM.UI.Plugin.SettingsPlugin
{
    public static class SelectableTextBlockBehavior
    {
        public static readonly DependencyProperty IsSelectableProperty =
            DependencyProperty.RegisterAttached("IsSelectable", typeof(bool), typeof(SelectableTextBlockBehavior), new PropertyMetadata(false, OnIsSelectableChanged));
        public static readonly DependencyProperty CopyTextProperty =
            DependencyProperty.RegisterAttached("CopyText", typeof(string), typeof(SelectableTextBlockBehavior), new PropertyMetadata(string.Empty));
        
                public static bool GetIsSelectable(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSelectableProperty);
        }

        public static void SetIsSelectable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSelectableProperty, value);
        }

        public static string GetCopyText(DependencyObject obj) 
        {
            string temp = string.Empty;
            if(obj is UXTextBlock)
            {
                UXTextBlock tb = (UXTextBlock)obj;
                if (tb.DataContext != null && tb.DataContext is UI_ThirdPartyLicenses)
                {
                    temp = ((UI_ThirdPartyLicenses)tb.DataContext).Title + System.Environment.NewLine + System.Environment.NewLine;
                    temp += ((UI_ThirdPartyLicenses)tb.DataContext).Content;
                    return temp;
                }
            }
            return (string)obj.GetValue(CopyTextProperty); 
        }

        public static void SetCopyText(DependencyObject obj, string value) 
        { 
            obj.SetValue(CopyTextProperty, value); 
        }

        private static void OnIsSelectableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                if ((bool)e.NewValue)
                {
                    textBlock.MouseLeftButtonDown += TextBlock_MouseLeftButtonDown;
                    textBlock.MouseMove += TextBlock_MouseMove;
                    textBlock.MouseLeftButtonUp += TextBlock_MouseLeftButtonUp;
                    textBlock.ContextMenu = CreateContextMenu(textBlock);
                }
                else
                {
                    textBlock.MouseLeftButtonDown -= TextBlock_MouseLeftButtonDown;
                    textBlock.MouseMove -= TextBlock_MouseMove;
                    textBlock.MouseLeftButtonUp -= TextBlock_MouseLeftButtonUp;
                    textBlock.ClearValue(FrameworkElement.ContextMenuProperty);
                }
            }
        }

        private static void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                textBlock.CaptureMouse();
                textBlock.Cursor = Cursors.IBeam;
                var start = textBlock.GetPositionFromPoint(e.GetPosition(textBlock), true);
                if (start != null)
                {
                    var selection = new TextRange(start, start);
                    textBlock.SetValue(TextBoxBase.SelectionBrushProperty, Brushes.Transparent);
                    textBlock.SetValue(TextBoxBase.SelectionOpacityProperty, 0.0);
                    textBlock.Tag = selection;
                }
            }
        }

        private static void TextBlock_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.IsMouseCaptured)
            {
                var selection = (TextRange)textBlock.Tag;
                if (selection != null)
                {
                    var end = textBlock.GetPositionFromPoint(e.GetPosition(textBlock), true);
                    if (end != null)
                    {
                        selection.Select(selection.Start, end);
                        textBlock.SetValue(TextBoxBase.SelectionBrushProperty, Brushes.Blue);
                        textBlock.SetValue(TextBoxBase.SelectionOpacityProperty, 1.0);
                        textBlock.SetValue(TextBoxBase.SelectionTextBrushProperty, Brushes.Black);
                    }
                }
            }
        }

        private static void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.IsMouseCaptured)
            {
                textBlock.ReleaseMouseCapture();
                textBlock.Cursor = Cursors.Arrow;

                var selection = (TextRange)textBlock.Tag;
                if (selection != null)
                {
                    Clipboard.SetText(selection.Text);
                    textBlock.ClearValue(TextBoxBase.SelectionBrushProperty);
                    textBlock.ClearValue(TextBoxBase.SelectionOpacityProperty);
                    textBlock.Tag = null;
                }
            }
        }

        private static ContextMenu CreateContextMenu(TextBlock textBlock)
        {
            var contextMenu = new ContextMenu();
            var copyMenuItem = new MenuItem { Header = Strings.Copy };
            copyMenuItem.Click += (s, e) =>
            {
                var selectedText = textBlock.Tag as string;
                if (!string.IsNullOrEmpty(selectedText))
                {
                    Clipboard.SetText(selectedText);
                }
                else
                {
                    var textToCopy = GetCopyText(textBlock);
                    if (!string.IsNullOrEmpty(textToCopy))
                    {
                        Clipboard.SetText(textToCopy);
                    }
                }
            };
            contextMenu.Items.Add(copyMenuItem); return contextMenu;
        }
    }
}