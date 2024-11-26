using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DDPM.SA.Common.UI
{
    public static partial class SACommonHelper
    {
        public static void SwitchToLightMode()
        {
            UpdateFreezable<SolidColorBrush>("Default_SA_OSD_Border_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E6FFFFFF"));
            UpdateFreezable<SolidColorBrush>("Default_SA_OSD_PathColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#343434"));

            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Path_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Path_IsTrigger_Color", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_BackGround_1", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E7E7E7"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Select_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#F8F8F8"));
            UpdateFreezable<SolidColorBrush>("Button_UXStyleColor_ForWhiteFrame", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0063B8"));
            UpdateFreezable<SolidColorBrush>("PartBorderColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D2D2D2"));
            UpdateFreezable<SolidColorBrush>("Border_Hover_Color", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#94DCF7"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_CameraSetting_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E7E7E7"));
            UpdateFreezable<SolidColorBrush>("Default_SA_QAM_Menu_BackGround", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            UpdateFreezable("TextBlock_ForegroundColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));

            #region EA Dialog Styles
            UpdateFreezable<SolidColorBrush>("Dlg_BdColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#D8E1EB"));
            UpdateFreezable<SolidColorBrush>("Dlg_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#E0E8E9"));
            UpdateFreezable<SolidColorBrush>("Dlg_TextColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));

            UpdateFreezable<SolidColorBrush>("Dlg_CloseX_Default", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#0E0E0E"));
            UpdateFreezable<SolidColorBrush>("Dlg_CloseX_HoverBkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
            #endregion EA Dialog Styles
        }
    }
}
