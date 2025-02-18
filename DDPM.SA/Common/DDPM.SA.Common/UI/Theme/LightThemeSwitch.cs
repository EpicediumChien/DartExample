using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DDPM.SA.Common.UI
{
    public static partial class SAUICommonHelper
    {
        public static void SwitchToLightMode()
        {
            UpdateFreezable<SolidColorBrush>("Default_SA_OSD_Border_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E6FFFFFF"));
            UpdateFreezable<SolidColorBrush>("Default_SA_OSD_PathColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#343434"));

            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Path_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Path_IsTrigger_Color", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#9D9D9D"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_BackGround_1", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E7E7E7"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Select_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F8F8F8"));
            UpdateFreezable<SolidColorBrush>("Button_UXStyleColor_ForWhiteFrame", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
            UpdateFreezable<SolidColorBrush>("PartBorderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D2D2D2"));
            UpdateFreezable<SolidColorBrush>("Border_Hover_Color", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#94DCF7"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_CameraSetting_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E7E7E7"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Menu_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Dragging_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#C5D4E3"));
            UpdateFreezable("TextBlock_ForegroundColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));

            #region EA Dialog Styles
            UpdateFreezable<SolidColorBrush>("Dlg_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D8E1EB"));
            UpdateFreezable<SolidColorBrush>("Dlg_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E0E8E9"));
            UpdateFreezable<SolidColorBrush>("Dlg_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));

            UpdateFreezable<SolidColorBrush>("Dlg_CloseX_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable<SolidColorBrush>("Dlg_CloseX_HoverBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            #endregion EA Dialog Styles

            #region TextBox Robert_Lin, 2024-12-1
            UpdateFreezable("TextBox_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("TextBox_Select", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
            UpdateFreezable("TextBox_TextColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("TextBox_TextColor_Select", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F5F6F7"));
            UpdateFreezable("TextBox_BdColor_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E1E1E1"));
            UpdateFreezable("TextBox_BdColor_Hover", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#CACACA"));
            UpdateFreezable("TextBox_BdColor_Active", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0672CB"));
            UpdateFreezable("TextBox_BdColor_Invalid", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D0353F"));
            #endregion TextBox Robert_Lin, 2024-12-1

            #region Editable ComboBox
            UpdateFreezable("DdpmCB_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("DdpmCB_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D2D2D2"));
            UpdateFreezable("DdpmCB_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("DdpmCB_HoverItemBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF232D36"));
            UpdateFreezable("DdpmCB_HoverItemBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFEEEEEE"));
            UpdateFreezable("DdpmCB_SelTextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FF49A3E5"));
            #endregion Editable ComboBox

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
            #endregion

            #region DDPM.SA.Common.Popup.PopupBase Robert_Lin, 2024-12-2
            UpdateFreezable("popupBase_Window_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("popupBase_Window_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D8E1EB"));
            UpdateFreezable("popupBase_CloseX_FgColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("popupBase_Header_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable("popupBase_Body_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            #endregion DDPM.SA.Common.Popup.PopupBase

            #region Tooltip
            UpdateFreezable("Tooltip_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("Tooltip_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E9E9E9"));
            UpdateFreezable("Tooltip_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#000000"));
            #endregion

            #region QAM Page
            UpdateBitmapImage("QAMImage_CallDDPM", new Uri($"pack://application:,,,/DDPM.QAM;component/Imgs/ddpm-light.png", UriKind.RelativeOrAbsolute));
            #endregion
        }
    }
}
