using Dell.Client.Framework.UX.WPF.Controls;
using System.Resources;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace DDPM.UI.Common
{
    public static partial class DdpmCommonHelper
    {
        /// <summary>
        /// Switch to dark mode color define
        /// </summary>
        /// <history>
        /// Add SplitListView - EzBtn (Robert_Lin, 2024-10-24)
        /// Add regions and sort functions (Eric Chien, 2024-10-30)
        /// </history>
        public static void SwitchToDarkMode()
        {
            try
            {
                #region Background Color
                UpdateFreezable("DDPMBackground", (ref LinearGradientBrush brush) =>
                {
                    GradientStop? gs4 = brush.GradientStops.FirstOrDefault(gs => gs.Offset == 0.7923);
                    if (gs4 != null)
                        gs4.Color = (Color)ColorConverter.ConvertFromString("#7F674AB8");
                });
                #endregion

                #region Default Text Color
                UpdateFreezable("DefaultTheme_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                UpdateFreezable("DefaultTheme_ProcessBarTxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A4B8CD"));
                UpdateFreezable("DefaultTheme_BdBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99132F54"));
                UpdateFreezable("DefaultTheme_BdSolidBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF132F54"));
                UpdateFreezable("DefaultTheme_BdBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#132F54"));
                UpdateFreezable("DefaultTheme_BdSolidBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1B385F"));
                UpdateFreezable("DefaultTheme_RbBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#800A0E14"));
                UpdateFreezable("DefaultTheme_PathColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("DefaultTheme_PbHeaderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#DAF5FD"));
                UpdateFreezable("DefaultTheme_USBKVMPCColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A4B8CD"));
                UpdateFreezable("DefaultTheme_USBKVMLineColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1B385F"));
                UpdateFreezable("DefaultTheme_BtFgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFF5F6F7"));
                UpdateFreezable("DefaultTheme_BtBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF0E1C2F"));
                UpdateFreezable("DefaultTheme_BtBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                UpdateFreezable("DefaultTheme_ComboBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#293B4D"));
                UpdateFreezable("DefaultTheme_ComboBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D"));
                UpdateBitmapImage("popup_ArrowCorner", new Uri($"pack://application:,,,/DDPM.UI.Common;component/Resources/popup_ArrowCorner.png", UriKind.RelativeOrAbsolute));
                UpdateFreezable("DefaultTheme_TbBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
                UpdateFreezable("DefaultTheme_TbFgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                UpdateFreezable("DefaultTheme_TbBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7E7E7E"));
                UpdateFreezable("DefaultTheme_ListBoxBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#80141D27"));
                UpdateFreezable("DefaultTheme_DialogBoxBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#132F54"));
                UpdateFreezable("DefaultThemeRightViewAlertBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#364861"));
                #endregion

                #region Add Device
                UpdateFreezable("RightMenuBg_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99132F54"));
                UpdateFreezable("txtCaption_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("ActionsTextBlock_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("ActionsBorder_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99132F54"));
                UpdateFreezable("ActionsRadio_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
                UpdateFreezable("ActionsRadio_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("ActionsRadio_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("SearchBoxBorder_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0F2641"));
                UpdateFreezable("SearchTextBox_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0F2641"));
                UpdateFreezable("SearchTextBox_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("SeachResultBorder_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99132F54"));
                UpdateFreezable("ModelNumberText_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A4B8CD"));
                UpdateFreezable("KeyToolTipText_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1D2C3B"));
                UpdateFreezable("KeyToolTipText_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("ActionParameterModalDialog_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0D121A"));
                UpdateFreezable("CaptionText_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A4B8CD"));
                UpdateFreezable("DescriptionText_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A4B8CD"));
                UpdateFreezable("Keystroke_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
                UpdateFreezable("Keystroke_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("txtOpen_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
                UpdateFreezable("txtOpen_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("CancelButton_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0D121A"));
                UpdateFreezable("CancelButton_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("CancelButtonBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#818488"));
                UpdateFreezable("SaveButtonDisa_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#333941"));
                UpdateFreezable("SaveButtonDisa_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#818488"));
                UpdateFreezable("txtActionCaption_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A4B8CD"));
                UpdateBitmapImage("title_back", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/arrow-left.png", UriKind.RelativeOrAbsolute));
                UpdateBitmapImage("mouse_setting_title_back", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/arrow-left.png", UriKind.RelativeOrAbsolute));
                UpdateBitmapImage("down_Expand", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/Down.png", UriKind.RelativeOrAbsolute));
                UpdateFreezable("txtMessage_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#ffffff"));
                UpdateFreezable("MouseSettingMenu_BgColor99", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99132F54"));
                UpdateFreezable("MouseSettingMenu_BgColor60", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#60132F54"));
                UpdateFreezable("MenuSeperatorColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1E3F6C"));

                UpdateFreezable("AddDeviceTxtCaption_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("AddDeviceBorder1_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99132F54"));
                UpdateFreezable("AddDeviceTextStyle1_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));

                UpdateFreezable("RightView_Header_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0A2343"));
                UpdateFreezable("RightView_Header_Text_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("RightView_Header_Text_HighlightColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));

                UpdateFreezable("bdrAlert_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#364861"));

                UpdateFreezable("bdrAlert_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));


                UpdateFreezable("ModuleCaption_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("ModuleItem_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));

                UpdateFreezable("txtUnpair_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));

                UpdateBitmapImage("Search_img", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/Search.png", UriKind.RelativeOrAbsolute));
                UpdateBitmapImage("Close_img", new Uri($"pack://application:,,,/DDPM.UI.Common;component/Resources/Close.png", UriKind.RelativeOrAbsolute));

                UpdateFreezable("ActionParameterModalDialogTextBoxBorderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7E7E7E"));

                UpdateFreezable("ActionsBorderBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1E3F6C"));

                UpdateFreezable("Border1BorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#132F54"));

                UpdateFreezable("SearchBoxBorderBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1E3F6C"));

                UpdateBitmapImage("left_img", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/Left.png", UriKind.RelativeOrAbsolute));
                #endregion

                #region dialog
                //Dialog start
                UpdateFreezable("BorderStyle1_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99132F54"));

                UpdateFreezable("TextBlockStyle1_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));

                UpdateFreezable("LabelStyle1_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#CCCCCC"));

                UpdateFreezable("RadialMenuModalDialogToggleSwitch_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));

                UpdateFreezable("txtLabelText_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
                UpdateFreezable("txtLabelText_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));

                UpdateFreezable("PairTileModalDialogTextStyle1FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#CFD2D3"));

                UpdateFreezable("PairTileModalDialogBgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0D121A"));

                UpdateFreezable("UnpairModalDialogBorder1", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E00C1827"));

                UpdateFreezable("BorderContinueBackground", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E1C2F"));
                UpdateFreezable("BorderContinueBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));

                UpdateFreezable("BorderYesBackground", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E1C2F"));
                UpdateFreezable("BorderYesBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));

                UpdateFreezable("txtAlert_BgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#364861"));
                UpdateFreezable("txtAlert_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));


                UpdateFreezable("WaitingModalDialogBorderBackground", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E00B172C"));
                UpdateFreezable("WaitingModalDialogBorderBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#293B4D"));


                UpdateFreezable("bdrAlertBackground", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#364861"));
                UpdateFreezable("WaitingModalDialogBorderBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E67F01"));
                //Dialog end
                #endregion



                #region Main Window Background
                SplashPath = "Resources/Images/splash{0}-round_Dark.png";
                UpdateFreezable("mainWindowBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF0D121A"));
                UpdateFreezable("bkImage", (ref ImageBrush brush) =>
                {
                    brush.Stretch = Stretch.Fill;
                    brush.ImageSource = GetImageSourceFromCommonResource("Resources/Images/Background.png", "DDPM");
                });
                UpdateFreezable("SpinnerWaitTxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("SpinnerBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF0D121A"));
                #endregion

                #region Add Device
                #endregion

                #region PathIcon
                UpdateFreezable("PathIcon_Color", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("FakeTransparent", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#02000000"));
                UpdateFreezable("MouseHover_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#33444E"));
                UpdateFreezable("Window_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                #endregion

                #region HomePage
                UpdateFreezable("HomePageFeature_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF97DCF4"));
                UpdateFreezable("HomePageKeyFeature_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("HomePageButton_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
                UpdateFreezable("HomePageButton_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                #endregion

                #region Battery
                UpdateFreezable("BatteryIndicator_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99193457"));
                UpdateFreezable("TextBlock_ForegroundColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                #endregion

                #region DeviceBasePage
                UpdateFreezable("DisplayName_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateBitmapImage("ConnectionTypeImageKey", new Uri("pack://application:,,,/DDPM.UI.Common;component/Resources/Port.png", UriKind.Absolute));
                #endregion

                #region Combo Box
                UpdateFreezable("DdpmCB_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF40586D"));
                UpdateFreezable("DdpmCB_BdBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF232D36"));
                UpdateFreezable("DdpmCB_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF2F3B4D"));
                UpdateFreezable("DdpmCB_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("DdpmCB_SelTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#8BCBEF"));
                UpdateFreezable("MouseCB_HIBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
                UpdateFreezable("DdpmCB_BdColor_Checked", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#31A2E3"));
                UpdateFreezable("DdpmCB_FgColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#596067"));
                #endregion

                #region BarItem Colors
                UpdateBitmapImage("Arrow_Left", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/arrow-left.png", UriKind.RelativeOrAbsolute));
                UpdateFreezable("btn_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E1C2F"));
                #endregion

                #region VbarItem
                System.Windows.Application.Current.Resources["Vbar_BdColor_Hover"] = Color.FromArgb(0xFF, 0x1B, 0x38, 0x5F);
                System.Windows.Application.Current.Resources["Vbar_BkColor_Hover"] = Color.FromArgb(0x00, 0x0E, 0x0E, 0x0E);
                UpdateFreezable("Vbar_BkBrush_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF1B385F"));
                #endregion

                #region VbarItem1
                //Robert_Lin 20250312 Change from #FF232D36 to #213242
                UpdateFreezable("DdpmCB_HoverItemBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#213242"));
                UpdateFreezable("DefaultTheme_USBKVMComboBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99204A82"));
                UpdateFreezable("DefaultTheme_USBKVMComboBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1B385F"));
                UpdateFreezable("Vbar_FgBrush_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("Vbar_BkBrush_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99132F54"));
                UpdateFreezable("Vbar_BdBrush_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99132F54"));
                UpdateFreezable("Vbar_FgBrush_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("Vbar_TextColor_Normal", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("Vbar_TextColor_Selected", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                #endregion

                #region Right View Container
                UpdateFreezable("RightView_Header_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0A2343"));
                UpdateFreezable("RightView_Header_Text_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("RightViewContainerBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#9C193457"));
                UpdateFreezable("GroupExpanderHeaderBottomLineColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF1E3F6C"));
                UpdateFreezable("RightView_Group_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99132F53"));
                UpdateFreezable("RightView_Group_FirstBdBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF102A4D"));
                UpdateFreezable("RightView_Group_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF1B385F"));
                UpdateFreezable("RightView_Combo_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("RightView_Combo_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#293B4D"));
                UpdateFreezable("RightView_Combo_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D"));
                UpdateFreezable("RightBaseBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF0D2443"));
                UpdateFreezable("RightView_PercentageTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A4B8CD"));
                UpdateFreezable("Brightness_TimeText_ForegroundColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A4B8CD"));
                UpdateFreezable("Brightness_TimeAMPM_ForegroundColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("RightView_Btn_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("RightView_Btn_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("RightGroupBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99194784"));
                UpdateFreezable("GroupBorderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99132F54"));
                UpdateFreezable("PartBorderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#132F54"));
                #endregion

                #region Tooltip
                UpdateFreezable("Tooltip_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF1D2C3B"));
                UpdateFreezable("Tooltip_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF293B4D"));
                UpdateFreezable("Tooltip_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFF5F6F7"));
                #endregion

                #region (i) InfoIcon
                UpdateFreezable("InfoIconColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#B6B6B6"));
                #endregion

                #region Button
                UpdateFreezable("Border_Btn_txt", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#132F54"));
                UpdateFreezable("CheckBox_Brush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7E7E7E"));
                UpdateFreezable("BorderBrushColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#132F54"));
                UpdateFreezable("Button_UXStyleColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
                UpdateFreezable("Button_UXStyleTxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("Button_UXStyleColor_ForWhiteFrame", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("Button1BorderDisColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#344354"));
                UpdateFreezable("Button1TextDisColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#828E9C"));
                #endregion

                #region PrimaryButton
                UpdateFreezable("PrimaryButton_BkColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
                UpdateFreezable("PrimaryButton_BkColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
                UpdateFreezable("PrimaryButton_BkColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00468B"));
                UpdateFreezable("PrimaryButton_BkColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7F2E3034"));

                UpdateFreezable("PrimaryButton_TextColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("PrimaryButton_TextColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("PrimaryButton_TextColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("PrimaryButton_TextColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7F7C7C7C"));
                #endregion PrimaryButton


                #region SecondaryButton
                UpdateFreezable("SecondaryButton_BkColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#01FFFFFF"));
                UpdateFreezable("SecondaryButton_BkColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1D2C3B"));
                UpdateFreezable("SecondaryButton_BkColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#293B4D"));
                UpdateFreezable("SecondaryButton_BkColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#01FFFFFF"));

                UpdateFreezable("SecondaryButton_BdColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("SecondaryButton_BdColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("SecondaryButton_BdColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("SecondaryButton_BdColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A3A3A3"));

                UpdateFreezable("SecondaryButton_TextColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("SecondaryButton_TextColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("SecondaryButton_TextColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("SecondaryButton_TextColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D"));

                //UpdateFreezable("SecondaryButton_DefaultBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#01FFFFFF"));
                //UpdateFreezable("SecondaryButton_DefaultBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                //UpdateFreezable("SecondaryButton_DefaultTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));

                //UpdateFreezable("SecondaryButton_HoverBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF1D2C3B"));
                //UpdateFreezable("SecondaryButton_HoverBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                //UpdateFreezable("SecondaryButton_HoverTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));

                //UpdateFreezable("SecondaryButton_PressedBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF293B4D"));
                //UpdateFreezable("SecondaryButton_PressedBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                //UpdateFreezable("SecondaryButton_PressedTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));

                //UpdateFreezable("SecondaryButton_DisabledBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#01FFFFFF"));
                //UpdateFreezable("SecondaryButton_DisabledBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A3A3A3"));
                //UpdateFreezable("SecondaryButton_DisabledTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D"));
                #endregion

                #region DestructiveButton
                UpdateFreezable("DestructiveButton_BkColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D0353F"));
                UpdateFreezable("DestructiveButton_BkColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF3E3B"));
                UpdateFreezable("DestructiveButton_BkColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D12A3D"));
                UpdateFreezable("DestructiveButton_BkColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7E2E3034"));

                UpdateFreezable("DestructiveButton_TextColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("DestructiveButton_TextColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F4FFFF"));
                UpdateFreezable("DestructiveButton_TextColor_Pressed", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("DestructiveButton_TextColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7F7C7C7C"));
                #endregion DestructiveButton

                #region SplitListView - EzBtn
                UpdateFreezable("EzBtn_BkColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF193457"));
                UpdateFreezable("EzBtn_BkColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF596067"));

                UpdateFreezable("EzBtn_BdColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF132F54"));
                UpdateFreezable("EzBtn_BdColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF132F54"));
                UpdateFreezable("EzBtn_BdColor_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF132F54"));

                UpdateFreezable("EzBtn_Arrow_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("EzBtn_Arrow_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("EzBtn_Arrow_Disabled", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFF5F6F7"));
                #endregion

                #region SplitItem of EasyArrange
                UpdateFreezable("SplitCtrl_OuterBorder_BdColor_Icon_Normal", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                UpdateFreezable("SplitSplitter_Icon", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                UpdateFreezable("SplitCtrl0B_IconFill", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF132F54"));
                //Robert_Lin 20250312 Remove duplicate (with Line #228) item
                //UpdateFreezable("DdpmCB_HoverItemBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF232D36"));
                UpdateFreezable("SplitItem_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF132F54"));
                #endregion

                #region LockIcon
                UpdateFreezable("LockIcon_Fill", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFFFF"));
                #endregion

                #region Progress Bar
                UpdateFreezable("ProgressBar_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#293B4D"));
                UpdateFreezable("Unpair_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("WalkThroughBox_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0B172D"));
                UpdateFreezable("EQLine_Color", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1B385F"));
                #endregion

                #region Grid Background
                UpdateFreezable("DefaultTheme_USBKVMPCGridBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99193457"));
                UpdateFreezable("DefaultTheme_GridBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99132F54"));
                #endregion

                #region Setup Border Background
                UpdateFreezable("DefaultTheme_SetupScanBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#193457"));
                UpdateFreezable("DefaultTheme_SetupScanBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("DefaultTheme_SetupSelectBdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1B385F"));
                #endregion

                #region Connection Hover UI
                UpdateFreezable("ConnHover_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99193457"));
                UpdateFreezable("ConnHover_ActiveTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("ConnHover_DeactiveTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#839DB4"));//839DB4
                #endregion Connection Hover UI

                #region DDPM.SA.Common.Popup.PopupBase Robert_Lin, 2024-12-2
                UpdateFreezable("popupBase_Window_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F20D121A"));
                UpdateFreezable("popupBase_Window_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#293B4D"));
                UpdateFreezable("popupBase_CloseX_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#CCCCCC"));
                UpdateFreezable("popupBase_Header_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("popupBase_Body_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));

                UpdateFreezable<SolidColorBrush>("Dlg_CloseX_HoverBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#33444E"));
                #endregion DDPM.SA.Common.Popup.PopupBase

                #region Headset
                UpdateFreezable("ToggleSwitch_NormalBackground", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0B1E38"));
                UpdateFreezable("ToggleSwitch_NormalBorderBrush", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#1E3F6C"));
                #endregion

                #region Consent page
                UpdateFreezable("ConsentPage_Background", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#13172A"));
                #endregion

                #region HomeDevice LineArt image [Robert_Lin, 2024-11-26]
                UpdateBitmapImage("MonitorImage_LineArt", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Monitors/Lineart.png", UriKind.RelativeOrAbsolute));
                #endregion HomeDevice LineArt image
                UpdateBitmapImage("KeyboardImage_LineArt", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/Lineart-kb.png", UriKind.RelativeOrAbsolute));
                UpdateBitmapImage("MouseImage_LineArt", new Uri($"pack://application:,,,/DDPM.UI.Resources;component/Resources/Images/Lineart-ms.png", UriKind.RelativeOrAbsolute));

                UpdateFreezable("DefaultThemeFullPageBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF0D121A"));

                #region Webcam
                UpdateFreezable("Webcam_TextBox_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
                UpdateFreezable("Webcam_TextBox_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7E7E7E"));
                UpdateFreezable("Webcam_UndoRedo_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99193457"));
                UpdateFreezable("Webcam_Countdown321_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF132F54"));
                UpdateFreezable("Webcam_Countdown321_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF081A33"));
                #endregion

                #region EzArrange
                UpdateFreezable("EzArrange_ShadowEffect", (ref DropShadowEffect shadow) =>
                {
                    shadow.Color = (Color)ColorConverter.ConvertFromString("#7F000000");
                    shadow.ShadowDepth = 0;
                    shadow.BlurRadius = 12;
                    shadow.Opacity = 1;
                    shadow.Direction = 0;
                });
                #endregion


                #region SettingsPlugin.UpdatePage.GroupBorder
                UpdateFreezable("UpdateGroup_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#00000000"));
                UpdateFreezable("UpdateGroup_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99132F54"));
                #endregion SettingsPlugin.UpdatePage.GroupBorder

                #region Add Application Page
                #region SearchBox
                UpdateFreezable("AddAppSearch_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
                UpdateFreezable("AddAppSearch_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#7E7E7E"));
                UpdateFreezable("AddAppSearch_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                #endregion SearchBox
                #region SortToggleButton
                UpdateFreezable("SortToggleButton_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#293B4D"));
                UpdateFreezable("SortToggleButton_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#40586D"));
                UpdateFreezable("SortToggleButton_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
                UpdateFreezable("SortToggleButton_ArrowColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
                UpdateFreezable("SortToggleButton_BdColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#31A2E3"));
                #endregion SortToggleButton
                #region AppListView
                UpdateFreezable("AddAppListView_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#99132F54"));
                #endregion AppListView
                #endregion Add Application Page


                UpdateBitmapImage("InterruptScreen_Image", new Uri($"pack://application:,,,/DDPM.UI.Common;component/Resources/Background_InterruptScreen.png", UriKind.RelativeOrAbsolute));
                UpdateFreezable("InterruptScreen_Backgroung", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#132F54"));
                UpdateFreezable("Slider_TxtColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A4B8CD"));
                UpdateFreezable("Slider_ValueColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A4B8CD"));
                #region USBKVM
                UpdateBitmapImage("USBKVM_MK", new Uri($"pack://application:,,,/DDPM.UI.Common;component/Resources/USBKVM_MK_Dark.png", UriKind.RelativeOrAbsolute));
                UpdateFreezable("USBKVM_MK_Color", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#19345799"));
                UpdateFreezable("USBKVM_MK_WordColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#A4B8CD"));
                #endregion

                BitmapImageUpdated?.Invoke(OSThemeEnum.Dark);
            }
            catch (Exception ex)
            {
                string log = $"[LightModeSwitch] Exception thrown when applying DarkMode : {ex.Message}\nStack Trace: {ex.StackTrace}";
                DdpmCommonHelper.WriteUILog(log);
            }
        }
    }
}
