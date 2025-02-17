using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GenDdpmSplashImages
{
    internal class MainViewModel : ObservableObject
    {
        #region Figma Parameters
        private const double ImageWidth_Figma = 553;
        private const double ImageHeight_Figma = 392;
        private const double LogoSize_Figma = 100;
        private const double TextHeight_Figma = 24;
        private const double BottomMargin_Figma = 15;
        //Calculated
        private const double FooterHeight_Figma = TextHeight_Figma + BottomMargin_Figma;
        private const double ImageWidthHeightRatio = ImageWidth_Figma / ImageHeight_Figma;
        #endregion

        #region Standard Splash Image
        private const double ImageWidth_Std = 1268.000;
        private const double ImageHeight_Std = 693.000;
        #endregion

        #region 4K Splash Image
        private const double ImageWidth_4k = 2534.000;
        private const double ImageHeight_4k = 1383.000;

        #endregion

        #region Calculated Image Properties
        //Input : ImageWidth and ImageHeight
        private double _imageWidth = ImageWidth_Std;
        private double _imageHeight = ImageHeight_Std;
        private double _xScale = 2.293;

        //Please set ImageHeight first, then set ImageWidth
        public double ImageWidth
        {
            get => _imageWidth;
            set
            {
                SetProperty(ref _imageWidth, value);
                _xScale = value / ImageWidth_Figma;
                OnPropertyChanged("LogoSize");
                OnPropertyChanged("TextHeight");
                OnPropertyChanged("BottomMargin");
                OnPropertyChanged("FooterHeight");
            }
        }

        public double ImageHeight
        {
            get => _imageHeight;
            set => SetProperty(ref _imageHeight, value);
        }

        public double LogoSize => LogoSize_Figma * _xScale;
        public double TextHeight => TextHeight_Figma * _xScale;
        public double BottomMarginValue => BottomMargin_Figma * _xScale;
        public Thickness BottomMargin
        {
            get
            {
                double bottom = BottomMargin_Figma * _xScale;
                return new Thickness(0,0, 0, bottom);
            }
        }
        public double FooterHeight => FooterHeight_Figma * _xScale;
        #endregion

        #region Text Properties
        private string _version = "2.0.0";
        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        private string _build = "2.0.0.29";
        public string Build
        {
            get => _build;
            set => SetProperty(ref _build, value);
        }

        private string _year = "2024";
        public string Year
        {
            get => _year;
            set => SetProperty(ref _year, value);
        }
        #endregion
        public void ChangeTo4KImage()
        {
            ImageHeight = ImageHeight_4k;
            ImageWidth = ImageWidth_4k;
        }
    }
}
