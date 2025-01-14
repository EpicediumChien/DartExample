//using System.Drawing;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// RadialMenuModalDialog.xaml 的互動邏輯
    /// </summary>
    public partial class RadialMenuModalDialog : Window
    {
        PenActions PenActions;
        PenViewModel _vm;

        private const double OuterRadius = 198;
        private const double InnerRadius = 58;
        private const double CenterX = 200;
        private const double CenterY = 200;

        private int SelectedMenuID = 0;
        private int SelectedActionID = 0;
        private bool IsComboOpen = false;
        readonly SolidColorBrush NormalFillBrush = new();
        readonly SolidColorBrush NormalBorderBrush = new();
        readonly LinearGradientBrush FocusFillBrush = new();
        readonly LinearGradientBrush FocusBorderBrush = new();

        public RadialMenuModalDialog(double width, double height, PenViewModel vm)
        {

            try
            {
                InitializeComponent();
                this.Width = width;
                this.Height = height;
                PenActions = vm.PenAction;
                _vm = vm;

                DrawPieChart();

                SelectedMenuID = 0;
                SelectedActionID = PenActions.RadialActions[SelectedMenuID].AssignedAction.ID;
                txtTitleBar.Text = Strings.RadialMenu;
                txtFunction.Text = Strings.FunctionForSelectedRadial;
                RefreshAction(true);
                txtLabel.Text = Strings.LabelForSelectedRadial;
                txtUseCenter.Text = Strings.UseCenterForEmulatingRightClick;
                tsUseCenter.IsChecked = PenActions.IsUseCenter;
                if (PenActions.IsUseCenter)
                {
                    tsUseCenter.Content = Strings.On;
                }
                else
                {
                    tsUseCenter.Content = Strings.Off;
                }
                btnRestore.Caption = Strings.RestoreToDefault;
                btnSave.Caption = Strings.Save;

                NormalFillBrush.Color = Color.FromArgb(0x99, 0x13, 0x2F, 0x54);
                //NormalBorderBrush.Color = Color.FromRgb(0x1E, 0x3F, 0x6C);
                NormalBorderBrush.Color = Color.FromRgb(0x13, 0x2F, 0x54);
                FocusFillBrush.StartPoint = new Point(0, 0);
                FocusFillBrush.EndPoint = new Point(1, 0);
                FocusFillBrush.GradientStops.Add(new GradientStop(Color.FromRgb(0x06, 0x72, 0xCB), 0));
                FocusFillBrush.GradientStops.Add(new GradientStop(Color.FromRgb(0x6E, 0x69, 0xCF), 1));
                FocusBorderBrush.StartPoint = new Point(0, 0);
                FocusBorderBrush.EndPoint = new Point(1, 0);
                FocusBorderBrush.GradientStops.Add(new GradientStop(Color.FromRgb(0x55, 0xB4, 0xFD), 0));
                FocusBorderBrush.GradientStops.Add(new GradientStop(Color.FromRgb(0x6E, 0x69, 0xCF), 1));

                DdpmCommonHelper.BitmapImageUpdated += imgComboImageUpdate;
            }
            catch ( Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\RadialMenuModalDialog.xaml.cs RadialMenuModalDialog ex:" + ex.Message);
            }
        }

        private void imgComboImageUpdate(OSThemeEnum oSThemeEnum)
        {
            imgCombo.Source = null;
            imgCombo.Source = (BitmapImage)System.Windows.Application.Current.Resources["down_Expand"];
        }

        private void DrawPieChart()
        {
            try
            {
                int numberOfSections = 8;
                double angleStep = 360.0 / numberOfSections;

                for (int i = 0; i < numberOfSections; i++)
                {
                    double startAngle = -22.5 - i * angleStep;
                    double endAngle = startAngle + angleStep;

                    // Create a path for each section
                    Path path = new Path
                    {
                        Name = $"Path{i}",
                        Fill = (i == 0) ? FocusFillBrush : NormalFillBrush,
                        Stroke = (i == 0) ? FocusBorderBrush : NormalBorderBrush,
                        StrokeThickness = 1
                    };

                    path.Data = CreatePieSliceGeometry(startAngle, endAngle);
                    path.MouseEnter += Path_MouseEnter;
                    path.MouseLeave += Path_MouseLeave;
                    path.MouseLeftButtonDown += Path_MouseLeftButtonDown;
                    canvas.Children.Add(path);
                }
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\RadialMenuModalDialog.xaml.cs DrawPieChart ex:" + ex.Message);
            }
        }

        private Geometry CreatePieSliceGeometry(double startAngle, double endAngle)
        {
            double startRadians = startAngle * Math.PI / 180;
            double endRadians = endAngle * Math.PI / 180;

            // Calculate points on the outer circle
            Point startOuterPoint = new Point(
                CenterX + OuterRadius * Math.Cos(startRadians),
                CenterY - OuterRadius * Math.Sin(startRadians));
            Point endOuterPoint = new Point(
                CenterX + OuterRadius * Math.Cos(endRadians),
                CenterY - OuterRadius * Math.Sin(endRadians));

            // Calculate points on the inner circle
            Point startInnerPoint = new Point(
                CenterX + InnerRadius * Math.Cos(startRadians),
                CenterY - InnerRadius * Math.Sin(startRadians));
            Point endInnerPoint = new Point(
                CenterX + InnerRadius * Math.Cos(endRadians),
                CenterY - InnerRadius * Math.Sin(endRadians));

            // Create the path figure
            PathFigure figure = new PathFigure { StartPoint = startOuterPoint };
            figure.Segments.Add(new ArcSegment(endOuterPoint, new Size(OuterRadius, OuterRadius), 0, false, SweepDirection.Counterclockwise, true));
            figure.Segments.Add(new LineSegment(endInnerPoint, true));
            figure.Segments.Add(new ArcSegment(startInnerPoint, new Size(InnerRadius, InnerRadius), 0, false, SweepDirection.Clockwise, true));
            figure.Segments.Add(new LineSegment(startOuterPoint, true));

            return new PathGeometry(new[] { figure });
        }

        private void BacklClick(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ComboButtonClick(object sender, MouseButtonEventArgs e)
        {
            if (IsComboOpen)
            {
                CloseActionCombo();
            }
            else
            {
                OpenActionCombo();
            }
        }

        private void ActionRadioButton_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                var rb = (UXRadioButton)sender;
                var id = int.Parse(rb.Name.Replace("Radio", ""));
                if (id == SelectedActionID && id != 8 && id != 23)
                { return; }

                txtLabelText.Visibility = Visibility.Visible;
                var parameter = "";
                if (id == 8)
                {
                    Window parentWindow = Window.GetWindow(this);
                    double windowLeft = 0;
                    double windowTop = 0;
                    ActionParameterModalDialog modalDialog = new(AdvancedAction.AssignKeystroke, parentWindow.ActualWidth, parentWindow.ActualHeight);
                    if (parentWindow != null)
                    {
                        modalDialog.Owner = parentWindow;
                        windowLeft = parentWindow.Left;
                        windowTop = parentWindow.Top;
                    }
                    modalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                    modalDialog.Left = windowLeft;
                    modalDialog.Top = windowTop;
                    if (modalDialog.ShowDialog()!.Value)
                    {
                        parameter = modalDialog.Parameter;
                        PenActions.RadialLabels[SelectedMenuID] = parameter;
                        spLabel.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        RefreshAction();
                        CloseActionCombo();
                        return;
                    }
                }
                else if (id == 23)
                {
                    Window parentWindow = Window.GetWindow(this);
                    double windowLeft = 0;
                    double windowTop = 0;
                    OpenRunModalDialog modalDialog = new(parentWindow.ActualWidth, parentWindow.ActualHeight, _vm.LaunchableAppValues, PenActions.RadialActions[SelectedMenuID].AssignedAction.Parameter);
                    if (parentWindow != null)
                    {
                        modalDialog.Owner = parentWindow;
                        windowLeft = parentWindow.Left;
                        windowTop = parentWindow.Top;
                    }
                    modalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                    modalDialog.Left = windowLeft;
                    modalDialog.Top = windowTop;
                    if (modalDialog.ShowDialog()!.Value)
                    {
                        //parameter = $"{modalDialog.ID}|{modalDialog.Parameter}";
                        //if (modalDialog.ID == 1)
                        //{
                        //    PenActions.RadialLabels[SelectedMenuID] = modalDialog.Parameter;
                        //}
                        //else
                        //{
                        //    //PenActions.RadialLabels[SelectedMenuID] = Actions.OpenRunActions[modalDialog.ID];
                        //    PenActions.RadialLabels[SelectedMenuID] = parameter;
                        //}
                        parameter = $"{modalDialog.Parameter}";
                        PenActions.RadialLabels[SelectedMenuID] = parameter;
                        spLabel.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        RefreshAction();
                        CloseActionCombo();
                        return;
                    }
                }
                else
                {
                    PenActions.RadialLabels[SelectedMenuID] = Actions.RadialMenuActions[id].Caption;
                    spLabel.Visibility = Visibility.Visible;
                }

                SelectedActionID = id;
                txtMenu.Text = rb.Content.ToString();
                CloseActionCombo();
                PenActions.RadialActions[SelectedMenuID].AssignedAction.ID = SelectedActionID;
                PenActions.RadialActions[SelectedMenuID].AssignedAction.Parameter = parameter;
                _vm.UpdateRadialMenu(SelectedMenuID, SelectedActionID);
                ActionList.ExportActionList(PenActions, "PEN");
                RefreshAction();
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\RadialMenuModalDialog.xaml.cs ActionRadioButton_Click ex:" + ex.Message);
            }
        }

        private void ActionButtonLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                int id;
                if (sender is UXRadioButton rb)
                {
                    id = (int)((UXRadioButton)sender).DataContext;
                    rb.Name = $"Radio{id}";
                    //rb.Content = Actions.RadialMenuActions[id].Caption;
                    rb.Content = _vm.ActionNames[id];
                    rb.IsChecked = id == SelectedActionID;
                }
                else if (sender is ActionButton btn)
                {
                    id = (int)((ActionButton)sender).DataContext;
                    if ((id == 8 || id == 23) && id == SelectedActionID)
                    {
                        btn.Name = $"btn{id}";
                        btn.Caption = Strings.Edit;
                        btn.Visibility = Visibility.Visible;
                        btn.Width = 101;
                    }
                    else
                    {
                        btn.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\RadialMenuModalDialog.xaml.cs ActionButtonLoaded ex:" + ex.Message);
            }
        }

        void RefreshAction(bool all = false)
        {
            try
            {
                //txtMenu.Text = Actions.RadialMenuActions[SelectedActionID].Caption;
                txtMenu.Text = _vm.ActionNames[SelectedActionID];
                txtLabelText.Text = PenActions.RadialLabels[SelectedMenuID];
                var index = _vm.RadialMenuActions.IndexOf(SelectedActionID);
                MenuItems.ItemsSource = null;
                //MenuItems.ItemsSource = Actions.RadialMenuActionsList;
                MenuItems.ItemsSource = _vm.RadialMenuActions;
                //if (SelectedActionID > 7)
                if (index > 6)
                {
                    svMenu.ScrollToVerticalOffset(index * 29);
                }
                RefreshLabel(all);
                if (SelectedActionID == 8 || SelectedActionID == 23)
                {
                    spLabel.Visibility = Visibility.Collapsed;
                }
                else
                {
                    spLabel.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\RadialMenuModalDialog.xaml.cs RefreshAction ex:" + ex.Message);
            }
        }
        void RefreshLabel(bool all = false)
        {
            try
            {
                if (all)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        var tb = (UXTextBlock)FindName($"Label{i}");
                        tb.Text = CheckLabel(PenActions.RadialLabels[i], i);
                    }
                }
                else
                {
                    var tb = (UXTextBlock)FindName($"Label{SelectedMenuID}");
                    tb.Text = CheckLabel(PenActions.RadialLabels[SelectedMenuID], SelectedMenuID);
                }
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\RadialMenuModalDialog.xaml.cs RefreshLabel ex:" + ex.Message);
            }
        }
        string CheckLabel(string text, int id)
        {
            double width = id switch
            {
                0 or 4 => 130,
                1 or 3 or 5 or 7 => 136,
                2 or 6 => 125,
                _ => 0
            };
            var typeface = new Typeface(new FontFamily("Roboto"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentUICulture,
                System.Windows.FlowDirection.LeftToRight,
                typeface,
                16,
                Brushes.Black,
                new NumberSubstitution(),
                1.0);
            if (formattedText.Width <= width)
            { return text; }

            while (formattedText.Width > width)
            {
                text = text.Substring(0, text.Length - 2);
                formattedText = new FormattedText(
                  $"{text}...",
                  System.Globalization.CultureInfo.CurrentUICulture,
                  System.Windows.FlowDirection.LeftToRight,
                  typeface,
                  16,
                  Brushes.Black,
                  new NumberSubstitution(),
                  1.0);
            }
            return $"{text}...";
        }
        void OpenActionCombo()
        {
            MenuPanel.Visibility = Visibility.Visible;

            DoubleAnimation visibilityAnimation = new()
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(0.3))
            };
            MenuPanel.BeginAnimation(DockPanel.OpacityProperty, visibilityAnimation);

            imgCombo.RenderTransform = new RotateTransform();
            DoubleAnimation rotateAnimation = new()
            {
                From = 0,
                To = 180,
                Duration = new Duration(TimeSpan.FromSeconds(0.3)),
            };
            imgCombo.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
            SectionB.Visibility = Visibility.Collapsed;
            IsComboOpen = true;
        }

        void CloseActionCombo()
        {
            imgCombo.RenderTransform = new RotateTransform();
            DoubleAnimation rotateAnimation = new()
            {
                From = 180,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.3)),
            };
            imgCombo.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
            MenuPanel.Visibility = Visibility.Collapsed;
            SectionB.Visibility = Visibility.Visible;
            IsComboOpen = false;
        }

        private void LabelTextChanged(object sender, TextChangedEventArgs e)
        {
            btnSave.IsEnabled = (txtLabelText.Text != PenActions.RadialLabels[SelectedMenuID] && txtLabelText.Text.Trim() != "");
        }

        private void EditActionClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var id = int.Parse(((ActionButton)sender).Name.Replace("btn", ""));
                var parameter = PenActions.RadialLabels[SelectedMenuID];

                Window parentWindow = Window.GetWindow(this);
                double windowLeft = 0;
                double windowTop = 0;
                if (id == 8)
                {
                    ActionParameterModalDialog modalDialog = new(AdvancedAction.AssignKeystroke, parentWindow.ActualWidth, parentWindow.ActualHeight, parameter);
                    if (parentWindow != null)
                    {
                        modalDialog.Owner = parentWindow;
                        windowLeft = parentWindow.Left;
                        windowTop = parentWindow.Top;
                    }
                    modalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                    modalDialog.Left = windowLeft;
                    modalDialog.Top = windowTop;
                    if (modalDialog.ShowDialog()!.Value && modalDialog.Parameter != parameter)
                    {
                        parameter = modalDialog.Parameter;
                        PenActions.RadialLabels[SelectedMenuID] = parameter;
                    }
                    else
                    {
                        CloseActionCombo();
                        return;
                    }
                }
                else if (id == 23)
                {
                    parameter = PenActions.RadialActions[SelectedMenuID].AssignedAction.Parameter;
                    //var arr = parameter.Split('|');
                    //OpenRunModalDialog modalDialog = new(parentWindow.ActualWidth, parentWindow.ActualHeight, int.Parse(arr[0]), arr[1]);
                    OpenRunModalDialog modalDialog = new(parentWindow.ActualWidth, parentWindow.ActualHeight, _vm.LaunchableAppValues, parameter);
                    if (parentWindow != null)
                    {
                        modalDialog.Owner = parentWindow;
                        windowLeft = parentWindow.Left;
                        windowTop = parentWindow.Top;
                    }
                    modalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                    modalDialog.Left = windowLeft;
                    modalDialog.Top = windowTop;
                    //if (modalDialog.ShowDialog()!.Value && $"{modalDialog.ID}|{modalDialog.Parameter}" != parameter)
                    if (modalDialog.ShowDialog()!.Value && $"{modalDialog.Parameter}" != parameter)
                    {
                        //parameter = $"{modalDialog.ID}|{modalDialog.Parameter}";
                        //if (modalDialog.ID == 1)
                        //{
                        //    PenActions.RadialLabels[SelectedMenuID] = modalDialog.Parameter;
                        //}
                        //else
                        //{
                        //    //PenActions.RadialLabels[SelectedMenuID] = Actions.OpenRunActions[modalDialog.ID];
                        //    PenActions.RadialLabels[SelectedMenuID] = _vm.LaunchableAppValues[modalDialog.ID];
                        //}
                        parameter = $"{modalDialog.Parameter}";
                        PenActions.RadialLabels[SelectedMenuID] = modalDialog.Parameter;
                    }
                    else
                    {
                        CloseActionCombo();
                        return;
                    }
                }
                PenActions.RadialActions[SelectedMenuID].AssignedAction.ID = SelectedActionID;
                PenActions.RadialActions[SelectedMenuID].AssignedAction.Parameter = parameter;
                _vm.UpdateRadialMenu(SelectedMenuID, SelectedActionID);
                ActionList.ExportActionList(PenActions, "PEN");
                RefreshAction();
                CloseActionCombo();
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\RadialMenuModalDialog.xaml.cs EditActionClick ex:" + ex.Message);
            }
        }

        private void RestoreClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                PenActions.RestoreRadialMenu();
                //ActionList.ExportActionList(PenActions, "PEN");
                SelectedActionID = PenActions.RadialActions[SelectedMenuID].AssignedAction.ID;
                RefreshAction(true);
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\RadialMenuModalDialog.xaml.cs RestoreClick ex:" + ex.Message);
            }
        }

        private void SaveClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                PenActions.RadialLabels[SelectedMenuID] = txtLabelText.Text.Trim();
                //_vm.UpdateRadialMenu(SelectedMenuID, SelectedActionID, parameter);
                _vm.UpdateRadialMenu(SelectedMenuID, SelectedActionID);
                ActionList.ExportActionList(PenActions, "PEN");
                RefreshLabel();
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\RadialMenuModalDialog.xaml.cs SaveClick ex:" + ex.Message);
            }
        }

        private void tsUseCenter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tsUseCenter.IsChecked!.Value)
                {
                    tsUseCenter.Content = Strings.On;
                    imgCenter.Visibility = Visibility.Visible;
                }
                else
                {
                    tsUseCenter.Content = Strings.Off;
                    imgCenter.Visibility = Visibility.Collapsed;
                }
                PenActions.IsUseCenter = tsUseCenter.IsChecked!.Value;
                _vm.UpdateRadialMenuRightClick(PenActions.IsUseCenter);
                ActionList.ExportActionList(PenActions, "PEN");
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\RadialMenuModalDialog.xaml.cs tsUseCenter_Click ex:" + ex.Message);
            }
        }

        private void Path_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (sender is Path path)
                {
                    path.Fill = FocusFillBrush;
                    path.Stroke = FocusBorderBrush;
                }
                if (sender is UXTextBlock tb)
                {
                    int id = int.Parse(tb.Name.Substring(5, 1));
                    var pa = (Path)canvas.Children[id];
                    pa.Fill = FocusFillBrush;
                    pa.Stroke = FocusBorderBrush;
                }
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\RadialMenuModalDialog.xaml.cs Path_MouseEnter ex:" + ex.Message);
            }
        }

        private void Path_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                if (sender is Path path)
                {
                    int id = int.Parse(path.Name.Substring(4, 1));
                    if (id == SelectedMenuID)
                    { return; }
                    path.Fill = NormalFillBrush;
                    path.Stroke = NormalBorderBrush;
                }
                if (sender is UXTextBlock tb)
                {
                    int id = int.Parse(tb.Name.Substring(5, 1));
                    if (id == SelectedMenuID)
                    { return; }
                    var pa = (Path)canvas.Children[id];
                    pa.Fill = NormalFillBrush;
                    pa.Stroke = NormalBorderBrush;
                }
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\RadialMenuModalDialog.xaml.cs Path_MouseLeave ex:" + ex.Message);
            }
        }

        private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Path path)
                {
                    int id = int.Parse(path.Name.Substring(4, 1));
                    if (id == SelectedMenuID)
                    { return; }

                    var pa = (Path)canvas.Children[SelectedMenuID];
                    pa.Fill = NormalFillBrush;
                    pa.Stroke = NormalBorderBrush;

                    path.Fill = FocusFillBrush;
                    path.Stroke = FocusBorderBrush;

                    SelectedMenuID = id;
                }
                if (sender is UXTextBlock tb)
                {
                    int id = int.Parse(tb.Name.Substring(5, 1));
                    if (id == SelectedMenuID)
                    { return; }

                    var pa = (Path)canvas.Children[SelectedMenuID];
                    pa.Fill = NormalFillBrush;
                    pa.Stroke = NormalBorderBrush;

                    pa = (Path)canvas.Children[id];
                    pa.Fill = FocusFillBrush;
                    pa.Stroke = FocusBorderBrush;

                    SelectedMenuID = id;
                }
                SelectedActionID = PenActions.RadialActions[SelectedMenuID].AssignedAction.ID;
                RefreshAction();
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\RadialMenuModalDialog.xaml.cs Path_MouseLeave ex:" + ex.Message);
            }
        }
    }
}
