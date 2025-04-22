using Dell.Client.Framework.UX.WPF.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace DDPM.UI.Common
{
    public static partial class DdpmCommonHelper
    {
        /// <summary>
        /// Switch to light mode color define
        /// </summary>
        /// <history>
        /// Add SplitListView - EzBtn (Robert_Lin, 2024-10-24)
        /// Add regions and sort functions (Eric Chien, 2024-10-30)
        /// </history>
        public static void SwitchToLightMode()
        {
            try
            {
                #region Background Color
                UpdateFreezable("DDPMBackground", (ref LinearGradientBrush brush) =>
                {
                    brush.StartPoint = new System.Windows.Point(0.913, 0.407);
                    brush.EndPoint = new System.Windows.Point(0.0, 1.0);
                    brush.Opacity = 0.05;
                    GradientStop? gs1 = brush.GradientStops.FirstOrDefault(gs => gs.Offset == 0.0537);
                    if (gs1 != null)
                        gs1.Color = (Color)ColorConverter.ConvertFromString("#80306CC7");
                    GradientStop? gs2 = brush.GradientStops.FirstOrDefault(gs => gs.Offset == 0.2371);
                    if (gs2 != null)
                        gs2.Color = (Color)ColorConverter.ConvertFromString("#800063B8");
                    GradientStop? gs3 = brush.GradientStops.FirstOrDefault(gs => gs.Offset == 0.504);
                    if (gs3 != null)
                        gs3.Color = (Color)ColorConverter.ConvertFromString("#E0003ACE");
                    GradientStop? gs4 = brush.GradientStops.FirstOrDefault(gs => gs.Offset == 0.7923);
                    if (gs4 != null)
                        gs4.Color = (Color)ColorConverter.ConvertFromString("#80674AB8");
                    GradientStop? gs5 = brush.GradientStops.FirstOrDefault(gs => gs.Offset == 0.9724);
                    if (gs5 != null)
                        gs5.Color = (Color)ColorConverter.ConvertFromString("#800063B8");
                });
                #endregion

                #region Default Text Color
                UpdateFreezable("DefaultTheme_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF0E0E0E"));
                UpdateFreezable("DefaultTheme_ProcessBarTxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF0E0E0E"));
                UpdateFreezable("DefaultTheme_BdBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99FFFFFF"));
                UpdateFreezable("DefaultTheme_BdSolidBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFF8FBFE"));
                UpdateFreezable("DefaultTheme_BdBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));
                UpdateFreezable("DefaultTheme_BdSolidBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#C5D4E3"));
                UpdateFreezable("DefaultTheme_RbBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#80FFFFFF"));
                UpdateFreezable("DefaultTheme_PathColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("DefaultTheme_PbHeaderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                UpdateFreezable("DefaultTheme_USBKVMPCColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D"));
                UpdateFreezable("DefaultTheme_USBKVMLineColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                UpdateFreezable("DefaultTheme_BtFgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                UpdateFreezable("DefaultTheme_BtBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D9E1F2"));
                UpdateFreezable("DefaultTheme_BtBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                UpdateFreezable("DefaultTheme_ComboBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("DefaultTheme_ComboBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D2D2D2"));
                UpdateBitmapImage("popup_ArrowCorner", new Uri($"pack://application:,,,/DDPM.UI.Common;component/Resources/LightMode/popup_ArrowCorner.png", UriKind.RelativeOrAbsolute));
                UpdateFreezable("DefaultTheme_TbBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                UpdateFreezable("DefaultTheme_TbFgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF0E0E0E"));
                UpdateFreezable("DefaultTheme_TbBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                UpdateFreezable("DefaultTheme_ListBoxBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99E7EAEE"));
                UpdateFreezable("DefaultTheme_DialogBoxBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("DefaultThemeRightViewAlertBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
                #endregion

                #region Add Device
                UpdateFreezable("RightMenuBg_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));
                UpdateFreezable("txtCaption_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                UpdateFreezable("ActionsTextBlock_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                UpdateFreezable("ActionsBorder_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f6f9fc"));
                UpdateFreezable("ActionsRadio_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F0F3FA"));
                UpdateFreezable("ActionsRadio_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));
                UpdateFreezable("ActionsRadio_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                UpdateFreezable("SearchBoxBorder_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));
                UpdateFreezable("SearchTextBox_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));
                UpdateFreezable("SearchTextBox_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#404040"));
                UpdateFreezable("SeachResultBorder_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));
                UpdateFreezable("ModelNumberText_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D")); //PIMS-348201, change from #185896
                UpdateFreezable("KeyToolTipText_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("KeyToolTipText_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1D2C3B"));
                UpdateFreezable("ActionParameterModalDialog_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#e3eef5"));
                UpdateFreezable("CaptionText_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                UpdateFreezable("DescriptionText_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                UpdateFreezable("Keystroke_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("Keystroke_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
                UpdateFreezable("txtOpen_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("txtOpen_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
                UpdateFreezable("CancelButton_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00FFFFFF"));
                UpdateFreezable("CancelButton_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
                UpdateFreezable("CancelButtonBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                UpdateFreezable("SaveButtonDisa_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#B6B6B6"));
                UpdateFreezable("SaveButtonDisa_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("txtActionCaption_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                UpdateBitmapImage("title_back", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/arrow-left-light.png", UriKind.RelativeOrAbsolute));
                UpdateBitmapImage("mouse_setting_title_back", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/arrow-left-light.png", UriKind.RelativeOrAbsolute));
                UpdateBitmapImage("down_Expand", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/Down-light.png", UriKind.RelativeOrAbsolute));
                UpdateFreezable("txtMessage_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                UpdateFreezable("MouseSettingMenu_BgColor99", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99f5f8f9"));
                UpdateFreezable("MouseSettingMenu_BgColor60", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#60f5f8f9"));
                UpdateFreezable("MenuSeperatorColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E1E6EF"));

                UpdateFreezable("AddDeviceTxtCaption_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                UpdateFreezable("AddDeviceBorder1_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99f5f8f9"));
                UpdateFreezable("AddDeviceTextStyle1_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));

                UpdateFreezable("RightView_Header_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("RightView_Header_Text_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0A2343"));
                UpdateFreezable("RightView_Header_Text_HighlightColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0A2343"));

                UpdateFreezable("bdrAlert_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));

                UpdateFreezable("bdrAlert_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));


                UpdateFreezable("ModuleCaption_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                UpdateFreezable("ModuleItem_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));

                UpdateFreezable("txtUnpair_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));

                UpdateBitmapImage("Search_img", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/Search_light.png", UriKind.RelativeOrAbsolute));
                UpdateBitmapImage("Close_img", new Uri($"pack://application:,,,/DDPM.UI.Common;component/Resources/Close_light.png", UriKind.RelativeOrAbsolute));

                UpdateFreezable("ActionParameterModalDialogTextBoxBorderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));

                UpdateFreezable("ActionsBorderBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));

                UpdateFreezable("Border1BorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));

                UpdateFreezable("SearchBoxBorderBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));

                UpdateBitmapImage("left_img", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/Left_light.png", UriKind.RelativeOrAbsolute));
                #endregion

                #region dialog
                //Dialog start
                UpdateFreezable("BorderStyle1_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));

                UpdateFreezable("TextBlockStyle1_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));

                UpdateFreezable("LabelStyle1_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#040404"));

                UpdateFreezable("RadialMenuModalDialogToggleSwitch_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));

                UpdateFreezable("txtLabelText_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("txtLabelText_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));

                UpdateFreezable("PairTileModalDialogTextStyle1FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));

                UpdateFreezable("PairTileModalDialogBgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));

                UpdateFreezable("UnpairModalDialogBorder1", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));

                UpdateFreezable("BorderContinueBackground", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
                UpdateFreezable("BorderContinueBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E1C2F"));

                UpdateFreezable("BorderYesBackground", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
                UpdateFreezable("BorderYesBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E1C2F"));

                UpdateFreezable("txtAlert_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("txtAlert_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#364861"));

                UpdateFreezable("WaitingModalDialogBorderBackground", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E0f4f7f9"));
                UpdateFreezable("WaitingModalDialogBorderBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D8E1EB"));

                UpdateFreezable("bdrAlertBackground", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#616130"));
                UpdateFreezable("WaitingModalDialogBorderBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF8040"));
                //Dialog end
                #endregion

                #region keyboard
                //UpdateFreezable("RightMenuBg_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));
                UpdateFreezable("txtCaption_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                UpdateFreezable("ActionsTextBlock_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                UpdateFreezable("ActionsGrid_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f7fafc"));

                //UpdateFreezable("ActionsBorder_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f7fafc"));

                UpdateFreezable("SearchBoxBorder_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#f4f7f9"));

                #endregion

                #region Main Window Background
                SplashPath = "Resources/Images/splash{0}-round_Light.png";
                UpdateFreezable("mainWindowBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFECF3F9"));
                UpdateFreezable("bkImage", (ref ImageBrush brush) =>
                {
                    brush.Stretch = Stretch.Fill;
                    brush.ImageSource = GetImageSourceFromCommonResource("Resources/Images/Background_Light.png", "DDPM");
                });
                UpdateFreezable("SpinnerWaitTxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("SpinnerBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                #endregion

                #region Add Device
                #endregion

                #region PathIcon
                UpdateFreezable("PathIcon_Color", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#434646"));
                UpdateFreezable("FakeTransparent", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#01000000"));
                UpdateFreezable("MouseHover_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("Window_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                #endregion

                #region HomePage
                UpdateFreezable("HomePageFeature_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#2460B4"));
                UpdateFreezable("HomePageKeyFeature_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("HomePageButton_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("HomePageButton_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                #endregion

                #region Battery
                UpdateFreezable("BatteryIndicator_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                UpdateFreezable("TextBlock_ForegroundColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                #endregion

                #region Display port icon
                UpdateFreezable("DisplayName_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D"));
                UpdateBitmapImage("ConnectionTypeImageKey", new Uri("pack://application:,,,/DDPM.UI.Common;component/Resources/LightMode/Port.png", UriKind.Absolute));
                #endregion

                #region Combo Box
                UpdateFreezable("DdpmCB_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFD2D2D2"));
                UpdateFreezable("DdpmCB_BdBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFE1EAF3"));
                UpdateFreezable("DdpmCB_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                UpdateFreezable("DdpmCB_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("DdpmCB_HoverItemBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#94DCF7"));
                UpdateFreezable("DdpmCB_SelTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0076CE"));
                UpdateFreezable("MouseCB_HIBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E1E1E1"));
                UpdateFreezable("DdpmCB_BdColor_Checked", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                UpdateFreezable("DdpmCB_FgColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#B6B6B6"));
                #endregion

                #region BarItem Colors
                UpdateBitmapImage("Arrow_Left", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/arrow-left-light.png", UriKind.RelativeOrAbsolute));
                UpdateFreezable("btn_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#2B6FC7"));
                #endregion

                #region VbarItem
                System.Windows.Application.Current.Resources["Vbar_BdColor_Hover"] = Color.FromArgb(0xFF, 0xE5, 0xEC, 0xF9);
                System.Windows.Application.Current.Resources["Vbar_BkColor_Hover"] = Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0);
                UpdateFreezable("Vbar_BkBrush_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F0F0F0"));
                #endregion

                #region VberItem1
                //Robert_Lin 20250312 Remove duplicate  (with Line# 229) item
                //UpdateFreezable("DdpmCB_HoverItemBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFF0F0F0"));
                UpdateFreezable("DefaultTheme_USBKVMComboBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5FFFFFF"));
                UpdateFreezable("DefaultTheme_USBKVMComboBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E1E6EF"));
                UpdateFreezable("Vbar_FgBrush_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("Vbar_BkBrush_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99FFFFFF"));
                UpdateFreezable("Vbar_BdBrush_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99FFFFFF"));
                UpdateFreezable("Vbar_FgBrush_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#9E9E9E"));
                UpdateFreezable("Vbar_TextColor_Normal", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("Vbar_TextColor_Selected", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                #endregion

                #region Right View Container
                UpdateFreezable("RightView_Header_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FBFDFF"));
                UpdateFreezable("RightView_Header_Text_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("RightViewContainerBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("GroupExpanderHeaderBottomLineColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFE1E6EF"));
                UpdateFreezable("RightView_Group_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99FFFFFF"));
                UpdateFreezable("RightView_Group_FirstBdBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99FFFFFF"));
                UpdateFreezable("RightView_Group_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));
                UpdateFreezable("RightView_Combo_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("RightView_Combo_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("RightView_Combo_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));
                UpdateFreezable("RightBaseBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFF5F6F7"));
                UpdateFreezable("RightView_PercentageTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00468B"));
                UpdateFreezable("Brightness_TimeText_ForegroundColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00468B"));
                UpdateFreezable("Brightness_TimeAMPM_ForegroundColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("RightView_Btn_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("RightView_Btn_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("RightGroupBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99F8FbFE"));
                UpdateFreezable("GroupBorderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F8FBFE"));
                UpdateFreezable("PartBorderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                #endregion

                #region Tooltip (Robert_Lin, 2024-10-24)
                UpdateFreezable("Tooltip_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("Tooltip_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E9E9E9"));
                UpdateFreezable("Tooltip_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                #endregion

                #region (i) InfoIcon
                UpdateFreezable("InfoIconColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#9D9D9D"));
                #endregion

                #region Button
                UpdateFreezable("Border_Btn_txt", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
                UpdateFreezable("CheckBox_Brush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7E7E7E"));
                UpdateFreezable("BorderBrushColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));
                UpdateFreezable("Button_UXStyleColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
                UpdateFreezable("Button_UXStyleTxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("Button_UXStyleColor_ForWhiteFrame", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
                UpdateFreezable("Button1BorderDisColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#80B6B6B6"));
                UpdateFreezable("Button1TextDisColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#B6B6B6"));
                #endregion

                #region PrimaryButton
                //PrimaryButton: No border, only Disabled state are different color between Dark and Light theme

                UpdateFreezable("PrimaryButton_BkColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
                UpdateFreezable("PrimaryButton_BkColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                UpdateFreezable("PrimaryButton_BkColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00468B"));
                UpdateFreezable("PrimaryButton_BkColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7F5C5C5C"));

                UpdateFreezable("PrimaryButton_TextColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("PrimaryButton_TextColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("PrimaryButton_TextColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("PrimaryButton_TextColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7F7C7C7C"));
                #endregion PrimaryButton


                #region SecondaryButton
                UpdateFreezable("SecondaryButton_BkColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#01FFFFFF"));
                UpdateFreezable("SecondaryButton_BkColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D9F5FD"));
                UpdateFreezable("SecondaryButton_BkColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#94DCF7"));
                UpdateFreezable("SecondaryButton_BkColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00FFFFFF"));

                UpdateFreezable("SecondaryButton_BdColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                UpdateFreezable("SecondaryButton_BdColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                UpdateFreezable("SecondaryButton_BdColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00468B"));
                UpdateFreezable("SecondaryButton_BdColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A3A3A3"));

                UpdateFreezable("SecondaryButton_TextColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                UpdateFreezable("SecondaryButton_TextColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                UpdateFreezable("SecondaryButton_TextColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                UpdateFreezable("SecondaryButton_TextColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D"));

                //UpdateFreezable("SecondaryButton_DefaultBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#01FFFFFF"));
                //UpdateFreezable("SecondaryButton_DefaultBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                //UpdateFreezable("SecondaryButton_DefaultTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));

                //UpdateFreezable("SecondaryButton_HoverBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D9F5FD"));
                //UpdateFreezable("SecondaryButton_HoverBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                //UpdateFreezable("SecondaryButton_HoverTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));

                //UpdateFreezable("SecondaryButton_PressedBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#94DCF7"));
                //UpdateFreezable("SecondaryButton_PressedBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00468B"));
                //UpdateFreezable("SecondaryButton_PressedTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));

                //UpdateFreezable("SecondaryButton_DisabledBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00FFFFFF"));
                //UpdateFreezable("SecondaryButton_DisabledBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A3A3A3"));
                //UpdateFreezable("SecondaryButton_DisabledTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D"));
                #endregion

                #region DestructiveButton
                UpdateFreezable("DestructiveButton_BkColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D0353F"));
                UpdateFreezable("DestructiveButton_BkColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF3E3B"));
                UpdateFreezable("DestructiveButton_BkColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D12A3D"));
                UpdateFreezable("DestructiveButton_BkColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7F5C5C5C"));

                UpdateFreezable("DestructiveButton_TextColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("DestructiveButton_TextColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F4FFFF"));
                UpdateFreezable("DestructiveButton_TextColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("DestructiveButton_TextColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7F7C7C7C"));
                #endregion DestructiveButton

                #region SplitListView - EzBtn (Robert_Lin, 2024-10-24)
                UpdateFreezable("EzBtn_BkColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("EzBtn_BkColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#B6B6B6"));

                UpdateFreezable("EzBtn_BdColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F0F0F0"));
                UpdateFreezable("EzBtn_BdColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F0F0F0"));
                UpdateFreezable("EzBtn_BdColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#787878"));

                UpdateFreezable("EzBtn_Arrow_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("EzBtn_Arrow_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("EzBtn_Arrow_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                #endregion

                #region SplitItem of PIP/PBP
                UpdateFreezable("BorderColor_SplitNormal", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF0E0E0E"));
                #endregion

                #region SplitItem of EasyArrange
                UpdateFreezable("SplitCtrl_OuterBorder_BdColor_Icon_Normal", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("SplitSplitter_Icon", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("SplitCtrl0B_IconFill", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("SplitItem_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFF0F0F0"));
                #endregion

                #region LockIcon
                UpdateFreezable("LockIcon_Fill", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF0E0E0E"));
                #endregion

                #region Progress Bar
                UpdateFreezable("ProgressBar_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#C5D4E3"));
                UpdateFreezable("Unpair_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                UpdateFreezable("WalkThroughBox_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("EQLine_Color", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#C5D4E3"));
                #endregion

                #region Grid Background
                UpdateFreezable("DefaultTheme_USBKVMPCGridBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A4B8CD"));
                UpdateFreezable("DefaultTheme_GridBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99FFFFFF"));
                #endregion

                #region Setup Border Background
                UpdateFreezable("DefaultTheme_SetupScanBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F0F0F0"));
                UpdateFreezable("DefaultTheme_SetupScanBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("DefaultTheme_SetupSelectBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#C5D4E3"));
                #endregion

                #region Connection Hover UI
                UpdateFreezable("ConnHover_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99FFFFFF"));
                UpdateFreezable("ConnHover_ActiveTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("ConnHover_DeactiveTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D"));
                #endregion Connection Hover UI

                #region DDPM.SA.Common.Popup.PopupBase Robert_Lin, 2024-12-2
                UpdateFreezable("popupBase_Window_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("popupBase_Window_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D8E1EB"));
                UpdateFreezable("popupBase_CloseX_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("popupBase_Header_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("popupBase_Body_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));

                UpdateFreezable<SolidColorBrush>("Dlg_CloseX_HoverBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                #endregion DDPM.SA.Common.Popup.PopupBase

                #region Headset
                UpdateFreezable("ToggleSwitch_NormalBackground", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FBFCFE"));
                UpdateFreezable("ToggleSwitch_NormalBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));
                #endregion

                #region Consent page
                UpdateFreezable("ConsentPage_Background", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D8E5F5"));
                #endregion

                #region HomeDevice LineArt image [Robert_Lin, 2024-11-26]
                UpdateBitmapImage("MonitorImage_LineArt", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Monitors/Lineart-w.png", UriKind.RelativeOrAbsolute));
                #endregion HomeDevice LineArt image
                UpdateBitmapImage("KeyboardImage_LineArt", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/LightMode/Lineart-kb.png", UriKind.RelativeOrAbsolute));
                UpdateBitmapImage("MouseImage_LineArt", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/LightMode/Lineart-ms.png", UriKind.RelativeOrAbsolute));

                UpdateFreezable("DefaultThemeFullPageBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9"));

                #region Webcam
                UpdateFreezable("Webcam_TextBox_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("Webcam_TextBox_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0D121A"));
                UpdateFreezable("Webcam_UndoRedo_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99F8FBFE"));
                UpdateFreezable("Webcam_Countdown321_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                UpdateFreezable("Webcam_Countdown321_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFB6B6B6"));
                #endregion

                #region EzArrange
                UpdateFreezable("EzArrange_ShadowEffect", (ref DropShadowEffect shadow) =>
                {
                    shadow.Color = (Color)ColorConverter.ConvertFromString("#0A000000");
                    shadow.ShadowDepth = 4;
                    shadow.BlurRadius = 10;
                    shadow.Opacity = 0.1;
                    shadow.Direction = 270;
                });
                #endregion


                #region SettingsPlugin.UpdatePage.GroupBorder
                UpdateFreezable("UpdateGroup_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E5ECF9")); //
                UpdateFreezable("UpdateGroup_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00F8FBFE")); //#99F8FBFE
                #endregion SettingsPlugin.UpdatePage.GroupBorder

                #region Add Application Page
                #region SearchBox
                UpdateFreezable("AddAppSearch_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("AddAppSearch_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E1E1E1"));
                UpdateFreezable("AddAppSearch_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                #endregion SearchBox
                #region SortToggleButton
                UpdateFreezable("SortToggleButton_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("SortToggleButton_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D2D2D2"));
                UpdateFreezable("SortToggleButton_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("SortToggleButton_ArrowColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                UpdateFreezable("SortToggleButton_BdColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#31A2E3"));
                #endregion SortToggleButton
                #region AppListView
                UpdateFreezable("AddAppListView_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFF5F6F7"));
                #endregion AppListView
                #endregion Add Application Page

                UpdateBitmapImage("InterruptScreen_Image", new Uri($"pack://application:,,,/DDPM.UI.Common;component/Resources/Background_InterruptScreen_Light.png", UriKind.RelativeOrAbsolute));
                UpdateFreezable("InterruptScreen_Backgroung", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFFF2"));
                UpdateFreezable("Slider_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
                UpdateFreezable("Slider_ValueColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
                #region USBKVM
                UpdateBitmapImage("USBKVM_MK", new Uri($"pack://application:,,,/DDPM.UI.Common;component/Resources/USBKVM_MK_light.png", UriKind.RelativeOrAbsolute));
                UpdateFreezable("USBKVM_MK_Color", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99FFFFFF"));
                UpdateFreezable("USBKVM_MK_WordColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                #endregion

                BitmapImageUpdated?.Invoke(OSThemeEnum.Light);
            }
            catch (Exception ex)
            {
                string log = $"[LightModeSwitch] Exception thrown when applying LightMode : {ex.Message}\nStack Trace: {ex.StackTrace}";
                DdpmCommonHelper.WriteUILog(log);
            }
        }
    }
}
